﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace YooAsset
{
    /// <summary>
    ///     清单文件
    /// </summary>
    [Serializable]
    internal class PackageManifest
    {
        /// <summary>
        ///     文件版本
        /// </summary>
        public string FileVersion;

        /// <summary>
        ///     启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        ///     资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower;

        /// <summary>
        ///     包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID;

        /// <summary>
        ///     文件名称样式
        /// </summary>
        public int OutputNameStyle;

        /// <summary>
        ///     构建管线名称
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        ///     资源包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        ///     资源包裹的版本信息
        /// </summary>
        public string PackageVersion;

        /// <summary>
        ///     资源列表（主动收集的资源列表）
        /// </summary>
        public List<PackageAsset> AssetList = new();

        /// <summary>
        ///     资源包列表
        /// </summary>
        public List<PackageBundle> BundleList = new();

        /// <summary>
        ///     资源映射集合（提供AssetPath获取PackageAsset）
        /// </summary>
        [NonSerialized] public Dictionary<string, PackageAsset> AssetDic;

        /// <summary>
        ///     资源路径映射集合（提供Location获取AssetPath）
        /// </summary>
        [NonSerialized] public Dictionary<string, string> AssetPathMapping1;

        /// <summary>
        ///     资源路径映射集合（提供AssetGUID获取AssetPath）
        /// </summary>
        [NonSerialized] public Dictionary<string, string> AssetPathMapping2;


        /// <summary>
        ///     资源包集合（提供BundleName获取PackageBundle）
        /// </summary>
        [NonSerialized] public Dictionary<string, PackageBundle> BundleDic1;

        /// <summary>
        ///     资源包集合（提供FileName获取PackageBundle）
        /// </summary>
        [NonSerialized] public Dictionary<string, PackageBundle> BundleDic2;

        /// <summary>
        ///     资源包集合（提供BundleGUID获取PackageBundle）
        /// </summary>
        [NonSerialized] public Dictionary<string, PackageBundle> BundleDic3;


        /// <summary>
        ///     尝试映射为资源路径
        /// </summary>
        public string TryMappingToAssetPath(string location)
        {
            if (string.IsNullOrEmpty(location))
                return string.Empty;

            if (LocationToLower)
                location = location.ToLower();

            if (AssetPathMapping1.TryGetValue(location, out var assetPath))
                return assetPath;
            return string.Empty;
        }

        /// <summary>
        ///     获取主资源包
        ///     注意：传入的资源路径一定合法有效！
        /// </summary>
        public PackageBundle GetMainPackageBundle(string assetPath)
        {
            if (AssetDic.TryGetValue(assetPath, out var packageAsset))
            {
                var bundleID = packageAsset.BundleID;
                if (bundleID >= 0 && bundleID < BundleList.Count)
                {
                    var packageBundle = BundleList[bundleID];
                    return packageBundle;
                }

                throw new Exception($"Invalid bundle id : {bundleID} Asset path : {assetPath}");
            }

            throw new Exception("Should never get here !");
        }

        /// <summary>
        ///     获取资源依赖列表
        ///     注意：传入的资源路径一定合法有效！
        /// </summary>
        public PackageBundle[] GetAllDependencies(string assetPath)
        {
            var packageBundle = GetMainPackageBundle(assetPath);
            var result = new List<PackageBundle>(packageBundle.DependIDs.Length);
            foreach (var dependID in packageBundle.DependIDs)
                if (dependID >= 0 && dependID < BundleList.Count)
                {
                    var dependBundle = BundleList[dependID];
                    result.Add(dependBundle);
                }
                else
                {
                    throw new Exception($"Invalid bundle id : {dependID} Asset path : {assetPath}");
                }

            return result.ToArray();
        }

        /// <summary>
        ///     尝试获取包裹的资源
        /// </summary>
        public bool TryGetPackageAsset(string assetPath, out PackageAsset result)
        {
            return AssetDic.TryGetValue(assetPath, out result);
        }

        /// <summary>
        ///     尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByBundleName(string bundleName, out PackageBundle result)
        {
            return BundleDic1.TryGetValue(bundleName, out result);
        }

        /// <summary>
        ///     尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByFileName(string fileName, out PackageBundle result)
        {
            return BundleDic2.TryGetValue(fileName, out result);
        }

        /// <summary>
        ///     尝试获取包裹的资源包
        /// </summary>
        public bool TryGetPackageBundleByBundleGUID(string bundleGUID, out PackageBundle result)
        {
            return BundleDic3.TryGetValue(bundleGUID, out result);
        }

        /// <summary>
        ///     是否包含资源文件
        /// </summary>
        public bool IsIncludeBundleFile(string bundleGUID)
        {
            return BundleDic3.ContainsKey(bundleGUID);
        }

        /// <summary>
        ///     获取资源信息列表
        /// </summary>
        public AssetInfo[] GetAssetsInfoByTags(string[] tags)
        {
            var result = new List<AssetInfo>(100);
            foreach (var packageAsset in AssetList)
                if (packageAsset.HasTag(tags))
                {
                    var assetInfo = new AssetInfo(PackageName, packageAsset, null);
                    result.Add(assetInfo);
                }

            return result.ToArray();
        }

        /// <summary>
        ///     资源定位地址转换为资源信息。
        /// </summary>
        /// <returns>如果转换失败会返回一个无效的资源信息类</returns>
        public AssetInfo ConvertLocationToAssetInfo(string location, Type assetType)
        {
            DebugCheckLocation(location);

            var assetPath = ConvertLocationToAssetInfoMapping(location);
            if (TryGetPackageAsset(assetPath, out var packageAsset))
            {
                var assetInfo = new AssetInfo(PackageName, packageAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error;
                if (string.IsNullOrEmpty(location))
                    error = "The location is null or empty !";
                else
                    error = $"The location is invalid : {location}";
                var assetInfo = new AssetInfo(PackageName, error);
                return assetInfo;
            }
        }

        private string ConvertLocationToAssetInfoMapping(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                YooLogger.Error("Failed to mapping location to asset path, The location is null or empty.");
                return string.Empty;
            }

            if (LocationToLower)
                location = location.ToLower();

            if (AssetPathMapping1.TryGetValue(location, out var assetPath)) return assetPath;

            YooLogger.Warning($"Failed to mapping location to asset path : {location}");
            return string.Empty;
        }

        /// <summary>
        ///     资源GUID转换为资源信息。
        /// </summary>
        /// <returns>如果转换失败会返回一个无效的资源信息类</returns>
        public AssetInfo ConvertAssetGUIDToAssetInfo(string assetGUID, Type assetType)
        {
            if (IncludeAssetGUID == false)
            {
                YooLogger.Warning(
                    "Package manifest not include asset guid ! Please check asset bundle collector settings.");
                var assetInfo = new AssetInfo(PackageName, "AssetGUID data is empty !");
                return assetInfo;
            }

            var assetPath = ConvertAssetGUIDToAssetInfoMapping(assetGUID);
            if (TryGetPackageAsset(assetPath, out var packageAsset))
            {
                var assetInfo = new AssetInfo(PackageName, packageAsset, assetType);
                return assetInfo;
            }
            else
            {
                string error;
                if (string.IsNullOrEmpty(assetGUID))
                    error = "The assetGUID is null or empty !";
                else
                    error = $"The assetGUID is invalid : {assetGUID}";
                var assetInfo = new AssetInfo(PackageName, error);
                return assetInfo;
            }
        }

        private string ConvertAssetGUIDToAssetInfoMapping(string assetGUID)
        {
            if (string.IsNullOrEmpty(assetGUID))
            {
                YooLogger.Error("Failed to mapping assetGUID to asset path, The assetGUID is null or empty.");
                return string.Empty;
            }

            if (AssetPathMapping2.TryGetValue(assetGUID, out var assetPath)) return assetPath;

            YooLogger.Warning($"Failed to mapping assetGUID to asset path : {assetGUID}");
            return string.Empty;
        }

        /// <summary>
        ///     获取资源包内的主资源列表
        /// </summary>
        public string[] GetBundleIncludeAssets(string assetPath)
        {
            var assetList = new List<string>();
            if (TryGetPackageAsset(assetPath, out var result))
                foreach (var packageAsset in AssetList)
                    if (packageAsset.BundleID == result.BundleID)
                        assetList.Add(packageAsset.AssetPath);
            return assetList.ToArray();
        }

        #region 调试方法

        [Conditional("DEBUG")]
        private void DebugCheckLocation(string location)
        {
            if (string.IsNullOrEmpty(location) == false)
            {
                // 检查路径末尾是否有空格
                var index = location.LastIndexOf(' ');
                if (index != -1)
                    if (location.Length == index + 1)
                        YooLogger.Warning($"Found blank character in location : \"{location}\"");

                if (location.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                    YooLogger.Warning($"Found illegal character in location : \"{location}\"");
            }
        }

        #endregion
    }
}