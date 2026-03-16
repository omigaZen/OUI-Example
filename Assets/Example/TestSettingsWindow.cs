using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// 测试设置窗口 - 演示HideBelow功能（会遮挡下层窗口）
    /// </summary>
    [Window("Assets/Example/Prefabs/TestSettingsWindow.prefab", WindowLayer.UI, hideBelow: true)]
    public class TestSettingsWindow : BaseWindow
    {
        private Text _infoText;
        private Toggle _soundToggle;
        private Slider _volumeSlider;
        private Button _closeBtn;

        public override void BindUI()
        {
            _infoText = FindChildComponent<Text>("InfoText");
            _soundToggle = FindChildComponent<Toggle>("SoundToggle");
            _volumeSlider = FindChildComponent<Slider>("VolumeSlider");
            _closeBtn = FindChildComponent<Button>("CloseBtn");
        }

        public override void Init()
        {
            _soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            _closeBtn.onClick.AddListener(OnCloseClick);
        }

        public override void OnOpen(object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _infoText.text = $"设置窗口\n参数: {args[0]}";
            }
            else
            {
                _infoText.text = "设置窗口";
            }

            Debug.Log($"[TestSettingsWindow] OnOpen - args: {(args != null ? string.Join(", ", args) : "null")}");
        }

        public override void OnClose()
        {
            Debug.Log("[TestSettingsWindow] OnClose");
        }

        public override void Release()
        {
            _soundToggle.onValueChanged.RemoveAllListeners();
            _volumeSlider.onValueChanged.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
        }

        private void OnSoundToggleChanged(bool value)
        {
            Debug.Log($"[TestSettingsWindow] Sound: {value}");
            _volumeSlider.interactable = value;
        }

        private void OnVolumeChanged(float value)
        {
            Debug.Log($"[TestSettingsWindow] Volume: {value}");
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
