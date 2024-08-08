using UnityEngine;

namespace YooAsset.Editor
{
    public static class AssetBundleBuilderHelper
    {
        /// <summary>
        ///     获取默认的输出根目录
        /// </summary>
        public static string GetDefaultBuildOutputRoot()
        {
            var projectPath = EditorTools.GetProjectPath();
            return $"{projectPath}/Bundles";
        }

        /// <summary>
        ///     获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsRoot()
        {
            return $"{Application.dataPath}/StreamingAssets/{YooAssetSettingsData.Setting.DefaultYooFolderName}/";
        }
    }
}