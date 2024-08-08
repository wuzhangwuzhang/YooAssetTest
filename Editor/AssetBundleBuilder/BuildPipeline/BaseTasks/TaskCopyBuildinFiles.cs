using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskCopyBuildinFiles
    {
        /// <summary>
        ///     拷贝首包资源文件
        /// </summary>
        internal void CopyBuildinFilesToStreaming(BuildParametersContext buildParametersContext,
            PackageManifest manifest)
        {
            var copyOption = buildParametersContext.Parameters.BuildinFileCopyOption;
            var packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            var buildinRootDirectory = buildParametersContext.GetBuildinRootDirectory();
            var buildPackageName = buildParametersContext.Parameters.PackageName;
            var buildPackageVersion = buildParametersContext.Parameters.PackageVersion;

            // 清空内置文件的目录
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyAll ||
                copyOption == EBuildinFileCopyOption.ClearAndCopyByTags) EditorTools.ClearFolder(buildinRootDirectory);

            // 拷贝补丁清单文件
            {
                var fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildPackageName, buildPackageVersion);
                var sourcePath = $"{packageOutputDirectory}/{fileName}";
                var destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                var fileName = YooAssetSettingsData.GetPackageHashFileName(buildPackageName, buildPackageVersion);
                var sourcePath = $"{packageOutputDirectory}/{fileName}";
                var destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                var fileName = YooAssetSettingsData.GetPackageVersionFileName(buildPackageName);
                var sourcePath = $"{packageOutputDirectory}/{fileName}";
                var destPath = $"{buildinRootDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyAll ||
                copyOption == EBuildinFileCopyOption.OnlyCopyAll)
                foreach (var packageBundle in manifest.BundleList)
                {
                    var sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    var destPath = $"{buildinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }

            // 拷贝文件列表（带标签的文件）
            if (copyOption == EBuildinFileCopyOption.ClearAndCopyByTags ||
                copyOption == EBuildinFileCopyOption.OnlyCopyByTags)
            {
                var tags = buildParametersContext.Parameters.BuildinFileCopyParams.Split(';');
                foreach (var packageBundle in manifest.BundleList)
                {
                    if (packageBundle.HasTag(tags) == false)
                        continue;
                    var sourcePath = $"{packageOutputDirectory}/{packageBundle.FileName}";
                    var destPath = $"{buildinRootDirectory}/{packageBundle.FileName}";
                    EditorTools.CopyFile(sourcePath, destPath, true);
                }
            }

            // 刷新目录
            AssetDatabase.Refresh();
            BuildLogger.Log($"Buildin files copy complete: {buildinRootDirectory}");
        }
    }
}