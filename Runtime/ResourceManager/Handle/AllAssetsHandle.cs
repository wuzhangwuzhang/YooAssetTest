﻿using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace YooAsset
{
    public sealed class AllAssetsHandle : HandleBase, IDisposable
    {
        private Action<AllAssetsHandle> _callback;

        internal AllAssetsHandle(ProviderOperation provider) : base(provider)
        {
        }


        /// <summary>
        ///     子资源对象集合
        /// </summary>
        public IReadOnlyList<Object> AllAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                    return null;
                return Provider.AllAssetObjects;
            }
        }

        /// <summary>
        ///     释放资源句柄
        /// </summary>
        public void Dispose()
        {
            ReleaseInternal();
        }

        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        ///     完成委托
        /// </summary>
        public event Action<AllAssetsHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new Exception($"{nameof(AllAssetsHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new Exception($"{nameof(AllAssetsHandle)} is invalid");
                _callback -= value;
            }
        }

        /// <summary>
        ///     等待异步执行完毕
        /// </summary>
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        ///     释放资源句柄
        /// </summary>
        public void Release()
        {
            ReleaseInternal();
        }
    }
}