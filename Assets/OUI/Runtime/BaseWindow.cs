using UnityEngine;
using UnityEngine.UI;

namespace OUI
{
    /// <summary>
    /// 窗口基类
    ///
    /// 前提条件：UI Camera 的 culling mask 必须包含 WINDOW_SHOW_LAYER（UI层=5），
    /// 且不包含 WINDOW_HIDE_LAYER（Ignore Raycast层=2）
    /// </summary>
    public abstract class BaseWindow : MonoBehaviour
    {
        private const int WINDOW_SHOW_LAYER = 5;  // UI层
        private const int WINDOW_HIDE_LAYER = 2;  // Ignore Raycast层

        private Canvas _canvas;
        private Canvas[] _childCanvas;
        private GraphicRaycaster _raycaster;
        private GraphicRaycaster[] _childRaycaster;

        private bool _hideByAbove = false;
        private bool _closed = false;

        internal WindowLayer WindowLayer { get; set; }
        internal bool HideBelow { get; set; }

        /// <summary>
        /// 窗口是否处于关闭状态
        /// </summary>
        public bool IsClosed => _closed;

        /// <summary>
        /// 是否被上层HideBelow窗口遮挡
        /// </summary>
        public bool HideByAbove => _hideByAbove;

        private bool IsHidden => _hideByAbove || _closed;

        /// <summary>
        /// 设置窗口的sortingOrder深度
        /// </summary>
        internal int Depth
        {
            set
            {
                Debug.Assert(_childCanvas.Length <= OUI.WINDOW_DEEP / 5,
                    $"Window子Canvas数量({_childCanvas.Length})超过上限({OUI.WINDOW_DEEP / 5})，sortingOrder将溢出");

                _canvas.sortingOrder = value;
                int childDepth = value;
                for (int i = 0; i < _childCanvas.Length; i++)
                {
                    if (_childCanvas[i] != _canvas)
                    {
                        childDepth += 5;
                        _childCanvas[i].sortingOrder = childDepth;
                    }
                }
            }
        }

        /// <summary>
        /// 内部方法：初始化Canvas引用
        /// </summary>
        internal void InternalInit()
        {
            _canvas = GetComponent<Canvas>();
            _raycaster = GetComponent<GraphicRaycaster>();
            _childCanvas = GetComponentsInChildren<Canvas>(true);
            _childRaycaster = GetComponentsInChildren<GraphicRaycaster>(true);

            Debug.Assert(_canvas != null, $"Window {GetType().Name} 缺少Canvas组件");
            Debug.Assert(_raycaster != null, $"Window {GetType().Name} 缺少GraphicRaycaster组件");
        }

        /// <summary>
        /// 内部方法：设置遮挡状态
        /// </summary>
        internal void SetHideByAbove(bool hidden)
        {
            if (_hideByAbove != hidden)
            {
                _hideByAbove = hidden;
                RefreshVisible();
                OnHideByAboveChanged(hidden);
            }
        }

        /// <summary>
        /// 内部方法：打开窗口
        /// </summary>
        internal void InternalOpen()
        {
            _closed = false;
            RefreshVisible();
        }

        /// <summary>
        /// 内部方法：关闭窗口
        /// </summary>
        internal void InternalClose()
        {
            _closed = true;
            RefreshVisible();
        }

        /// <summary>
        /// 刷新窗口可见性（通过Layer切换）
        /// 注意：此方法仅切换挂有Canvas的GameObject的Layer，窗口内不挂Canvas的GameObject（如3D物体、粒子）不会被隐藏。
        /// </summary>
        private void RefreshVisible()
        {
            bool visible = !IsHidden;
            int setLayer = visible ? WINDOW_SHOW_LAYER : WINDOW_HIDE_LAYER;

            _canvas.gameObject.layer = setLayer;
            foreach (var child in _childCanvas)
            {
                if (child != _canvas)
                {
                    child.gameObject.layer = setLayer;
                }
            }

            _raycaster.enabled = visible;
            foreach (var child in _childRaycaster)
            {
                if (child != _raycaster)
                {
                    child.enabled = visible;
                }
            }
        }

        /// <summary>
        /// 关闭自身的便捷方法（隐藏，不销毁）
        /// </summary>
        public void Close()
        {
            OUI.Instance.CloseWindow(GetType());
        }

        /// <summary>
        /// 按相对路径查找子Transform。找不到时抛出异常以便尽早暴露问题。
        /// </summary>
        protected Transform FindChild(string path)
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                throw new System.Exception($"Window {GetType().Name} 找不到子节点: {path}");
            }
            return child;
        }

        /// <summary>
        /// 按相对路径查找子节点上的组件。找不到时抛出异常以便尽早暴露问题。
        /// </summary>
        protected T FindChildComponent<T>(string path) where T : Component
        {
            Transform child = FindChild(path);
            T component = child.GetComponent<T>();
            if (component == null)
            {
                throw new System.Exception($"Window {GetType().Name} 子节点 {path} 上找不到组件: {typeof(T).Name}");
            }
            return component;
        }

        /// <summary>
        /// 资源加载完成后调用一次，用于绑定UI组件引用。在Init()之前执行。
        /// </summary>
        public abstract void BindUI();

        /// <summary>
        /// BindUI()之后调用一次，用于初始化逻辑。
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 释放窗口资源时调用。
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// 打开窗口时调用（首次打开和重新打开都会调用）。
        /// </summary>
        /// <param name="args">参数数组</param>
        public virtual void OnOpen(object[] args) { }

        /// <summary>
        /// 关闭窗口时调用（隐藏，不销毁）。
        /// </summary>
        public virtual void OnClose() { }

        /// <summary>
        /// 遮挡状态变化时调用。
        /// 子类可重写以响应被遮挡/恢复事件（如暂停/恢复动画、音效等）。
        /// </summary>
        protected virtual void OnHideByAboveChanged(bool hidden) { }
    }
}
