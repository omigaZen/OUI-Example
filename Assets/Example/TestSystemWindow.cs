using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试系统窗口 - 演示System层级（最高层）和窗口栈管理
    /// </summary>
    [Window("Assets/Example/Prefabs/TestSystemWindow.prefab", WindowLayer.System, hideBelow: true)]
    public class TestSystemWindow : BaseWindow
    {
        private Text _infoText;
        private Button _closeAllBtn;
        private Button _closeBtn;
        private ScrollRect _scrollRect;
        private Text _stackInfoText;

        public override void BindUI()
        {
            _infoText = FindChildComponent<Text>("InfoText");
            _closeAllBtn = FindChildComponent<Button>("CloseAllBtn");
            _closeBtn = FindChildComponent<Button>("CloseBtn");
            _scrollRect = FindChildComponent<ScrollRect>("ScrollView");
            _stackInfoText = FindChildComponent<Text>("ScrollView/Viewport/Content/StackInfoText");
        }

        public override void Init()
        {
            _closeAllBtn.onClick.AddListener(OnCloseAllClick);
            _closeBtn.onClick.AddListener(OnCloseClick);
        }

        public override void OnOpen(object[] args)
        {
            _infoText.text = "系统窗口 - System层（最高层）";
            UpdateStackInfo();
            Debug.Log("[TestSystemWindow] OnOpen");
        }

        public override void OnClose()
        {
            Debug.Log("[TestSystemWindow] OnClose");
        }

        public override void Release()
        {
            _closeAllBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (!IsClosed)
            {
                UpdateStackInfo();
            }
        }

        private void UpdateStackInfo()
        {
            var windows = OUI.Instance.GetAllWindows();
            string info = $"当前窗口栈 (共{windows.Count}个):\n\n";

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                var window = windows[i];
                string status = window.IsClosed ? "[已关闭]" : window.HideByAbove ? "[被遮挡]" : "[可见]";
                info += $"{windows.Count - i}. {window.GetType().Name}\n";
                info += $"   层级: {window.WindowLayer} | 状态: {status}\n\n";
            }

            _stackInfoText.text = info;
        }

        private void OnCloseAllClick()
        {
            OUI.Instance.CloseAllWindows();
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
