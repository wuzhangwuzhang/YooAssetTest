using System;

namespace YooAsset
{
    internal class RequestRemotePackageVersionOperation : AsyncOperationBase
    {
        private readonly bool _appendTimeTicks;

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly int _timeout;
        private int _requestCount;
        private ESteps _steps = ESteps.None;
        private UnityWebTextRequestOperation _webTextRequestOp;


        internal RequestRemotePackageVersionOperation(DefaultCacheFileSystem fileSystem, bool appendTimeTicks,
            int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }

        /// <summary>
        ///     包裹版本
        /// </summary>
        internal string PackageVersion { set; get; }

        internal override void InternalOnStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName,
                nameof(RequestRemotePackageVersionOperation));
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
                    var fileName = YooAssetSettingsData.GetPackageVersionFileName(_fileSystem.PackageName);
                    var url = GetWebRequestURL(fileName);
                    _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                }

                Progress = _webTextRequestOp.Progress;
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "Remote package version file content is empty !";
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
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName,
                        nameof(RequestRemotePackageVersionOperation));
                }
            }
        }

        private string GetWebRequestURL(string fileName)
        {
            string url;

            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                url = _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
            else
                url = _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);

            // 在URL末尾添加时间戳
            if (_appendTimeTicks)
                return $"{url}?{DateTime.UtcNow.Ticks}";
            return url;
        }

        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done
        }
    }
}