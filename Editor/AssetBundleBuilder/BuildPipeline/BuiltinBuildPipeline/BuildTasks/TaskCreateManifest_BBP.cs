namespace YooAsset.Editor
{
    public class TaskCreateManifest_BBP : TaskCreateManifest, IBuildTask
    {
        private TaskBuilding_BBP.BuildResultContext _buildResultContext;

        void IBuildTask.Run(BuildContext context)
        {
            CreateManifestFile(context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            if (_buildResultContext == null)
                _buildResultContext = context.GetContextObject<TaskBuilding_BBP.BuildResultContext>();

            return _buildResultContext.UnityManifest.GetAllDependencies(bundleName);
        }
    }
}