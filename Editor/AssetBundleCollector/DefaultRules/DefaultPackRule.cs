using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
    public class DefaultPackRule
    {
        /// <summary>
        ///     AssetBundle文件的后缀名
        /// </summary>
        public const string AssetBundleFileExtension = "bundle";

        /// <summary>
        ///     原生文件的后缀名
        /// </summary>
        public const string RawFileExtension = "rawfile";

        /// <summary>
        ///     Unity着色器资源包名称
        /// </summary>
        public const string ShadersBundleName = "unityshaders";


        public static PackRuleResult CreateShadersPackRuleResult()
        {
            var result = new PackRuleResult(ShadersBundleName, AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     以文件路径作为资源包名
    ///     注意：每个文件独自打资源包
    ///     例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image_backgroud.bundle"
    ///     例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view_main.bundle"
    /// </summary>
    [DisplayName("资源包名: 文件路径")]
    public class PackSeparately : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var bundleName = PathUtility.RemoveExtension(data.AssetPath);
            var result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     以父类文件夹路径作为资源包名
    ///     注意：文件夹下所有文件打进一个资源包
    ///     例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image.bundle"
    ///     例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view.bundle"
    /// </summary>
    [DisplayName("资源包名: 父类文件夹路径")]
    public class PackDirectory : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var bundleName = Path.GetDirectoryName(data.AssetPath);
            var result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     以收集器路径下顶级文件夹为资源包名
    ///     注意：文件夹下所有文件打进一个资源包
    ///     例如：收集器路径为 "Assets/UIPanel"
    ///     例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
    ///     例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
    /// </summary>
    [DisplayName("资源包名: 收集器下顶级文件夹路径")]
    public class PackTopDirectory : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var assetPath = data.AssetPath.Replace(data.CollectPath, string.Empty);
            assetPath = assetPath.TrimStart('/');
            var splits = assetPath.Split('/');
            if (splits.Length > 0)
            {
                if (Path.HasExtension(splits[0]))
                    throw new Exception($"Not found root directory : {assetPath}");
                var bundleName = $"{data.CollectPath}/{splits[0]}";
                var result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
                return result;
            }

            throw new Exception($"Not found root directory : {assetPath}");
        }
    }

    /// <summary>
    ///     以收集器路径作为资源包名
    ///     注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 收集器路径")]
    public class PackCollector : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName;
            var collectPath = data.CollectPath;
            if (AssetDatabase.IsValidFolder(collectPath))
                bundleName = collectPath;
            else
                bundleName = PathUtility.RemoveExtension(collectPath);

            var result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     以分组名称作为资源包名
    ///     注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 分组名称")]
    public class PackGroup : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var bundleName = data.GroupName;
            var result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     打包原生文件
    /// </summary>
    [DisplayName("打包原生文件")]
    public class PackRawFile : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var bundleName = data.AssetPath;
            var result = new PackRuleResult(bundleName, DefaultPackRule.RawFileExtension);
            return result;
        }
    }

    /// <summary>
    ///     打包着色器
    /// </summary>
    [DisplayName("打包着色器文件")]
    public class PackShader : IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            return DefaultPackRule.CreateShadersPackRuleResult();
        }
    }

    /// <summary>
    ///     打包着色器变种集合
    /// </summary>
    [DisplayName("打包着色器变种集合文件")]
    public class PackShaderVariants : IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            return DefaultPackRule.CreateShadersPackRuleResult();
        }
    }
}