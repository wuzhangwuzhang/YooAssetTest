namespace YooAsset.Editor
{
    public class TaskGetBuildMap_SBP : TaskGetBuildMap, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);
        }
    }
}