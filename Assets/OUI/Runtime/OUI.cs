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
        private readonly Dictionary<Type, Queue<object[]>> _pendingShowQueues = new Dictionary<Type, Queue<object[]>>();
        private readonly Dictionary<Type, WindowAttribute> _windowAttributeCache = new Dictionary<Type, WindowAttribute>();
        private bool _suppressQueuedShowDrain;

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
            await ShowUIAsync(typeof(T), Array.Empty<object>());
        }

        /// <summary>
        /// 异步打开窗口
        /// </summary>
        /// <param name="args">传递给OnOpen的可变参数</param>
        public async UniTask ShowUIAsync<T>(params object[] args) where T : BaseWindow
        {
            await ShowUIAsync(typeof(T), args);
        }

        /// <summary>
        /// 内部统一的打开入口，支持按Type处理自动续开。
        /// </summary>
        private async UniTask ShowUIAsync(Type type, object[] args)
        {
            WindowAttribute attr = GetWindowAttribute(type);
            object[] argsSnapshot = SnapshotArgs(args);
            BaseWindow existingWindow = GetWindow(type);

            if (existingWindow != null)
            {
                if (existingWindow.IsClosed)
                {
                    ReopenWindow(existingWindow, argsSnapshot);
                }
                else
                {
                    HandleDuplicateShow(type, attr, argsSnapshot);
                }
                return;
            }

            if (_loadingWindows.Contains(type))
            {
                HandleDuplicateShow(type, attr, argsSnapshot);
                return;
            }

            await LoadAndCreateWindow(type, attr, argsSnapshot);
        }

        /// <summary>
        /// 加载并创建窗口
        /// </summary>
        private async UniTask LoadAndCreateWindow(Type type, WindowAttribute attr, object[] args)
        {
            GameObject instance = null;
            BaseWindow window = null;
            bool addedToStack = false;

            _loadingWindows.Add(type);

            try
            {
                if (AssetLoader == null)
                {
                    throw new Exception("OUI.AssetLoader 未设置");
                }

                GameObject prefab = await AssetLoader.LoadAsync(attr.AssetPath);
                if (prefab == null)
                {
                    throw new Exception($"Window {type.Name} 加载到的 Prefab 为空: {attr.AssetPath}");
                }

                instance = Instantiate(prefab, _uiRoot);

                window = instance.GetComponent<BaseWindow>();
                if (window == null)
                {
                    window = instance.AddComponent(type) as BaseWindow;
                    if (window == null)
                    {
                        throw new Exception($"无法为 Window Prefab {attr.AssetPath} 添加 {type.Name} 组件");
                    }
                }

                window.WindowLayer = attr.Layer;
                window.HideBelow = attr.HideBelow;
                window.InternalInit();

                Push(window);
                addedToStack = true;
                OnSortWindowDepth(attr.Layer);

                window.BindUI();
                window.Init();
                window.InternalOpen();
                window.OnOpen(args);

                OnSetWindowVisible();
            }
            catch (Exception ex)
            {
                if (addedToStack && window != null)
                {
                    Pop(window);
                    OnSortWindowDepth(attr.Layer);
                    OnSetWindowVisible();
                }

                if (instance != null)
                {
                    if (AssetLoader != null)
                    {
                        AssetLoader.Unload(instance);
                    }
                    Destroy(instance);
                }

                ClearPendingShows(type);
                Debug.LogError($"[OUI] 打开窗口 {type.Name} 失败，已清空待打开队列。\n{ex}");
                throw;
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

            CloseWindowInternal(window, true);
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

            ClearPendingShows(type);

            bool previousSuppress = _suppressQueuedShowDrain;
            _suppressQueuedShowDrain = true;

            try
            {
                WindowLayer layer = window.WindowLayer;

                if (!window.IsClosed)
                {
                    window.OnClose();
                    window.InternalClose();
                }

                window.Release();
                Pop(window);

                if (AssetLoader != null)
                {
                    AssetLoader.Unload(window.gameObject);
                }
                Destroy(window.gameObject);

                OnSortWindowDepth(layer);
                OnSetWindowVisible();
            }
            finally
            {
                _suppressQueuedShowDrain = previousSuppress;
            }
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
            bool previousSuppress = _suppressQueuedShowDrain;
            _suppressQueuedShowDrain = true;

            try
            {
                for (int i = 0; i < _stack.Count; i++)
                {
                    if (!_stack[i].IsClosed)
                    {
                        _stack[i].OnClose();
                        _stack[i].InternalClose();
                    }
                }

                // 批量关闭后视为取消所有待续开的请求，避免出现旧请求延后自动弹出。
                ClearAllPendingShows();
                OnSetWindowVisible();
            }
            finally
            {
                _suppressQueuedShowDrain = previousSuppress;
            }
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
            bool previousSuppress = _suppressQueuedShowDrain;
            _suppressQueuedShowDrain = true;

            try
            {
                ClearAllPendingShows();

                for (int i = _stack.Count - 1; i >= 0; i--)
                {
                    BaseWindow window = _stack[i];

                    if (!window.IsClosed)
                    {
                        window.OnClose();
                        window.InternalClose();
                    }

                    window.Release();

                    if (AssetLoader != null)
                    {
                        AssetLoader.Unload(window.gameObject);
                    }
                    Destroy(window.gameObject);
                }

                _stack.Clear();
            }
            finally
            {
                _suppressQueuedShowDrain = previousSuppress;
            }
        }

        /// <summary>
        /// 将窗口插入栈中
        /// </summary>
        private void Push(BaseWindow window)
        {
            int insertIndex = -1;

            for (int i = 0; i < _stack.Count; i++)
            {
                if (window.WindowLayer == _stack[i].WindowLayer)
                {
                    insertIndex = i + 1;
                }
            }

            if (insertIndex == -1)
            {
                for (int i = 0; i < _stack.Count; i++)
                {
                    if (window.WindowLayer > _stack[i].WindowLayer)
                    {
                        insertIndex = i + 1;
                    }
                }
            }

            if (insertIndex == -1)
            {
                insertIndex = 0;
            }

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
                BaseWindow window = _stack[i];
                if (window.IsClosed)
                {
                    continue;
                }

                if (!isHideNext)
                {
                    window.SetHideByAbove(false);
                    if (window.HideBelow)
                    {
                        isHideNext = true;
                    }
                }
                else
                {
                    window.SetHideByAbove(true);
                }
            }
        }

        private void CloseWindowInternal(BaseWindow window, bool triggerQueuedShow)
        {
            if (window.IsClosed)
            {
                return;
            }

            window.OnClose();
            window.InternalClose();
            OnSetWindowVisible();

            if (triggerQueuedShow)
            {
                TryShowNextPendingWindow(window.GetType());
            }
        }

        private void HandleDuplicateShow(Type type, WindowAttribute attr, object[] args)
        {
            if (attr.DuplicateShowMode != WindowDuplicateShowMode.QueueAfterClose)
            {
                return;
            }

            EnqueuePendingShow(type, args);
        }

        private void TryShowNextPendingWindow(Type type)
        {
            if (_suppressQueuedShowDrain)
            {
                return;
            }

            if (!TryDequeuePendingShow(type, out object[] nextArgs))
            {
                return;
            }

            ContinuePendingShowAsync(type, nextArgs).Forget();
        }

        private async UniTask ContinuePendingShowAsync(Type type, object[] args)
        {
            try
            {
                await ShowUIAsync(type, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OUI] 自动续开窗口 {type.Name} 失败。\n{ex}");
            }
        }

        private void EnqueuePendingShow(Type type, object[] args)
        {
            if (!_pendingShowQueues.TryGetValue(type, out Queue<object[]> queue))
            {
                queue = new Queue<object[]>();
                _pendingShowQueues[type] = queue;
            }

            queue.Enqueue(args);
        }

        private bool TryDequeuePendingShow(Type type, out object[] args)
        {
            args = null;
            if (!_pendingShowQueues.TryGetValue(type, out Queue<object[]> queue) || queue.Count == 0)
            {
                return false;
            }

            args = queue.Dequeue();
            if (queue.Count == 0)
            {
                _pendingShowQueues.Remove(type);
            }

            return true;
        }

        private void ClearPendingShows(Type type)
        {
            _pendingShowQueues.Remove(type);
        }

        private void ClearAllPendingShows()
        {
            _pendingShowQueues.Clear();
        }

        private WindowAttribute GetWindowAttribute(Type type)
        {
            if (_windowAttributeCache.TryGetValue(type, out WindowAttribute attr))
            {
                return attr;
            }

            object[] attributes = type.GetCustomAttributes(typeof(WindowAttribute), false);
            if (attributes.Length == 0 || !(attributes[0] is WindowAttribute windowAttribute))
            {
                throw new Exception($"Window {type.Name} 缺少 [WindowAttribute]");
            }

            _windowAttributeCache[type] = windowAttribute;
            return windowAttribute;
        }

        private static object[] SnapshotArgs(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return Array.Empty<object>();
            }

            object[] snapshot = new object[args.Length];
            Array.Copy(args, snapshot, args.Length);
            return snapshot;
        }
    }
}
