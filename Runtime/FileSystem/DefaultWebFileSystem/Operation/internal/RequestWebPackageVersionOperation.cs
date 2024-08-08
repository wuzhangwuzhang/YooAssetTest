namespace YooAsset
{
    internal class RequestWebPackageVersionOperation : AsyncOperationBase
    {
        private readonly DefaultWebFileSystem _fileSystem;
        private readonly int _timeout;
        private ESteps _steps = ESteps.None;
        private UnityWebTextRequestOperation _webTextRequestOp;


        internal RequestWebPackageVersionOperation(DefaultWebFileSystem fileSystem, int timeout)
        {
            _fileSystem = fileSystem;
            _timeout = timeout;
        }

        /// <summary>
        ///     包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }

        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_webTextRequestOp == null)
                {
                    var filePath = _fileSystem.GetWebPackageVersionFilePath();
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                }

                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Web package version file content is empty !";
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
            RequestPackageVersion,
            Done
        }
    }
}