﻿using System;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskVerifyBuildResult_BBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as BuiltinBuildParameters;

            // 模拟构建模式下跳过验证
            if (buildParameters.BuildMode == EBuildMode.SimulateBuild)
                return;

            // 验证构建结果
            if (buildParameters.VerifyBuildingResult)
            {
                var buildResultContext = context.GetContextObject<TaskBuilding_BBP.BuildResultContext>();
                VerifyingBuildingResult(context, buildResultContext.UnityManifest);
            }
        }

        /// <summary>
        ///     验证构建结果
        /// </summary>
        private void VerifyingBuildingResult(BuildContext context, AssetBundleManifest unityManifest)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var unityCreateBundles = unityManifest.GetAllAssetBundles();

            // 1. 过滤掉原生Bundle
            var mapBundles = buildMapContext.Collection.Select(t => t.BundleName).ToArray();

            // 2. 验证Bundle
            var exceptBundleList1 = unityCreateBundles.Except(mapBundles).ToList();
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
            var exceptBundleList2 = mapBundles.Except(unityCreateBundles).ToList();
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