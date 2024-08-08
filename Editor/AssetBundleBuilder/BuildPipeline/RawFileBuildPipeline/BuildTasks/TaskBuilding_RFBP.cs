namespace YooAsset.Editor
{
    public class TaskBuilding_RFBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();

            var buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
                CopyRawBundle(buildMapContext, buildParametersContext);
        }

        /// <summary>
        ///     拷贝原生文件
        /// </summary>
        private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
        {
            var pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                foreach (var buildAsset in bundleInfo.MainAssets)
                    EditorTools.CopyFile(buildAsset.AssetInfo.AssetPath, dest, true);
            }
        }
    }
}