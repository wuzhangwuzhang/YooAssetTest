using System;
using System.Collections.Generic;
using System.Linq;

namespace YooAsset.Editor
{
    public class ManifestContext : IContextObject
    {
        internal PackageManifest Manifest;
    }

    public abstract class TaskCreateManifest
    {
        private readonly Dictionary<int, HashSet<string>> _cacheBundleTags = new(10000);
        private readonly Dictionary<string, int> _cachedBundleID = new(10000);

        /// <summary>
        ///     创建补丁清单文件到输出目录
        /// </summary>
        protected void CreateManifestFile(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();

            // 创建新补丁清单
            var manifest = new PackageManifest();
            manifest.FileVersion = YooAssetSettings.ManifestFileVersion;
            manifest.EnableAddressable = buildMapContext.Command.EnableAddressable;
            manifest.LocationToLower = buildMapContext.Command.LocationToLower;
            manifest.IncludeAssetGUID = buildMapContext.Command.IncludeAssetGUID;
            manifest.OutputNameStyle = (int)buildParameters.FileNameStyle;
            manifest.BuildPipeline = buildParameters.BuildPipeline;
            manifest.PackageName = buildParameters.PackageName;
            manifest.PackageVersion = buildParameters.PackageVersion;
            manifest.BundleList = GetAllPackageBundle(buildMapContext);
            manifest.AssetList = GetAllPackageAsset(buildMapContext);

            if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
            {
                // 处理资源包的依赖列表
                ProcessBundleDepends(context, manifest);

                // 处理资源包的标签集合
                ProcessBundleTags(manifest);
            }

            // 创建补丁清单文本文件
            {
                var fileName =
                    YooAssetSettingsData.GetManifestJsonFileName(buildParameters.PackageName,
                        buildParameters.PackageVersion);
                var filePath = $"{packageOutputDirectory}/{fileName}";
                ManifestTools.SerializeToJson(filePath, manifest);
                BuildLogger.Log($"Create package manifest file: {filePath}");
            }

            // 创建补丁清单二进制文件
            string packageHash;
            {
                var fileName = YooAssetSettingsData.GetManifestBinaryFileName(buildParameters.PackageName,
                    buildParameters.PackageVersion);
                var filePath = $"{packageOutputDirectory}/{fileName}";
                ManifestTools.SerializeToBinary(filePath, manifest);
                packageHash = HashUtility.FileMD5(filePath);
                BuildLogger.Log($"Create package manifest file: {filePath}");

                var manifestContext = new ManifestContext();
                var bytesData = FileUtility.ReadAllBytes(filePath);
                manifestContext.Manifest = ManifestTools.DeserializeFromBinary(bytesData);
                context.SetContextObject(manifestContext);
            }

            // 创建补丁清单哈希文件
            {
                var fileName =
                    YooAssetSettingsData.GetPackageHashFileName(buildParameters.PackageName,
                        buildParameters.PackageVersion);
                var filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, packageHash);
                BuildLogger.Log($"Create package manifest hash file: {filePath}");
            }

            // 创建补丁清单版本文件
            {
                var fileName = YooAssetSettingsData.GetPackageVersionFileName(buildParameters.PackageName);
                var filePath = $"{packageOutputDirectory}/{fileName}";
                FileUtility.WriteAllText(filePath, buildParameters.PackageVersion);
                BuildLogger.Log($"Create package manifest version file: {filePath}");
            }
        }

        /// <summary>
        ///     获取资源包的依赖集合
        /// </summary>
        protected abstract string[] GetBundleDepends(BuildContext context, string bundleName);

        /// <summary>
        ///     获取主资源对象列表
        /// </summary>
        private List<PackageAsset> GetAllPackageAsset(BuildMapContext buildMapContext)
        {
            var result = new List<PackageAsset>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var assetInfos = bundleInfo.GetAllManifestAssetInfos();
                foreach (var assetInfo in assetInfos)
                {
                    var packageAsset = new PackageAsset();
                    packageAsset.Address = buildMapContext.Command.EnableAddressable ? assetInfo.Address : string.Empty;
                    packageAsset.AssetPath = assetInfo.AssetInfo.AssetPath;
                    packageAsset.AssetGUID = buildMapContext.Command.IncludeAssetGUID
                        ? assetInfo.AssetInfo.AssetGUID
                        : string.Empty;
                    packageAsset.AssetTags = assetInfo.AssetTags.ToArray();
                    packageAsset.BundleID = GetCachedBundleID(assetInfo.BundleName);
                    result.Add(packageAsset);
                }
            }

            return result;
        }

        /// <summary>
        ///     获取资源包列表
        /// </summary>
        private List<PackageBundle> GetAllPackageBundle(BuildMapContext buildMapContext)
        {
            var result = new List<PackageBundle>(1000);
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                var packageBundle = bundleInfo.CreatePackageBundle();
                result.Add(packageBundle);
            }

            // 注意：缓存资源包索引
            for (var index = 0; index < result.Count; index++)
            {
                var bundleName = result[index].BundleName;
                _cachedBundleID.Add(bundleName, index);
            }

            return result;
        }

        /// <summary>
        ///     处理资源包的依赖集合
        /// </summary>
        private void ProcessBundleDepends(BuildContext context, PackageManifest manifest)
        {
            // 查询引擎生成的资源包依赖关系，然后记录到清单
            foreach (var packageBundle in manifest.BundleList)
            {
                var mainBundleID = GetCachedBundleID(packageBundle.BundleName);
                var depends = GetBundleDepends(context, packageBundle.BundleName);
                var dependIDs = new List<int>(depends.Length);
                foreach (var dependBundleName in depends)
                {
                    var bundleID = GetCachedBundleID(dependBundleName);
                    if (bundleID != mainBundleID)
                        dependIDs.Add(bundleID);
                }

                packageBundle.DependIDs = dependIDs.ToArray();
            }
        }

        /// <summary>
        ///     处理资源包的标签集合
        /// </summary>
        private void ProcessBundleTags(PackageManifest manifest)
        {
            // 将主资源的标签信息传染给其依赖的资源包集合
            foreach (var packageAsset in manifest.AssetList)
            {
                var assetTags = packageAsset.AssetTags;
                var bundleID = packageAsset.BundleID;
                CacheBundleTags(bundleID, assetTags);

                var packageBundle = manifest.BundleList[bundleID];
                foreach (var dependBundleID in packageBundle.DependIDs) CacheBundleTags(dependBundleID, assetTags);
            }

            for (var index = 0; index < manifest.BundleList.Count; index++)
            {
                var packageBundle = manifest.BundleList[index];
                if (_cacheBundleTags.TryGetValue(index, out var value))
                {
                    packageBundle.Tags = value.ToArray();
                }
                else
                {
                    // 注意：SBP构建管线会自动剔除一些冗余资源的引用关系，导致游离资源包没有被任何主资源包引用。
                    var warning = BuildLogger.GetErrorMessage(ErrorCode.FoundStrayBundle,
                        $"Found stray bundle ! Bundle ID : {index} Bundle name : {packageBundle.BundleName}");
                    BuildLogger.Warning(warning);
                }
            }
        }

        private void CacheBundleTags(int bundleID, string[] assetTags)
        {
            if (_cacheBundleTags.ContainsKey(bundleID) == false)
                _cacheBundleTags.Add(bundleID, new HashSet<string>());

            foreach (var assetTag in assetTags)
                if (_cacheBundleTags[bundleID].Contains(assetTag) == false)
                    _cacheBundleTags[bundleID].Add(assetTag);
        }

        /// <summary>
        ///     获取资源包的索引ID
        /// </summary>
        private int GetCachedBundleID(string bundleName)
        {
            if (_cachedBundleID.TryGetValue(bundleName, out var value) == false)
                throw new Exception($"Should never get here ! Not found bundle ID : {bundleName}");
            return value;
        }
    }
}