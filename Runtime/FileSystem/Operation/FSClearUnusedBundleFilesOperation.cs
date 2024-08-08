namespace YooAsset
{
    internal abstract class FSClearUnusedBundleFilesOperation : AsyncOperationBase
    {
    }

    internal sealed class FSClearUnusedBundleFilesCompleteOperation : FSClearUnusedBundleFilesOperation
    {
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }

        internal override void InternalOnUpdate()
        {
        }
    }
}