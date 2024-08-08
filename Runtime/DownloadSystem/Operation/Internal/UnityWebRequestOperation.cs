﻿using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
    internal abstract class UnityWebRequestOperation : AsyncOperationBase
    {
        protected readonly string _requestURL;

        // 超时相关
        protected readonly float _timeout;
        private bool _isAbort;
        protected ulong _latestDownloadBytes;
        protected float _latestDownloadRealtime;
        protected ESteps _steps = ESteps.None;

        protected UnityWebRequest _webRequest;

        internal UnityWebRequestOperation(string url, int timeout)
        {
            _requestURL = url;
            _timeout = timeout;
        }

        public string URL => _requestURL;

        /// <summary>
        ///     释放下载器
        /// </summary>
        protected void DisposeRequest()
        {
            if (_webRequest != null)
            {
                _webRequest.Dispose();
                _webRequest = null;
            }
        }

        /// <summary>
        ///     检测超时
        /// </summary>
        protected void CheckRequestTimeout()
        {
            // 注意：在连续时间段内无新增下载数据及判定为超时
            if (_isAbort == false)
            {
                if (_latestDownloadBytes != _webRequest.downloadedBytes)
                {
                    _latestDownloadBytes = _webRequest.downloadedBytes;
                    _latestDownloadRealtime = Time.realtimeSinceStartup;
                }

                var offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
                if (offset > _timeout)
                {
                    _webRequest.Abort();
                    _isAbort = true;
                }
            }
        }

        /// <summary>
        ///     检测请求结果
        /// </summary>
        protected bool CheckRequestResult()
        {
#if UNITY_2020_3_OR_NEWER
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                Error = $"URL : {_requestURL} Error : {_webRequest.error}";
                return false;
            }

            return true;
#else
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
            {
                Error = $"URL : {_requestURL} Error : {_webRequest.error}";
                return false;
            }
            else
            {
                return true;
            }
#endif
        }

        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            Done
        }
    }
}