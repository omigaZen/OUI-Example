using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试Top层窗口 - 演示Top层级和子Canvas深度管理
    /// </summary>
    [Window("Assets/Example/Prefabs/TestTopWindow.prefab", WindowLayer.Top, hideBelow: false)]
    public class TestTopWindow : BaseWindow
    {
        private Text _titleText;
        private Button _closeBtn;
        private Toggle _hideToggle;
        private Text _childCanvasText;

        public override void BindUI()
        {
            _titleText = FindChildComponent<Text>("Title");
            _closeBtn = FindChildComponent<Button>("CloseBtn");
            _hideToggle = FindChildComponent<Toggle>("HideToggle");
            _childCanvasText = FindChildComponent<Text>("ChildCanvas/ChildText");
        }

        public override void Init()
        {
            _closeBtn.onClick.AddListener(OnCloseClick);
            _hideToggle.onValueChanged.AddListener(OnHideToggleChanged);
        }

        public override void OnOpen(object[] args)
        {
            _titleText.text = "Top层窗口 - 不遮挡下层";
            _hideToggle.isOn = false;
            Debug.Log("[TestTopWindow] OnOpen");
        }

        public override void OnClose()
        {
            Debug.Log("[TestTopWindow] OnClose");
        }

        public override void Release()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _hideToggle.onValueChanged.RemoveAllListeners();
        }

        protected override void OnHideByAboveChanged(bool hidden)
        {
            Debug.Log($"[TestTopWindow] OnHideByAboveChanged - hidden: {hidden}");
        }

        private void OnHideToggleChanged(bool value)
        {
            _childCanvasText.text = value ? "子Canvas - 隐藏模式" : "子Canvas - 正常显示";
            Debug.Log($"[TestTopWindow] HideToggle: {value}");
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
