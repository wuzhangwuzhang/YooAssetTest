namespace YooAsset
{
    internal class RequestBuildinPackageVersionOperation : AsyncOperationBase
    {
        private readonly DefaultBuildinFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;
        private UnityWebTextRequestOperation _webTextRequestOp;


        internal RequestBuildinPackageVersionOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                    var filePath = _fileSystem.GetBuildinPackageVersionFilePath();
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url);
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
                        Error = "Buildin package version file content is empty !";
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