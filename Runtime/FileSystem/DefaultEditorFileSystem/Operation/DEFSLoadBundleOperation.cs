namespace YooAsset
{
    internal class DEFSLoadBundleOperation : FSLoadBundleOperation
    {
        private readonly PackageBundle _bundle;
        private readonly DefaultEditorFileSystem _fileSystem;

        internal DEFSLoadBundleOperation(DefaultEditorFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }

        internal override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            Status = EOperationStatus.Succeed;
        }

        internal override void InternalOnUpdate()
        {
        }

        internal override void InternalWaitForAsyncComplete()
        {
        }

        public override void AbortDownloadOperation()
        {
        }
    }
}