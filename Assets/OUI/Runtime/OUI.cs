using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OUI
{
    /// <summary>
    /// OUI UI管理系统
    /// </summary>
    public class OUI : MonoBehaviour
    {
        public const int LAYER_DEEP = 2000;
        public const int WINDOW_DEEP = 100;

        private static OUI _instance;
        public static OUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("OUI");
                    _instance = go.AddComponent<OUI>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 资源加载器，由外部注入
        /// </summary>
        public IWindowAssetLoader AssetLoader;

        private Transform _uiRoot;
        private readonly List<BaseWindow> _stack = new List<BaseWindow>();
        private readonly HashSet<Type> _loadingWindows = new HashSet<Type>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _uiRoot = transform;
        }

        public async UniTask ShowUIAsync<T>() where T : BaseWindow
        {
            await ShowUIAsync<T>(Array.Empty<object>());
        }

        /// <summary>
        /// 异步打开窗口
        /// </summary>
        /// <param name="args">传递给OnOpen的可变参数</param>
        public async UniTask ShowUIAsync<T>(params object[] args) where T : BaseWindow
        {
            Type type = typeof(T);

            // 检查窗口是否已打开
            BaseWindow existingWindow = GetWindow<T>();
            if (existingWindow != null && !existingWindow.IsClosed)
            {
                return;
            }

            // 检查是否正在加载中
            if (_loadingWindows.Contains(type))
            {
                return;
            }

            // 窗口已存在但已关闭，重新打开
            if (existingWindow != null && existingWindow.IsClosed)
            {
                ReopenWindow(existingWindow, args);
                return;
            }

            // 首次打开，加载资源
            await LoadAndCreateWindow<T>(type, args);
        }

        /// <summary>
        /// 加载并创建窗口
        /// </summary>
        private async UniTask LoadAndCreateWindow<T>(Type type, object[] args) where T : BaseWindow
        {
            _loadingWindows.Add(type);

            try
            {
                // 读取WindowAttribute
                WindowAttribute attr = type.GetCustomAttributes(typeof(WindowAttribute), false)[0] as WindowAttribute;
                if (attr == null)
                {
                    throw new Exception($"Window {type.Name} 缺少 [WindowAttribute]");
                }

                if (AssetLoader == null)
                {
                    throw new Exception("OUI.AssetLoader 未设置");
                }

                // 加载资源
                GameObject prefab = await AssetLoader.LoadAsync(attr.AssetPath);
                GameObject go = Instantiate(prefab, _uiRoot);

                // 获取或添加BaseWindow组件
                BaseWindow window = go.GetComponent<BaseWindow>();
                if (window == null)
                {
                    window = go.AddComponent(type) as BaseWindow;
                    if (window == null)
                    {
                        throw new Exception($"无法为 Window Prefab {attr.AssetPath} 添加 {type.Name} 组件");
                    }
                }

                // 初始化窗口
                window.WindowLayer = attr.Layer;
                window.HideBelow = attr.HideBelow;
                window.InternalInit();

                // 加入栈
                Push(window);
                OnSortWindowDepth(attr.Layer);

                // 生命周期回调
                window.BindUI();
                window.Init();

                // 打开窗口
                window.InternalOpen();
                window.OnOpen(args);

                OnSetWindowVisible();
            }
            finally
            {
                _loadingWindows.Remove(type);
            }
        }

        /// <summary>
        /// 重新打开已存在的窗口
        /// </summary>
        private void ReopenWindow(BaseWindow window, object[] args)
        {
            Pop(window);
            Push(window);
            OnSortWindowDepth(window.WindowLayer);
            window.InternalOpen();
            window.OnOpen(args);
            OnSetWindowVisible();
        }

        /// <summary>
        /// 关闭指定类型的窗口
        /// </summary>
        public void CloseWindow<T>() where T : BaseWindow
        {
            CloseWindow(typeof(T));
        }

        /// <summary>
        /// 关闭指定类型的窗口
        /// </summary>
        public void CloseWindow(Type type)
        {
            BaseWindow window = GetWindow(type);
            if (window == null)
            {
                throw new Exception($"Window {type.Name} 不存在");
            }

            window.OnClose();
            window.InternalClose();
            OnSetWindowVisible();
        }

        /// <summary>
        /// 销毁指定类型的窗口
        /// </summary>
        public void DestroyWindow<T>() where T : BaseWindow
        {
            DestroyWindow(typeof(T));
        }

        /// <summary>
        /// 销毁指定类型的窗口
        /// </summary>
        public void DestroyWindow(Type type)
        {
            BaseWindow window = GetWindow(type);
            if (window == null)
            {
                throw new Exception($"Window {type.Name} 不存在");
            }

            // 如果未关闭，先关闭
            if (!window.IsClosed)
            {
                window.OnClose();
                window.InternalClose();
            }

            // 释放资源
            window.Release();

            // 从栈中移除
            WindowLayer layer = window.WindowLayer;
            Pop(window);

            // 销毁GameObject
            if (AssetLoader != null)
            {
                AssetLoader.Unload(window.gameObject);
            }
            Destroy(window.gameObject);

            OnSortWindowDepth(layer);
            OnSetWindowVisible();
        }

        /// <summary>
        /// 获取已存在的窗口实例
        /// </summary>
        public T GetWindow<T>() where T : BaseWindow
        {
            return GetWindow(typeof(T)) as T;
        }

        /// <summary>
        /// 获取已存在的窗口实例
        /// </summary>
        public BaseWindow GetWindow(Type type)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].GetType() == type)
                {
                    return _stack[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 关闭所有窗口（隐藏，不销毁）
        /// </summary>
        public void CloseAllWindows()
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                if (!_stack[i].IsClosed)
                {
                    _stack[i].OnClose();
                    _stack[i].InternalClose();
                }
            }
            OnSetWindowVisible();
        }

        /// <summary>
        /// 获取所有窗口列表（只读）
        /// </summary>
        public List<BaseWindow> GetAllWindows()
        {
            return new List<BaseWindow>(_stack);
        }

        /// <summary>
        /// 销毁所有窗口（真正销毁）
        /// </summary>
        public void DestroyAll()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                BaseWindow window = _stack[i];

                // 如果未关闭，先关闭
                if (!window.IsClosed)
                {
                    window.OnClose();
                    window.InternalClose();
                }

                // 释放资源
                window.Release();

                // 销毁GameObject
                if (AssetLoader != null)
                {
                    AssetLoader.Unload(window.gameObject);
                }
                Destroy(window.gameObject);
            }

            _stack.Clear();
        }

        /// <summary>
        /// 将窗口插入栈中
        /// </summary>
        private void Push(BaseWindow window)
        {
            int insertIndex = -1;

            // 找到同Layer的最后一个窗口，插入其后面
            for (int i = 0; i < _stack.Count; i++)
            {
                if (window.WindowLayer == _stack[i].WindowLayer)
                    insertIndex = i + 1;
            }

            // 如果该Layer没有窗口，找相邻的更低Layer的最后一个窗口，插入其后面
            if (insertIndex == -1)
            {
                for (int i = 0; i < _stack.Count; i++)
                {
                    if (window.WindowLayer > _stack[i].WindowLayer)
                        insertIndex = i + 1;
                }
            }

            // 栈为空或该Layer最低，插入到索引0
            if (insertIndex == -1)
                insertIndex = 0;

            _stack.Insert(insertIndex, window);
        }

        /// <summary>
        /// 从栈中移除窗口
        /// </summary>
        private void Pop(BaseWindow window)
        {
            _stack.Remove(window);
        }

        /// <summary>
        /// 重新计算指定Layer的所有窗口的sortingOrder
        /// </summary>
        private void OnSortWindowDepth(WindowLayer layer)
        {
            int depth = (int)layer * LAYER_DEEP;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].WindowLayer == layer)
                {
                    _stack[i].Depth = depth;
                    depth += WINDOW_DEEP;
                }
            }
        }

        /// <summary>
        /// 重新计算所有窗口的HideBelow可见性
        /// </summary>
        private void OnSetWindowVisible()
        {
            bool isHideNext = false;
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                var window = _stack[i];
                if (window.IsClosed) continue;

                if (!isHideNext)
                {
                    window.SetHideByAbove(false);
                    if (window.HideBelow)
                        isHideNext = true;
                }
                else
                {
                    window.SetHideByAbove(true);
                }
            }
        }
    }
}
