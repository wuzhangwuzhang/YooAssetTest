﻿using System.IO;

namespace YooAsset
{
    internal class DownloadPackageManifestOperation : AsyncOperationBase
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private int _requestCount;
        private ESteps _steps = ESteps.None;
        private UnityWebFileRequestOperation _webFileRequestOp;


        internal DownloadPackageManifestOperation(DefaultCacheFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }

        internal override void InternalOnStart()
        {
            _requestCount =
                WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName,
                    nameof(DownloadPackageManifestOperation));
            _steps = ESteps.DownloadFile;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckExist)
            {
                var filePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_webFileRequestOp == null)
                {
                    var savePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                    var fileName =
                        YooAssetSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, _packageVersion);
                    var webURL = GetDownloadRequestURL(fileName);
                    _webFileRequestOp = new UnityWebFileRequestOperation(webURL, savePath, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webFileRequestOp);
                }

                if (_webFileRequestOp.IsDone == false)
                    return;

                if (_webFileRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webFileRequestOp.Error;
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName,
                        nameof(DownloadPackageManifestOperation));
                }
            }
        }

        private string GetDownloadRequestURL(string fileName)
        {
            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                return _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
            return _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);
        }

        private enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            Done
        }
    }
}