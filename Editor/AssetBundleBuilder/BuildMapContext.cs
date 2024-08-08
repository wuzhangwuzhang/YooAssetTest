﻿using System;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class BuildMapContext : IContextObject
    {
        /// <summary>
        ///     资源包集合
        /// </summary>
        private readonly Dictionary<string, BuildBundleInfo> _bundleInfoDic = new(10000);

        /// <summary>
        ///     未被依赖的资源列表
        /// </summary>
        public readonly List<ReportIndependAsset> IndependAssets = new(1000);

        /// <summary>
        ///     参与构建的资源总数
        ///     说明：包括主动收集的资源以及其依赖的所有资源
        /// </summary>
        public int AssetFileCount;

        /// <summary>
        ///     资源收集命令
        /// </summary>
        public CollectCommand Command { set; get; }

        /// <summary>
        ///     资源包信息列表
        /// </summary>
        public Dictionary<string, BuildBundleInfo>.ValueCollection Collection => _bundleInfoDic.Values;


        /// <summary>
        ///     添加一个打包资源
        /// </summary>
        public void PackAsset(BuildAssetInfo assetInfo)
        {
            var bundleName = assetInfo.BundleName;
            if (string.IsNullOrEmpty(bundleName))
                throw new Exception("Should never get here !");

            if (_bundleInfoDic.TryGetValue(bundleName, out var bundleInfo))
            {
                bundleInfo.PackAsset(assetInfo);
            }
            else
            {
                var newBundleInfo = new BuildBundleInfo(bundleName);
                newBundleInfo.PackAsset(assetInfo);
                _bundleInfoDic.Add(bundleName, newBundleInfo);
            }
        }

        /// <summary>
        ///     是否包含资源包
        /// </summary>
        public bool IsContainsBundle(string bundleName)
        {
            return _bundleInfoDic.ContainsKey(bundleName);
        }

        /// <summary>
        ///     获取资源包信息，如果没找到返回NULL
        /// </summary>
        public BuildBundleInfo GetBundleInfo(string bundleName)
        {
            if (_bundleInfoDic.TryGetValue(bundleName, out var result)) return result;
            throw new Exception($"Should never get here ! Not found bundle : {bundleName}");
        }

        /// <summary>
        ///     获取构建管线里需要的数据
        /// </summary>
        public AssetBundleBuild[] GetPipelineBuilds()
        {
            var builds = new List<AssetBundleBuild>(_bundleInfoDic.Count);
            foreach (var bundleInfo in _bundleInfoDic.Values) builds.Add(bundleInfo.CreatePipelineBuild());
            return builds.ToArray();
        }

        /// <summary>
        ///     创建着色器信息类
        /// </summary>
        public void CreateShadersBundleInfo(string shadersBundleName)
        {
            if (IsContainsBundle(shadersBundleName) == false)
            {
                var shaderBundleInfo = new BuildBundleInfo(shadersBundleName);
                _bundleInfoDic.Add(shadersBundleName, shaderBundleInfo);
            }
        }
    }
}