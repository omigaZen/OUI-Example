using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试提示窗口 - 演示Tips层级、非遮挡窗口和重复打开排队
    /// </summary>
    [Window("Assets/Example/Prefabs/TestTipsWindow.prefab", WindowLayer.Tips, hideBelow: false, DuplicateShowMode = WindowDuplicateShowMode.QueueAfterClose)]
    public class TestTipsWindow : BaseWindow
    {
        private Text _messageText;
        private Button _okBtn;
        private float _autoCloseTime = 3f;
        private float _timer;

        public override void BindUI()
        {
            _messageText = FindChildComponent<Text>("MessageText");
            _okBtn = FindChildComponent<Button>("OkBtn");
        }

        public override void Init()
        {
            _okBtn.onClick.AddListener(OnOkClick);
        }

        public override void OnOpen(object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _messageText.text = args[0].ToString();
            }
            else
            {
                _messageText.text = "提示信息";
            }

            _timer = 0f;
            Debug.Log($"[TestTipsWindow] OnOpen - message: {_messageText.text}");
        }

        public override void OnClose()
        {
            Debug.Log("[TestTipsWindow] OnClose");
        }

        public override void Release()
        {
            _okBtn.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            if (!IsClosed)
            {
                _timer += Time.deltaTime;
                if (_timer >= _autoCloseTime)
                {
                    Close();
                }
            }
        }

        private void OnOkClick()
        {
            Close();
        }
    }
}
