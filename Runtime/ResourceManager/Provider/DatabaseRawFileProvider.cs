﻿using UnityEditor;

namespace YooAsset
{
    internal class DatabaseRawFileProvider : ProviderOperation
    {
        public DatabaseRawFileProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(
            manager, providerGUID, assetInfo)
        {
        }

        internal override void InternalOnStart()
        {
            DebugBeginRecording();
        }

        internal override void InternalOnUpdate()
        {
#if UNITY_EDITOR
            if (IsDone)
                return;

            if (_steps == ESteps.None)
            {
                // 检测资源文件是否存在
                var guid = AssetDatabase.AssetPathToGUID(MainAssetInfo.AssetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    var error = $"Not found asset : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.CheckBundle;

                // 注意：模拟异步加载效果提前返回
                if (IsWaitForAsyncComplete == false)
                    return;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                    return;

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Checking;
            }

            // 2. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                RawBundleObject = new RawBundle(null, null, MainAssetInfo.AssetPath);
                InvokeCompletion(string.Empty, EOperationStatus.Succeed);
            }
#endif
        }
    }
}