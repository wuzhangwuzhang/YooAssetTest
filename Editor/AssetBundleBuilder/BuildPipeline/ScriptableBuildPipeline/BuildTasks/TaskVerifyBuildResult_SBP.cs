using System;
using System.Linq;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    public class TaskVerifyBuildResult_SBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 模拟构建模式下跳过验证
            if (buildParameters.BuildMode == EBuildMode.SimulateBuild)
                return;

            // 验证构建结果
            if (buildParameters.VerifyBuildingResult)
            {
                var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
                VerifyingBuildingResult(context, buildResultContext.Results);
            }
        }

        /// <summary>
        ///     验证构建结果
        /// </summary>
        private void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var unityCreateBundles = buildResults.BundleInfos.Keys.ToList();

            // 1. 过滤掉原生Bundle
            var expectBundles = buildMapContext.Collection.Select(t => t.BundleName).ToList();

            // 2. 验证Bundle
            var exceptBundleList1 = unityCreateBundles.Except(expectBundles).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    var warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle,
                        $"Found unintended build bundle : {exceptBundle}");
                    BuildLogger.Warning(warning);
                }

                var exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult,
                    "Unintended build, See the detailed warnings !");
                throw new Exception(exception);
            }

            // 3. 验证Bundle
            var exceptBundleList2 = expectBundles.Except(unityCreateBundles).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    var warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle,
                        $"Found unintended build bundle : {exceptBundle}");
                    BuildLogger.Warning(warning);
                }

                var exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult,
                    "Unintended build, See the detailed warnings !");
                throw new Exception(exception);
            }

            BuildLogger.Log("Build results verify success!");
        }
    }
}