using System.Threading;

namespace YooAsset
{
    internal class TempFileElement
    {
        /// <summary>
        ///     注意：原子操作对象
        /// </summary>
        public int Result;

        public TempFileElement(string filePath, string fileCRC, long fileSize)
        {
            TempFilePath = filePath;
            TempFileCRC = fileCRC;
            TempFileSize = fileSize;
        }

        public string TempFilePath { get; }
        public string TempFileCRC { get; }
        public long TempFileSize { get; }
    }

    /// <summary>
    ///     下载文件验证（线程版）
    /// </summary>
    internal class VerifyTempFileOperation : AsyncOperationBase
    {
        private readonly TempFileElement _element;
        private ESteps _steps = ESteps.None;


        internal VerifyTempFileOperation(TempFileElement element)
        {
            _element = element;
        }

        /// <summary>
        ///     验证结果
        /// </summary>
        public EFileVerifyResult VerifyResult { protected set; get; }

        internal override void InternalOnStart()
        {
            _steps = ESteps.VerifyFile;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.VerifyFile)
                if (BeginVerifyFileWithThread(_element))
                    _steps = ESteps.Waiting;

            if (_steps == ESteps.Waiting)
            {
                var result = _element.Result;
                if (result == 0)
                    return;

                VerifyResult = (EFileVerifyResult)result;
                if (VerifyResult == EFileVerifyResult.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to verify file : {_element.TempFilePath} ! ErrorCode : {VerifyResult}";
                }
            }
        }

        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                // 注意：等待子线程验证文件完毕
                InternalOnUpdate();
                if (IsDone)
                    break;
            }
        }

        private bool BeginVerifyFileWithThread(TempFileElement element)
        {
            return ThreadPool.QueueUserWorkItem(VerifyInThread, element);
        }

        private void VerifyInThread(object obj)
        {
            var element = (TempFileElement)obj;
            var result = (int)FileSystemHelper.FileVerify(element.TempFilePath, element.TempFileSize,
                element.TempFileCRC, EFileVerifyLevel.High);
            element.Result = result;
        }

        private enum ESteps
        {
            None,
            VerifyFile,
            Waiting,
            Done
        }
    }
}