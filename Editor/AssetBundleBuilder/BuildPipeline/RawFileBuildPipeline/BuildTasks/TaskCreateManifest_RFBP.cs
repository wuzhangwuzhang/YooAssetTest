using System;

namespace YooAsset.Editor
{
    public class TaskCreateManifest_RFBP : TaskCreateManifest, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            CreateManifestFile(context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            return Array.Empty<string>();
        }
    }
}