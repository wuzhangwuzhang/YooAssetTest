#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class UxmlLoader
    {
        private static readonly Dictionary<Type, string> _uxmlDic = new();

        /// <summary>
        ///     加载窗口的布局文件
        /// </summary>
        public static VisualTreeAsset LoadWindowUXML<TWindow>() where TWindow : class
        {
            var windowType = typeof(TWindow);

            // 缓存里查询并加载
            if (_uxmlDic.TryGetValue(windowType, out var uxmlGUID))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(uxmlGUID);
                if (string.IsNullOrEmpty(assetPath))
                {
                    _uxmlDic.Clear();
                    throw new Exception(
                        $"Invalid UXML GUID : {uxmlGUID} ! Please close the window and open it again !");
                }

                var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                return treeAsset;
            }

            // 全局搜索并加载
            var guids = AssetDatabase.FindAssets(windowType.Name);
            if (guids.Length == 0)
                throw new Exception($"Not found any assets : {windowType.Name}");

            foreach (var assetGUID in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType == typeof(VisualTreeAsset))
                {
                    _uxmlDic.Add(windowType, assetGUID);
                    var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                    return treeAsset;
                }
            }

            throw new Exception($"Not found UXML file : {windowType.Name}");
        }
    }
}
#endif