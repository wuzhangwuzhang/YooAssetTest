﻿using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal class ResourceManager
    {
        // 全局场景句柄集合
        private static readonly Dictionary<string, SceneHandle> _sceneHandles = new(100);
        private static long _sceneCreateCount;
        internal readonly Dictionary<string, LoadBundleFileOperation> _loaderDic = new(5000);

        internal readonly Dictionary<string, ProviderOperation> _providerDic = new(5000);

        /// <summary>
        ///     所属包裹
        /// </summary>
        public readonly string PackageName;

        private IBundleQuery _bundleQuery;

        private bool _simulationOnEditor;


        public ResourceManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize(InitializeParameters initializeParameters, IBundleQuery bundleServices)
        {
            _simulationOnEditor = initializeParameters is EditorSimulateModeParameters;
            _bundleQuery = bundleServices;
        }

        /// <summary>
        ///     尝试卸载指定资源的资源包（包括依赖资源）
        /// </summary>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to unload asset ! {assetInfo.Error}");
                return;
            }

            // 卸载主资源包加载器
            var mainBundleName = _bundleQuery.GetMainBundleName(assetInfo);
            var mainLoader = TryGetFileLoader(mainBundleName);
            if (mainLoader != null)
            {
                mainLoader.TryDestroyProviders();
                if (mainLoader.CanDestroyLoader())
                {
                    var bundleName = mainLoader.BundleFileInfo.Bundle.BundleName;
                    mainLoader.DestroyLoader();
                    _loaderDic.Remove(bundleName);
                }
            }

            // 卸载依赖资源包加载器
            var dependBundleNames = _bundleQuery.GetDependBundleNames(assetInfo);
            foreach (var dependBundleName in dependBundleNames)
            {
                var dependLoader = TryGetFileLoader(dependBundleName);
                if (dependLoader != null)
                    if (dependLoader.CanDestroyLoader())
                    {
                        var bundleName = dependLoader.BundleFileInfo.Bundle.BundleName;
                        dependLoader.DestroyLoader();
                        _loaderDic.Remove(bundleName);
                    }
            }
        }

        /// <summary>
        ///     加载场景对象
        ///     注意：返回的场景句柄是唯一的，每个场景句柄对应自己的场景提供者对象。
        ///     注意：业务逻辑层应该避免同时加载一个子场景。
        /// </summary>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool suspendLoad,
            uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load scene ! {assetInfo.Error}");
                var completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SceneHandle>();
            }

            // 如果加载的是主场景，则卸载所有缓存的场景
            if (loadSceneParams.loadSceneMode == LoadSceneMode.Single) UnloadAllScene();

            // 注意：同一个场景的ProviderGUID每次加载都会变化
            var providerGUID = $"{assetInfo.GUID}-{++_sceneCreateCount}";
            ProviderOperation provider;
            {
                if (_simulationOnEditor)
                    provider = new DatabaseSceneProvider(this, providerGUID, assetInfo, loadSceneParams, suspendLoad);
                else
                    provider = new BundledSceneProvider(this, providerGUID, assetInfo, loadSceneParams, suspendLoad);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            var handle = provider.CreateHandle<SceneHandle>();
            handle.PackageName = PackageName;
            _sceneHandles.Add(providerGUID, handle);
            return handle;
        }

        /// <summary>
        ///     加载资源对象
        /// </summary>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load asset ! {assetInfo.Error}");
                var completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<AssetHandle>();
            }

            var providerGUID = nameof(LoadAssetAsync) + assetInfo.GUID;
            var provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseAssetProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledAssetProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AssetHandle>();
        }

        /// <summary>
        ///     加载子资源对象
        /// </summary>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load sub assets ! {assetInfo.Error}");
                var completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<SubAssetsHandle>();
            }

            var providerGUID = nameof(LoadSubAssetsAsync) + assetInfo.GUID;
            var provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseSubAssetsProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledSubAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<SubAssetsHandle>();
        }

        /// <summary>
        ///     加载所有资源对象
        /// </summary>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load all assets ! {assetInfo.Error}");
                var completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<AllAssetsHandle>();
            }

            var providerGUID = nameof(LoadAllAssetsAsync) + assetInfo.GUID;
            var provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseAllAssetsProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledAllAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AllAssetsHandle>();
        }

        /// <summary>
        ///     加载原生文件
        /// </summary>
        public RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load raw file ! {assetInfo.Error}");
                var completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompleted(assetInfo.Error);
                return completedProvider.CreateHandle<RawFileHandle>();
            }

            var providerGUID = nameof(LoadRawFileAsync) + assetInfo.GUID;
            var provider = TryGetProvider(providerGUID);
            if (provider == null)
            {
                if (_simulationOnEditor)
                    provider = new DatabaseRawFileProvider(this, providerGUID, assetInfo);
                else
                    provider = new BundledRawFileProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                _providerDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<RawFileHandle>();
        }


        internal void UnloadSubScene(string sceneName)
        {
            var removeKeys = new List<string>();
            foreach (var valuePair in _sceneHandles)
            {
                var sceneHandle = valuePair.Value;
                if (sceneHandle.SceneName == sceneName)
                {
                    // 释放子场景句柄
                    sceneHandle.ReleaseInternal();
                    removeKeys.Add(valuePair.Key);
                }
            }

            foreach (var key in removeKeys) _sceneHandles.Remove(key);
        }

        internal void UnloadAllScene()
        {
            // 释放所有场景句柄
            foreach (var valuePair in _sceneHandles) valuePair.Value.ReleaseInternal();
            _sceneHandles.Clear();
        }

        internal void ClearSceneHandle()
        {
            // 释放资源包下的所有场景
            if (_bundleQuery.ManifestValid())
            {
                var packageName = PackageName;
                var removeList = new List<string>();
                foreach (var valuePair in _sceneHandles)
                    if (valuePair.Value.PackageName == packageName)
                        removeList.Add(valuePair.Key);
                foreach (var key in removeList) _sceneHandles.Remove(key);
            }
        }

        internal LoadBundleFileOperation CreateMainBundleFileLoader(AssetInfo assetInfo)
        {
            var bundleInfo = _bundleQuery.GetMainBundleInfo(assetInfo);
            return CreateFileLoaderInternal(bundleInfo);
        }

        internal LoadDependBundleFileOperation CreateDependFileLoaders(AssetInfo assetInfo)
        {
            var bundleInfos = _bundleQuery.GetDependBundleInfos(assetInfo);
            var depends = new List<LoadBundleFileOperation>(bundleInfos.Length);
            foreach (var bundleInfo in bundleInfos)
            {
                var dependLoader = CreateFileLoaderInternal(bundleInfo);
                depends.Add(dependLoader);
            }

            var operation = new LoadDependBundleFileOperation(depends);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        internal void RemoveBundleProviders(List<ProviderOperation> removeList)
        {
            foreach (var provider in removeList) _providerDic.Remove(provider.ProviderGUID);
        }

        internal bool HasAnyLoader()
        {
            return _loaderDic.Count > 0;
        }

        private LoadBundleFileOperation CreateFileLoaderInternal(BundleInfo bundleInfo)
        {
            // 如果加载器已经存在
            var bundleName = bundleInfo.Bundle.BundleName;
            var loaderOperation = TryGetFileLoader(bundleName);
            if (loaderOperation != null)
                return loaderOperation;

            // 新增下载需求
            loaderOperation = new LoadBundleFileOperation(this, bundleInfo);
            OperationSystem.StartOperation(PackageName, loaderOperation);
            _loaderDic.Add(bundleName, loaderOperation);
            return loaderOperation;
        }

        private LoadBundleFileOperation TryGetFileLoader(string bundleName)
        {
            if (_loaderDic.TryGetValue(bundleName, out var value))
                return value;
            return null;
        }

        private ProviderOperation TryGetProvider(string providerGUID)
        {
            if (_providerDic.TryGetValue(providerGUID, out var value))
                return value;
            return null;
        }

        #region 调试信息

        internal List<DebugProviderInfo> GetDebugReportInfos()
        {
            var result = new List<DebugProviderInfo>(_providerDic.Count);
            foreach (var provider in _providerDic.Values)
            {
                var providerInfo = new DebugProviderInfo();
                providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
                providerInfo.SpawnScene = provider.SpawnScene;
                providerInfo.SpawnTime = provider.SpawnTime;
                providerInfo.LoadingTime = provider.LoadingTime;
                providerInfo.RefCount = provider.RefCount;
                providerInfo.Status = provider.Status.ToString();
                providerInfo.DependBundleInfos = new List<DebugBundleInfo>();
                provider.GetBundleDebugInfos(providerInfo.DependBundleInfos);
                result.Add(providerInfo);
            }

            return result;
        }

        #endregion
    }
}