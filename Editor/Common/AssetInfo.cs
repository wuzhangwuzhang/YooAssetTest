using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    [Serializable]
    public class AssetInfo
    {
        /// <summary>
        ///     资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        ///     资源GUID
        /// </summary>
        public string AssetGUID;

        private string _fileExtension;

        /// <summary>
        ///     资源类型
        /// </summary>
        public Type AssetType;

        public AssetInfo(string assetPath)
        {
            AssetPath = assetPath;
            AssetGUID = AssetDatabase.AssetPathToGUID(AssetPath);
            AssetType = AssetDatabase.GetMainAssetTypeAtPath(AssetPath);
        }

        /// <summary>
        ///     文件格式
        /// </summary>
        public string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(_fileExtension))
                    _fileExtension = Path.GetExtension(AssetPath);
                return _fileExtension;
            }
        }

        /// <summary>
        ///     是否为着色器资源
        /// </summary>
        public bool IsShaderAsset()
        {
            if (AssetType == typeof(Shader) || AssetType == typeof(ShaderVariantCollection))
                return true;
            return false;
        }
    }
}