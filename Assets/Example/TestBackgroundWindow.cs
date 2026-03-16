using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试底层窗口 - 演示Bottom层级，始终在最底层
    /// </summary>
    [Window("Assets/Example/Prefabs/TestBackgroundWindow.prefab", WindowLayer.Bottom, hideBelow: false)]
    public class TestBackgroundWindow : BaseWindow
    {
        private Text _statusText;
        private Image _bgImage;
        private float _colorTimer;

        public override void BindUI()
        {
            _statusText = FindChildComponent<Text>("StatusText");
            _bgImage = GetComponent<Image>();
        }

        public override void Init()
        {
            _colorTimer = 0f;
        }

        public override void OnOpen(object[] args)
        {
            _statusText.text = "背景窗口 - Bottom层";
            Debug.Log("[TestBackgroundWindow] OnOpen");
        }

        public override void OnClose()
        {
            Debug.Log("[TestBackgroundWindow] OnClose");
        }

        public override void Release()
        {
        }

        protected override void OnHideByAboveChanged(bool hidden)
        {
            _statusText.text = hidden ? "背景窗口 - 被遮挡" : "背景窗口 - 可见";
            Debug.Log($"[TestBackgroundWindow] OnHideByAboveChanged - hidden: {hidden}");
        }

        private void Update()
        {
            if (!IsClosed && !HideByAbove)
            {
                // 简单的颜色动画
                _colorTimer += Time.deltaTime;
                float h = Mathf.PingPong(_colorTimer * 0.1f, 1f);
                _bgImage.color = Color.HSVToRGB(h, 0.3f, 0.8f);
            }
        }
    }
}
