using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class RawFileBuildPipeline : IBuildPipeline
    {
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            if (buildParameters is RawFileBuildParameters)
            {
                var builder = new AssetBundleBuilder();
                return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
            }

            throw new Exception($"Invalid build parameter type : {buildParameters.GetType().Name}");
        }

        /// <summary>
        ///     获取默认的构建流程
        /// </summary>
        private List<IBuildTask> GetDefaultBuildPipeline()
        {
            var pipeline = new List<IBuildTask>
            {
                new TaskPrepare_RFBP(),
                new TaskGetBuildMap_RFBP(),
                new TaskBuilding_RFBP(),
                new TaskEncryption_RFBP(),
                new TaskUpdateBundleInfo_RFBP(),
                new TaskCreateManifest_RFBP(),
                new TaskCreateReport_RFBP(),
                new TaskCreatePackage_RFBP(),
                new TaskCopyBuildinFiles_RFBP()
            };
            return pipeline;
        }
    }
}