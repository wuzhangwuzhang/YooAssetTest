﻿namespace YooAsset
{
    internal class LoadBuildinPackageManifestOperation : AsyncOperationBase
    {
        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly string _packageHash;
        private readonly string _packageVersion;
        private DeserializeManifestOperation _deserializer;
        private ESteps _steps = ESteps.None;
        private UnityWebDataRequestOperation _webDataRequestOp;


        internal LoadBuildinPackageManifestOperation(DefaultBuildinFileSystem fileSystem, string packageVersion,
            string packageHash)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
        }

        /// <summary>
        ///     包裹清单
        /// </summary>
        public PackageManifest Manifest { private set; get; }

        internal override void InternalOnStart()
        {
            _steps = ESteps.RequestFileData;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestFileData)
            {
                if (_webDataRequestOp == null)
                {
                    var filePath = _fileSystem.GetBuildinPackageManifestFilePath(_packageVersion);
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webDataRequestOp = new UnityWebDataRequestOperation(url);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webDataRequestOp);
                }

                if (_webDataRequestOp.IsDone == false)
                    return;

                if (_webDataRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.VerifyFileData;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webDataRequestOp.Error;
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                var fileHash = HashUtility.BytesMD5(_webDataRequestOp.Result);
                if (fileHash == _packageHash)
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify buildin package manifest file !";
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializer == null)
                {
                    _deserializer = new DeserializeManifestOperation(_webDataRequestOp.Result);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _deserializer);
                }

                Progress = _deserializer.Progress;
                if (_deserializer.IsDone == false)
                    return;

                if (_deserializer.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _deserializer.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _deserializer.Error;
                }
            }
        }

        private enum ESteps
        {
            None,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done
        }
    }
}