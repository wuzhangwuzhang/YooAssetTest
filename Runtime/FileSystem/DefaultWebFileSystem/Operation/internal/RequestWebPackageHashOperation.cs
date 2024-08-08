namespace YooAsset
{
    internal class RequestWebPackageHashOperation : AsyncOperationBase
    {
        private readonly DefaultWebFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private ESteps _steps = ESteps.None;
        private UnityWebTextRequestOperation _webTextRequestOp;


        public RequestWebPackageHashOperation(DefaultWebFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }

        /// <summary>
        ///     包裹哈希值
        /// </summary>
        public string PackageHash { private set; get; }

        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageHash;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_webTextRequestOp == null)
                {
                    var filePath = _fileSystem.GetWebPackageHashFilePath(_packageVersion);
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                }

                Progress = _webTextRequestOp.Progress;
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    PackageHash = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageHash))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Web package hash file content is empty !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                }
            }
        }

        private enum ESteps
        {
            None,
            RequestPackageHash,
            Done
        }
    }
}