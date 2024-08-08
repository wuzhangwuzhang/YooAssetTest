﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
    [Serializable]
    public class AssetBundleCollectorPackage
    {
        /// <summary>
        ///     包裹名称
        /// </summary>
        public string PackageName = string.Empty;

        /// <summary>
        ///     包裹描述
        /// </summary>
        public string PackageDesc = string.Empty;

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
        ///     自动收集所有着色器（所有着色器存储在一个资源包内）
        /// </summary>
        public bool AutoCollectShaders = true;

        /// <summary>
        ///     资源忽略规则名
        /// </summary>
        public string IgnoreRuleName = nameof(NormalIgnoreRule);

        /// <summary>
        ///     分组列表
        /// </summary>
        public List<AssetBundleCollectorGroup> Groups = new();


        /// <summary>
        ///     检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            if (string.IsNullOrEmpty(IgnoreRuleName))
            {
                throw new Exception($"{nameof(IgnoreRuleName)} is null or empty !");
            }

            if (AssetBundleCollectorSettingData.HasIgnoreRuleName(IgnoreRuleName) == false)
                throw new Exception(
                    $"Invalid {nameof(IIgnoreRule)} class type : {IgnoreRuleName} in package : {PackageName}");

            foreach (var group in Groups) group.CheckConfigError();
        }

        /// <summary>
        ///     修复配置错误
        /// </summary>
        public bool FixConfigError()
        {
            var isFixed = false;

            if (string.IsNullOrEmpty(IgnoreRuleName))
            {
                Debug.LogWarning($"Set the {nameof(IgnoreRuleName)} to {nameof(NormalIgnoreRule)}");
                IgnoreRuleName = nameof(NormalIgnoreRule);
                isFixed = true;
            }

            foreach (var group in Groups)
                if (group.FixConfigError())
                    isFixed = true;

            return isFixed;
        }

        /// <summary>
        ///     获取打包收集的资源文件
        /// </summary>
        public List<CollectAssetInfo> GetAllCollectAssets(CollectCommand command)
        {
            var result = new Dictionary<string, CollectAssetInfo>(10000);

            // 收集打包资源
            foreach (var group in Groups)
            {
                var temper = group.GetAllCollectAssets(command);
                foreach (var collectAsset in temper)
                    if (result.ContainsKey(collectAsset.AssetInfo.AssetPath) == false)
                        result.Add(collectAsset.AssetInfo.AssetPath, collectAsset);
                    else
                        throw new Exception(
                            $"The collecting asset file is existed : {collectAsset.AssetInfo.AssetPath}");
            }

            // 检测可寻址地址是否重复
            if (command.EnableAddressable)
            {
                var addressTemper = new Dictionary<string, string>();
                foreach (var collectInfoPair in result)
                    if (collectInfoPair.Value.CollectorType == ECollectorType.MainAssetCollector)
                    {
                        var address = collectInfoPair.Value.Address;
                        var assetPath = collectInfoPair.Value.AssetInfo.AssetPath;
                        if (string.IsNullOrEmpty(address))
                            continue;

                        if (addressTemper.TryGetValue(address, out var existed) == false)
                            addressTemper.Add(address, assetPath);
                        else
                            throw new Exception(
                                $"The address is existed : {address} \nAssetPath:\n     {existed}\n     {assetPath}");
                    }
            }

            // 返回列表
            return result.Values.ToList();
        }

        /// <summary>
        ///     获取所有的资源标签
        /// </summary>
        public List<string> GetAllTags()
        {
            var result = new HashSet<string>();
            foreach (var group in Groups)
            {
                var groupTags = EditorTools.StringToStringList(group.AssetTags, ';');
                foreach (var tag in groupTags)
                    if (result.Contains(tag) == false)
                        result.Add(tag);

                foreach (var collector in group.Collectors)
                {
                    var collectorTags = EditorTools.StringToStringList(collector.AssetTags, ';');
                    foreach (var tag in collectorTags)
                        if (result.Contains(tag) == false)
                            result.Add(tag);
                }
            }

            return result.ToList();
        }
    }
}