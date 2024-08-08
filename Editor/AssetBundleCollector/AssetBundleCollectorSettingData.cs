using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class AssetBundleCollectorSettingData
    {
        private static readonly Dictionary<string, Type> _cacheActiveRuleTypes = new();
        private static readonly Dictionary<string, IActiveRule> _cacheActiveRuleInstance = new();

        private static readonly Dictionary<string, Type> _cacheAddressRuleTypes = new();
        private static readonly Dictionary<string, IAddressRule> _cacheAddressRuleInstance = new();

        private static readonly Dictionary<string, Type> _cachePackRuleTypes = new();
        private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new();

        private static readonly Dictionary<string, Type> _cacheFilterRuleTypes = new();
        private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new();

        private static readonly Dictionary<string, Type> _cacheIgnoreRuleTypes = new();
        private static readonly Dictionary<string, IIgnoreRule> _cacheIgnoreRuleInstance = new();

        private static AssetBundleCollectorSetting _setting;


        static AssetBundleCollectorSettingData()
        {
            // IPackRule
            {
                // 清空缓存集合
                _cachePackRuleTypes.Clear();
                _cachePackRuleInstance.Clear();

                // 获取所有类型
                var types = new List<Type>(100)
                {
                    typeof(PackSeparately),
                    typeof(PackDirectory),
                    typeof(PackTopDirectory),
                    typeof(PackCollector),
                    typeof(PackGroup),
                    typeof(PackRawFile),
                    typeof(PackShaderVariants)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IPackRule));
                types.AddRange(customTypes);
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (_cachePackRuleTypes.ContainsKey(type.Name) == false)
                        _cachePackRuleTypes.Add(type.Name, type);
                }
            }

            // IFilterRule
            {
                // 清空缓存集合
                _cacheFilterRuleTypes.Clear();
                _cacheFilterRuleInstance.Clear();

                // 获取所有类型
                var types = new List<Type>(100)
                {
                    typeof(CollectAll),
                    typeof(CollectScene),
                    typeof(CollectPrefab),
                    typeof(CollectSprite)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IFilterRule));
                types.AddRange(customTypes);
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (_cacheFilterRuleTypes.ContainsKey(type.Name) == false)
                        _cacheFilterRuleTypes.Add(type.Name, type);
                }
            }

            // IAddressRule
            {
                // 清空缓存集合
                _cacheAddressRuleTypes.Clear();
                _cacheAddressRuleInstance.Clear();

                // 获取所有类型
                var types = new List<Type>(100)
                {
                    typeof(AddressByFileName),
                    typeof(AddressByFolderAndFileName),
                    typeof(AddressByGroupAndFileName),
                    typeof(AddressDisable)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IAddressRule));
                types.AddRange(customTypes);
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (_cacheAddressRuleTypes.ContainsKey(type.Name) == false)
                        _cacheAddressRuleTypes.Add(type.Name, type);
                }
            }

            // IActiveRule
            {
                // 清空缓存集合
                _cacheActiveRuleTypes.Clear();
                _cacheActiveRuleInstance.Clear();

                // 获取所有类型
                var types = new List<Type>(100)
                {
                    typeof(EnableGroup),
                    typeof(DisableGroup)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IActiveRule));
                types.AddRange(customTypes);
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (_cacheActiveRuleTypes.ContainsKey(type.Name) == false)
                        _cacheActiveRuleTypes.Add(type.Name, type);
                }
            }

            // IIgnoreRule
            {
                // 清空缓存集合
                _cacheIgnoreRuleTypes.Clear();
                _cacheIgnoreRuleInstance.Clear();

                // 获取所有类型
                var types = new List<Type>(100)
                {
                    typeof(NormalIgnoreRule),
                    typeof(RawFileIgnoreRule)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IIgnoreRule));
                types.AddRange(customTypes);
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (_cacheIgnoreRuleTypes.ContainsKey(type.Name) == false)
                        _cacheIgnoreRuleTypes.Add(type.Name, type);
                }
            }
        }

        /// <summary>
        ///     配置数据是否被修改
        /// </summary>
        public static bool IsDirty { private set; get; }

        public static AssetBundleCollectorSetting Setting
        {
            get
            {
                if (_setting == null)
                    _setting = SettingLoader.LoadSettingData<AssetBundleCollectorSetting>();
                return _setting;
            }
        }

        /// <summary>
        ///     存储配置文件
        /// </summary>
        public static void SaveFile()
        {
            if (Setting != null)
            {
                IsDirty = false;
                EditorUtility.SetDirty(Setting);
                AssetDatabase.SaveAssets();
                Debug.Log($"{nameof(AssetBundleCollectorSetting)}.asset is saved!");
            }
        }

        /// <summary>
        ///     修复配置文件
        /// </summary>
        public static void FixFile()
        {
            var isFixed = Setting.FixAllPackageConfigError();
            if (isFixed)
            {
                IsDirty = true;
                Debug.Log("Fix package config error done !");
            }
        }

        /// <summary>
        ///     清空所有数据
        /// </summary>
        public static void ClearAll()
        {
            Setting.ClearAll();
            SaveFile();
        }

        public static List<RuleDisplayName> GetActiveRuleNames()
        {
            var names = new List<RuleDisplayName>();
            foreach (var pair in _cacheActiveRuleTypes)
            {
                var ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }

            return names;
        }

        public static List<RuleDisplayName> GetAddressRuleNames()
        {
            var names = new List<RuleDisplayName>();
            foreach (var pair in _cacheAddressRuleTypes)
            {
                var ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }

            return names;
        }

        public static List<RuleDisplayName> GetPackRuleNames()
        {
            var names = new List<RuleDisplayName>();
            foreach (var pair in _cachePackRuleTypes)
            {
                var ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }

            return names;
        }

        public static List<RuleDisplayName> GetFilterRuleNames()
        {
            var names = new List<RuleDisplayName>();
            foreach (var pair in _cacheFilterRuleTypes)
            {
                var ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }

            return names;
        }

        public static List<RuleDisplayName> GetIgnoreRuleNames()
        {
            var names = new List<RuleDisplayName>();
            foreach (var pair in _cacheIgnoreRuleTypes)
            {
                var ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }

            return names;
        }

        private static string GetRuleDisplayName(string name, Type type)
        {
            var attribute = DisplayNameAttributeHelper.GetAttribute<DisplayNameAttribute>(type);
            if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
                return attribute.DisplayName;
            return name;
        }

        public static bool HasActiveRuleName(string ruleName)
        {
            return _cacheActiveRuleTypes.ContainsKey(ruleName);
        }

        public static bool HasAddressRuleName(string ruleName)
        {
            return _cacheAddressRuleTypes.ContainsKey(ruleName);
        }

        public static bool HasPackRuleName(string ruleName)
        {
            return _cachePackRuleTypes.ContainsKey(ruleName);
        }

        public static bool HasFilterRuleName(string ruleName)
        {
            return _cacheFilterRuleTypes.ContainsKey(ruleName);
        }

        public static bool HasIgnoreRuleName(string ruleName)
        {
            return _cacheIgnoreRuleTypes.ContainsKey(ruleName);
        }

        public static IActiveRule GetActiveRuleInstance(string ruleName)
        {
            if (_cacheActiveRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheActiveRuleTypes.TryGetValue(ruleName, out var type))
            {
                instance = (IActiveRule)Activator.CreateInstance(type);
                _cacheActiveRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IActiveRule)} is invalid：{ruleName}");
        }

        public static IAddressRule GetAddressRuleInstance(string ruleName)
        {
            if (_cacheAddressRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheAddressRuleTypes.TryGetValue(ruleName, out var type))
            {
                instance = (IAddressRule)Activator.CreateInstance(type);
                _cacheAddressRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IAddressRule)} is invalid：{ruleName}");
        }

        public static IPackRule GetPackRuleInstance(string ruleName)
        {
            if (_cachePackRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cachePackRuleTypes.TryGetValue(ruleName, out var type))
            {
                instance = (IPackRule)Activator.CreateInstance(type);
                _cachePackRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IPackRule)} is invalid：{ruleName}");
        }

        public static IFilterRule GetFilterRuleInstance(string ruleName)
        {
            if (_cacheFilterRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheFilterRuleTypes.TryGetValue(ruleName, out var type))
            {
                instance = (IFilterRule)Activator.CreateInstance(type);
                _cacheFilterRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IFilterRule)} is invalid：{ruleName}");
        }

        public static IIgnoreRule GetIgnoreRuleInstance(string ruleName)
        {
            if (_cacheIgnoreRuleInstance.TryGetValue(ruleName, out var instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheIgnoreRuleTypes.TryGetValue(ruleName, out var type))
            {
                instance = (IIgnoreRule)Activator.CreateInstance(type);
                _cacheIgnoreRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IIgnoreRule)} is invalid：{ruleName}");
        }

        // 公共参数编辑相关
        public static void ModifyShowPackageView(bool showPackageView)
        {
            Setting.ShowPackageView = showPackageView;
            IsDirty = true;
        }

        public static void ModifyShowEditorAlias(bool showAlias)
        {
            Setting.ShowEditorAlias = showAlias;
            IsDirty = true;
        }

        public static void ModifyUniqueBundleName(bool uniqueBundleName)
        {
            Setting.UniqueBundleName = uniqueBundleName;
            IsDirty = true;
        }

        // 资源包裹编辑相关
        public static AssetBundleCollectorPackage CreatePackage(string packageName)
        {
            var package = new AssetBundleCollectorPackage();
            package.PackageName = packageName;
            Setting.Packages.Add(package);
            IsDirty = true;
            return package;
        }

        public static void RemovePackage(AssetBundleCollectorPackage package)
        {
            if (Setting.Packages.Remove(package))
                IsDirty = true;
            else
                Debug.LogWarning($"Failed remove package : {package.PackageName}");
        }

        public static void ModifyPackage(AssetBundleCollectorPackage package)
        {
            if (package != null) IsDirty = true;
        }

        // 资源分组编辑相关
        public static AssetBundleCollectorGroup CreateGroup(AssetBundleCollectorPackage package, string groupName)
        {
            var group = new AssetBundleCollectorGroup();
            group.GroupName = groupName;
            package.Groups.Add(group);
            IsDirty = true;
            return group;
        }

        public static void RemoveGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
        {
            if (package.Groups.Remove(group))
                IsDirty = true;
            else
                Debug.LogWarning($"Failed remove group : {group.GroupName}");
        }

        public static void ModifyGroup(AssetBundleCollectorPackage package, AssetBundleCollectorGroup group)
        {
            if (package != null && group != null) IsDirty = true;
        }

        // 资源收集器编辑相关
        public static void CreateCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            group.Collectors.Add(collector);
            IsDirty = true;
        }

        public static void RemoveCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            if (group.Collectors.Remove(collector))
                IsDirty = true;
            else
                Debug.LogWarning($"Failed remove collector : {collector.CollectPath}");
        }

        public static void ModifyCollector(AssetBundleCollectorGroup group, AssetBundleCollector collector)
        {
            if (group != null && collector != null) IsDirty = true;
        }

        /// <summary>
        ///     获取所有的资源标签
        /// </summary>
        public static string GetPackageAllTags(string packageName)
        {
            var allTags = Setting.GetPackageAllTags(packageName);
            return string.Join(";", allTags);
        }
    }
}