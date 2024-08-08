using System;

namespace YooAsset.Editor
{
    public class TaskUpdateBundleInfo_SBP : TaskUpdateBundleInfo, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            UpdateBundleInfo(context);
        }

        protected override string GetUnityHash(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var parameters = buildParametersContext.Parameters;
            var buildMode = parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild) return "00000000000000000000000000000000"; //32位

            // 注意：当资源包的依赖列表发生变化的时候，ContentHash也会发生变化！
            var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
            if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
            {
                return value.Hash.ToString();
            }

            var message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleHash,
                $"Not found unity bundle hash : {bundleInfo.BundleName}");
            throw new Exception(message);
        }

        protected override uint GetUnityCRC(BuildBundleInfo bundleInfo, BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var parameters = buildParametersContext.Parameters;
            var buildMode = parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild) return 0;

            var buildResult = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
            if (buildResult.Results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
            {
                return value.Crc;
            }

            var message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleCRC,
                $"Not found unity bundle crc : {bundleInfo.BundleName}");
            throw new Exception(message);
        }

        protected override string GetBundleFileHash(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext)
        {
            var filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
                return GetFilePathTempHash(filePath);
            return HashUtility.FileMD5(filePath);
        }

        protected override string GetBundleFileCRC(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext)
        {
            var filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
                return "00000000"; //8位
            return HashUtility.FileCRC32(filePath);
        }

        protected override long GetBundleFileSize(BuildBundleInfo bundleInfo,
            BuildParametersContext buildParametersContext)
        {
            var filePath = bundleInfo.PackageSourceFilePath;
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
                return GetBundleTempSize(bundleInfo);
            return FileUtility.GetFileSize(filePath);
        }
    }
}