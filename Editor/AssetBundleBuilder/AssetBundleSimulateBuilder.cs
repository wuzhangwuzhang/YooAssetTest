using System;
using UnityEditor;

namespace YooAsset.Editor
{
    public static class AssetBundleSimulateBuilder
    {
        /// <summary>
        ///     模拟构建
        /// </summary>
        public static SimulateBuildResult SimulateBuild(string buildPipelineName, string packageName)
        {
            var packageVersion = "Simulate";
            BuildResult buildResult;

            if (buildPipelineName == EBuildPipeline.BuiltinBuildPipeline.ToString())
            {
                var buildParameters = new BuiltinBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = packageVersion;
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                var pipeline = new BuiltinBuildPipeline();
                buildResult = pipeline.Run(buildParameters, false);
            }
            else if (buildPipelineName == EBuildPipeline.ScriptableBuildPipeline.ToString())
            {
                var buildParameters = new ScriptableBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = packageVersion;
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                var pipeline = new ScriptableBuildPipeline();
                buildResult = pipeline.Run(buildParameters, true);
            }
            else if (buildPipelineName == EBuildPipeline.RawFileBuildPipeline.ToString())
            {
                var buildParameters = new RawFileBuildParameters();
                buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
                buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
                buildParameters.BuildPipeline = buildPipelineName;
                buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
                buildParameters.BuildMode = EBuildMode.SimulateBuild;
                buildParameters.PackageName = packageName;
                buildParameters.PackageVersion = packageVersion;
                buildParameters.FileNameStyle = EFileNameStyle.HashName;
                buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
                buildParameters.BuildinFileCopyParams = string.Empty;

                var pipeline = new RawFileBuildPipeline();
                buildResult = pipeline.Run(buildParameters, true);
            }
            else
            {
                throw new NotImplementedException(buildPipelineName);
            }

            // 返回结果
            if (buildResult.Success)
            {
                var reulst = new SimulateBuildResult();
                reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return reulst;
            }

            return null;
        }
    }
}