using System;
using System.Text;

namespace YooAsset.Editor
{
    public abstract class TaskUpdateBundleInfo
    {
        public void UpdateBundleInfo(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            var packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
            var outputNameStyle = (int)buildParametersContext.Parameters.FileNameStyle;

            // 1.检测文件名长度
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                // NOTE：检测文件名长度不要超过260字符。
                var fileName = bundleInfo.BundleName;
                if (fileName.Length >= 260)
                {
                    var message = BuildLogger.GetErrorMessage(ErrorCode.CharactersOverTheLimit,
                        $"Bundle file name character count exceeds limit : {fileName}");
                    throw new Exception(message);
                }
            }

            // 2.更新构建输出的文件路径
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                bundleInfo.BuildOutputFilePath = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                if (bundleInfo.Encrypted)
                    bundleInfo.PackageSourceFilePath = bundleInfo.EncryptedFilePath;
                else
                    bundleInfo.PackageSourceFilePath = bundleInfo.BuildOutputFilePath;
            }

            // 3.更新文件其它信息
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                bundleInfo.PackageUnityHash = GetUnityHash(bundleInfo, context);
                bundleInfo.PackageUnityCRC = GetUnityCRC(bundleInfo, context);
                bundleInfo.PackageFileHash = GetBundleFileHash(bundleInfo, buildParametersContext);
                bundleInfo.PackageFileCRC = GetBundleFileCRC(bundleInfo, buildParametersContext);
                bundleInfo.PackageFileSize = GetBundleFileSize(bundleInfo, buildParametersContext);
            }

            // 4.更新补丁包输出的文件路径
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var bundleName = bundleInfo.BundleName;
                var fileHash = bundleInfo.PackageFileHash;
                var fileExtension = ManifestTools.GetRemoteBundleFileExtension(bundleName);
                var fileName =
                    ManifestTools.GetRemoteBundleFileName(outputNameStyle, bundleName, fileExtension, fileHash);
                bundleInfo.PackageDestFilePath = $"{packageOutputDirectory}/{fileName}";
            }
        }

        protected abstract string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context);
        protected abstract uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context);

        protected abstract string GetBundleFileHash(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext);

        protected abstract string GetBundleFileCRC(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext);

        protected abstract long GetBundleFileSize(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext);

        protected string GetFilePathTempHash(string filePath)
        {
            var bytes = Encoding.UTF8.GetBytes(filePath);
            return HashUtility.BytesMD5(bytes);

            // 注意：在文件路径的哈希值冲突的情况下，可以使用下面的方法
            //return $"{HashUtility.BytesMD5(bytes)}-{Guid.NewGuid():N}";
        }

        protected long GetBundleTempSize(BuildBundleInfo bundleInfo)
        {
            long tempSize = 0;

            var assetPaths = bundleInfo.GetAllMainAssetPaths();
            foreach (var assetPath in assetPaths)
            {
                var size = FileUtility.GetFileSize(assetPath);
                tempSize += size;
            }

            if (tempSize == 0)
            {
                var message = BuildLogger.GetErrorMessage(ErrorCode.BundleTempSizeIsZero,
                    $"Bundle temp size is zero, check bundle main asset list : {bundleInfo.BundleName}");
                throw new Exception(message);
            }

            return tempSize;
        }
    }
}