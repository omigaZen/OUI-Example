using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OUI
{
    /// <summary>
    /// 窗口资源加载接口，由外部注入具体实现
    /// </summary>
    public interface IWindowAssetLoader
    {
        /// <summary>
        /// 异步加载窗口资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>加载的GameObject实例</returns>
        UniTask<GameObject> LoadAsync(string assetPath);

        /// <summary>
        /// 卸载窗口资源
        /// </summary>
        /// <param name="go">要卸载的GameObject</param>
        void Unload(GameObject go);
    }
}
