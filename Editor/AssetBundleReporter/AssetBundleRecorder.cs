﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset.Editor
{
    public static class AssetBundleRecorder
    {
        private static readonly Dictionary<string, AssetBundle> _loadedAssetBundles = new(1000);

        /// <summary>
        ///     获取AssetBundle对象，如果没有被缓存就重新加载。
        /// </summary>
        public static AssetBundle GetAssetBundle(string filePath)
        {
            // 如果文件不存在
            if (File.Exists(filePath) == false)
            {
                Debug.LogWarning($"Not found asset bundle file : {filePath}");
                return null;
            }

            // 验证文件有效性（可能文件被加密）
            var fileData = File.ReadAllBytes(filePath);
            if (EditorTools.CheckBundleFileValid(fileData) == false)
            {
                Debug.LogWarning($"The asset bundle file is invalid and may be encrypted : {filePath}");
                return null;
            }

            if (_loadedAssetBundles.TryGetValue(filePath, out var bundle))
            {
                return bundle;
            }

            var newBundle = AssetBundle.LoadFromFile(filePath);
            if (newBundle != null)
            {
                var assetNames = newBundle.GetAllAssetNames();
                foreach (var name in assetNames) newBundle.LoadAsset(name);
                _loadedAssetBundles.Add(filePath, newBundle);
            }

            return newBundle;
        }

        /// <summary>
        ///     卸载所有已经加载的AssetBundle文件
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var valuePair in _loadedAssetBundles)
                if (valuePair.Value != null)
                    valuePair.Value.Unload(true);
            _loadedAssetBundles.Clear();
        }
    }
}