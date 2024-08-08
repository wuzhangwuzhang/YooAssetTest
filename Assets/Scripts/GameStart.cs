using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class GameStart : MonoBehaviour
{
    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
    private string packageName = "DefaultPackage";

    void Awake()
    {
        Debug.Log($"资源系统运行模式：{PlayMode}");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);

        StartCoroutine(DownLoadAssetsByYooAssets(() =>
        {
            //初始化lua
            Debug.Log("Finish!!!");
        }));
    }
    
    IEnumerator DownLoadAssetsByYooAssets(Action onDownloadComplete)
    {
        // 1.初始化资源系统
        YooAssets.Initialize();

        // 创建资源包裹类
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
            package = YooAssets.CreatePackage(packageName);
        
        InitializationOperation initializationOperation = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            //注意：如果是原生文件系统选择EDefaultBuildPipeline.RawFileBuildPipeline
            var buildPipeline = EDefaultBuildPipeline.BuiltinBuildPipeline; 
            Debug.Log($"BuildPipeline:{buildPipeline}");
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
            initializationOperation = package.InitializeAsync(createParameters);
            yield return initializationOperation;
    
            if(initializationOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else 
                Debug.LogError($"资源包初始化失败：{initializationOperation.Error}");
        }
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            //联机运行模式
            // 注意：GameQueryServices.cs 太空战机的脚本类，详细见StreamingAssetsHelper.cs
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initializationOperation = package.InitializeAsync(createParameters);
            yield return initializationOperation;
            
            if(initializationOperation.Status == EOperationStatus.Succeed)
                Debug.Log("资源包初始化成功！");
            else 
                Debug.LogError($"资源包初始化失败：{initializationOperation.Error}");
        }
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            //单机模式
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initializationOperation = package.InitializeAsync(createParameters);
            yield return initializationOperation;
        }

        //2.获取资源版本号
        yield return new WaitForSecondsRealtime(0.5f);
        package = YooAssets.GetPackage(packageName);
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
        }
        else
        {
            var packageVersion = operation.PackageVersion;
            Debug.Log($"Request package version : {packageVersion}");
           
            yield return new WaitForSecondsRealtime(0.5f);

            package = YooAssets.GetPackage(packageName);
            var updatePackageManifestOperation = package.UpdatePackageManifestAsync(packageVersion);
            yield return updatePackageManifestOperation;
            
            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.5f);
                int downloadingMaxNum = 10;
                int failedTryAgain = 3;
                var packageNew = YooAssets.GetPackage(packageName);
                var downloader = packageNew.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
                if (downloader.TotalDownloadCount == 0)
                {
                    Debug.Log("Not found any download files !");
                }
                else
                {
                    // 发现新更新文件后，挂起流程系统
                    // 注意：开发者需要在下载前检测磁盘空间不足
                    int totalDownloadCount = downloader.TotalDownloadCount;
                    long totalDownloadBytes = downloader.TotalDownloadBytes;
                    Debug.Log($"更新文件数量: {totalDownloadCount}, 更新大小:{totalDownloadBytes/1024/1024}M");
                    
                    downloader.OnDownloadErrorCallback = (fileName, error) =>
                    {
                        Debug.LogError($"Download error:{fileName}, {error}");
                    };
                    downloader.OnDownloadProgressCallback = (totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes) =>
                    {
                        Debug.Log($"Download progress，文件数:{totalDownloadCount}, 已下载数量:{currentDownloadCount}, 已下载字节数：{currentDownloadBytes} 总大小：{totalDownloadBytes}");
                    };
                    
                    downloader.BeginDownload();
                    yield return downloader;

                    // 检测下载结果
                    if (downloader.Status != EOperationStatus.Succeed)
                        yield break;

                    package = YooAssets.GetPackage(packageName);
                    var clearUnusedBundleFilesAsync = package.ClearUnusedBundleFilesAsync();
                    clearUnusedBundleFilesAsync.Completed += (sender) =>
                    {
                        Debug.Log("ClearUnusedBundleFilesAsync");
                    };
                }
                YooAssets.SetDefaultPackage(package);
                onDownloadComplete?.Invoke();
            }
        }
    }
     
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    
    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {
        //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
        string hostServerIP = "http://127.0.0.1";
        string appVersion = "v1.0";

#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
    }
     
}
