using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskBuilding_BBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var builtinBuildParameters = buildParametersContext.Parameters as BuiltinBuildParameters;

            // 模拟构建模式下跳过引擎构建
            var buildMode = buildParametersContext.Parameters.BuildMode;
            if (buildMode == EBuildMode.SimulateBuild)
                return;

            // 开始构建
            var pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            var buildOptions = builtinBuildParameters.GetBundleBuildOptions();
            var unityManifest = BuildPipeline.BuildAssetBundles(pipelineOutputDirectory,
                buildMapContext.GetPipelineBuilds(), buildOptions, buildParametersContext.Parameters.BuildTarget);
            if (unityManifest == null)
            {
                var message =
                    BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, "UnityEngine build failed !");
                throw new Exception(message);
            }

            if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
            {
                var unityOutputManifestFilePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
                if (File.Exists(unityOutputManifestFilePath) == false)
                {
                    var message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFatal,
                        $"Not found output {nameof(AssetBundleManifest)} file : {unityOutputManifestFilePath}");
                    throw new Exception(message);
                }
            }

            BuildLogger.Log("UnityEngine build success !");
            var buildResultContext = new BuildResultContext();
            buildResultContext.UnityManifest = unityManifest;
            context.SetContextObject(buildResultContext);
        }

        public class BuildResultContext : IContextObject
        {
            public AssetBundleManifest UnityManifest;
        }
    }
}