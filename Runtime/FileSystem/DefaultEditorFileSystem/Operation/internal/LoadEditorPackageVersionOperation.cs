﻿using System.IO;

namespace YooAsset
{
    internal class LoadEditorPackageVersionOperation : AsyncOperationBase
    {
        private readonly DefaultEditorFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;


        internal LoadEditorPackageVersionOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        ///     包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }

        internal override void InternalOnStart()
        {
            _steps = ESteps.LoadVersion;
        }

        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.LoadVersion)
            {
                var versionFilePath = _fileSystem.GetEditorPackageVersionFilePath();
                if (File.Exists(versionFilePath))
                {
                    _steps = ESteps.Done;
                    PackageVersion = FileUtility.ReadAllText(versionFilePath);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found simulation package version file : {versionFilePath}";
                }
            }
        }

        private enum ESteps
        {
            None,
            LoadVersion,
            Done
        }
    }
}