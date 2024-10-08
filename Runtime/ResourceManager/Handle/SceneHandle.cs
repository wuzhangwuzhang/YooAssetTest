﻿using System;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    public class SceneHandle : HandleBase
    {
        private Action<SceneHandle> _callback;

        internal SceneHandle(ProviderOperation provider) : base(provider)
        {
        }

        internal string PackageName { set; get; }

        /// <summary>
        ///     场景名称
        /// </summary>
        public string SceneName
        {
            get
            {
                if (IsValidWithWarning == false)
                    return string.Empty;
                return Provider.SceneName;
            }
        }

        /// <summary>
        ///     场景对象
        /// </summary>
        public Scene SceneObject
        {
            get
            {
                if (IsValidWithWarning == false)
                    return new Scene();
                return Provider.SceneObject;
            }
        }

        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        ///     完成委托
        /// </summary>
        public event Action<SceneHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                    throw new Exception($"{nameof(SceneHandle)} is invalid");
                if (Provider.IsDone)
                    value.Invoke(this);
                else
                    _callback += value;
            }
            remove
            {
                if (IsValidWithWarning == false)
                    throw new Exception($"{nameof(SceneHandle)} is invalid");
                _callback -= value;
            }
        }

        /// <summary>
        ///     等待异步执行完毕
        /// </summary>
        internal void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
                return;
            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        ///     激活场景（当同时存在多个场景时用于切换激活场景）
        /// </summary>
        public bool ActivateScene()
        {
            if (IsValidWithWarning == false)
                return false;

            if (SceneObject.IsValid() && SceneObject.isLoaded) return SceneManager.SetActiveScene(SceneObject);

            YooLogger.Warning($"Scene is invalid or not loaded : {SceneObject.name}");
            return false;
        }

        /// <summary>
        ///     解除场景加载挂起操作
        /// </summary>
        public bool UnSuspend()
        {
            if (IsValidWithWarning == false)
                return false;

            if (Provider is DatabaseSceneProvider)
            {
                var provider = Provider as DatabaseSceneProvider;
                provider.UnSuspendLoad();
            }
            else if (Provider is BundledSceneProvider)
            {
                var provider = Provider as BundledSceneProvider;
                provider.UnSuspendLoad();
            }
            else
            {
                throw new NotImplementedException();
            }

            return true;
        }

        /// <summary>
        ///     是否为主场景
        /// </summary>
        public bool IsMainScene()
        {
            if (IsValidWithWarning == false)
                return false;

            if (Provider is DatabaseSceneProvider)
            {
                var temp = Provider as DatabaseSceneProvider;
                return temp.SceneMode == LoadSceneMode.Single;
            }

            if (Provider is BundledSceneProvider)
            {
                var temp = Provider as BundledSceneProvider;
                return temp.SceneMode == LoadSceneMode.Single;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        ///     异步卸载子场景
        /// </summary>
        public UnloadSceneOperation UnloadAsync()
        {
            var packageName = GetAssetInfo().PackageName;

            // 如果句柄无效
            if (IsValidWithWarning == false)
            {
                var error = $"{nameof(SceneHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 如果是主场景
            if (IsMainScene())
            {
                var error =
                    $"Cannot unload main scene. Use {nameof(YooAssets.LoadSceneAsync)} method to change the main scene !";
                YooLogger.Error(error);
                var operation = new UnloadSceneOperation(error);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 卸载子场景
            {
                var operation = new UnloadSceneOperation(Provider);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }
        }
    }
}