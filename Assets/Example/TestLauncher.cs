using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OUI.Example
{
    /// <summary>
    /// Editor环境下的资源加载器实现
    /// </summary>
    public class EditorWindowAssetLoader : IWindowAssetLoader
    {
        public UniTask<GameObject> LoadAsync(string assetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                throw new System.Exception($"无法加载资源: {assetPath}");
            }
            return UniTask.FromResult(prefab);
        }

        public void Unload(GameObject go)
        {
            // Editor模式下使用AssetDatabase加载的资源不需要手动卸载
            // 只需要销毁实例化的GameObject
            if (go != null)
            {
                Object.Destroy(go);
            }
        }
    }


    /// <summary>
    /// 测试启动器 - 用于初始化OUI并打开测试窗口
    /// </summary>
    public class TestLauncher : MonoBehaviour
    {
        [Header("测试按钮")]
        public Button openMainBtn;
        public Button openBackgroundBtn;
        public Button openSystemBtn;
        public Button openTipsBtn;

#if UNITY_EDITOR
        private void Start()
        {
            // 初始化OUI资源加载器
            OUI.Instance.AssetLoader = new EditorWindowAssetLoader();

            // 绑定按钮事件
            if (openMainBtn != null)
                openMainBtn.onClick.AddListener(() => OUI.Instance.ShowUIAsync<TestMainWindow>("测试参数").Forget());

            if (openBackgroundBtn != null)
                openBackgroundBtn.onClick.AddListener(() => OUI.Instance.ShowUIAsync<TestBackgroundWindow>().Forget());

            if (openSystemBtn != null)
                openSystemBtn.onClick.AddListener(() => OUI.Instance.ShowUIAsync<TestSystemWindow>().Forget());

            if (openTipsBtn != null)
                openTipsBtn.onClick.AddListener(() => OUI.Instance.ShowUIAsync<TestTipsWindow>("快速提示").Forget());

            Debug.Log("[TestLauncher] OUI 初始化完成");
        }
#endif

        private void OnDestroy()
        {
            if (openMainBtn != null)
                openMainBtn.onClick.RemoveAllListeners();
            if (openBackgroundBtn != null)
                openBackgroundBtn.onClick.RemoveAllListeners();
            if (openSystemBtn != null)
                openSystemBtn.onClick.RemoveAllListeners();
            if (openTipsBtn != null)
                openTipsBtn.onClick.RemoveAllListeners();
        }
    }
}
