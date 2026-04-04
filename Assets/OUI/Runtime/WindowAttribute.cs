using System;

namespace OUI
{
    /// <summary>
    /// 重复打开同一窗口时的处理策略。
    /// 默认保持兼容：窗口已打开或仍在加载时直接忽略请求。
    /// </summary>
    public enum WindowDuplicateShowMode
    {
        Return = 0,
        QueueAfterClose = 1
    }

    /// <summary>
    /// Window特性，用于标识Window对应的UI资源路径、Layer、HideBelow信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class WindowAttribute : Attribute
    {
        /// <summary>
        /// UI资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 窗口层级
        /// </summary>
        public WindowLayer Layer { get; }

        /// <summary>
        /// 是否遮挡下层窗口
        /// </summary>
        public bool HideBelow { get; }

        /// <summary>
        /// 重复打开同一Window时的处理策略。
        /// 只有显式声明 QueueAfterClose 的窗口才会缓存待续开请求。
        /// </summary>
        public WindowDuplicateShowMode DuplicateShowMode { get; set; } = WindowDuplicateShowMode.Return;

        public WindowAttribute(string assetPath, WindowLayer layer = WindowLayer.UI, bool hideBelow = true)
        {
            AssetPath = assetPath;
            Layer = layer;
            HideBelow = hideBelow;
        }
    }
}
