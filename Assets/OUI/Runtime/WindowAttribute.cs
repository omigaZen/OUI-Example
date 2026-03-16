using System;

namespace OUI
{
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

        public WindowAttribute(string assetPath, WindowLayer layer = WindowLayer.UI, bool hideBelow = true)
        {
            AssetPath = assetPath;
            Layer = layer;
            HideBelow = hideBelow;
        }
    }
}
