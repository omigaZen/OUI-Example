using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试主窗口 - 演示基础窗口功能和参数传递
    /// </summary>
    [Window("Assets/Example/Prefabs/TestMainWindow.prefab", WindowLayer.UI, hideBelow: true)]
    public class TestMainWindow : BaseWindow
    {
        private Text _titleText;
        private Button _openSettingsBtn;
        private Button _openTipsBtn;
        private Button _closeBtn;

        public override void BindUI()
        {
            _titleText = FindChildComponent<Text>("Title");
            _openSettingsBtn = FindChildComponent<Button>("OpenSettingsBtn");
            _openTipsBtn = FindChildComponent<Button>("OpenTipsBtn");
            _closeBtn = FindChildComponent<Button>("CloseBtn");
        }

        public override void Init()
        {
            _openSettingsBtn.onClick.AddListener(OnOpenSettingsClick);
            _openTipsBtn.onClick.AddListener(OnOpenTipsClick);
            _closeBtn.onClick.AddListener(OnCloseClick);
        }

        public override void OnOpen(object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _titleText.text = $"主窗口 - {args[0]}";
            }
            else
            {
                _titleText.text = "主窗口";
            }

            Debug.Log($"[TestMainWindow] OnOpen - args: {(args != null ? string.Join(", ", args) : "null")}");
        }

        public override void OnClose()
        {
            Debug.Log("[TestMainWindow] OnClose");
        }

        public override void Release()
        {
            _openSettingsBtn.onClick.RemoveAllListeners();
            _openTipsBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
        }

        protected override void OnHideByAboveChanged(bool hidden)
        {
            Debug.Log($"[TestMainWindow] OnHideByAboveChanged - hidden: {hidden}");
        }

        private void OnOpenSettingsClick()
        {
            OUI.Instance.ShowUIAsync<TestSettingsWindow>("来自主窗口的参数").Forget();
        }

        private void OnOpenTipsClick()
        {
            OUI.Instance.ShowUIAsync<TestTipsWindow>("这是一条提示信息").Forget();
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
