#if UNITY_2019_4_OR_NEWER
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class AssetBundleReporterWindow : EditorWindow
    {
        private ReporterAssetListViewer _assetListViewer;
        private BuildReport _buildReport;
        private ReporterBundleListViewer _bundleListViewer;
        private string _reportFilePath;
        private string _searchKeyWord;
        private ReporterSummaryViewer _summaryViewer;

        private EViewMode _viewMode;

        private ToolbarMenu _viewModeMenu;

        public void OnDestroy()
        {
            AssetBundleRecorder.UnloadAll();
        }


        public void CreateGUI()
        {
            try
            {
                var root = rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<AssetBundleReporterWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 导入按钮
                var importBtn = root.Q<Button>("ImportButton");
                importBtn.clicked += ImportBtn_onClick;

                // 视图模式菜单
                _viewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
                _viewModeMenu.menu.AppendAction(EViewMode.Summary.ToString(), ViewModeMenuAction0, ViewModeMenuFun0);
                _viewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), ViewModeMenuAction1, ViewModeMenuFun1);
                _viewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), ViewModeMenuAction2, ViewModeMenuFun2);

                // 搜索栏
                var searchField = root.Q<ToolbarSearchField>("SearchField");
                searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

                // 加载视图
                _summaryViewer = new ReporterSummaryViewer();
                _summaryViewer.InitViewer();

                // 加载视图
                _assetListViewer = new ReporterAssetListViewer();
                _assetListViewer.InitViewer();

                // 加载视图
                _bundleListViewer = new ReporterBundleListViewer();
                _bundleListViewer.InitViewer();

                // 显示视图
                _viewMode = EViewMode.Summary;
                _viewModeMenu.text = EViewMode.Summary.ToString();
                _summaryViewer.AttachParent(root);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        [MenuItem("YooAsset/AssetBundle Reporter", false, 103)]
        public static void OpenWindow()
        {
            var window =
                GetWindow<AssetBundleReporterWindow>("AssetBundle Reporter", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(800, 600);
        }

        private void ImportBtn_onClick()
        {
            var selectFilePath = EditorUtility.OpenFilePanel("导入报告", EditorTools.GetProjectPath(), "json");
            if (string.IsNullOrEmpty(selectFilePath))
                return;

            _reportFilePath = selectFilePath;
            var jsonData = FileUtility.ReadAllText(_reportFilePath);
            _buildReport = BuildReport.Deserialize(jsonData);
            _summaryViewer.FillViewData(_buildReport);
            _assetListViewer.FillViewData(_buildReport, _searchKeyWord);
            _bundleListViewer.FillViewData(_buildReport, _reportFilePath, _searchKeyWord);
        }

        private void OnSearchKeyWordChange(ChangeEvent<string> e)
        {
            _searchKeyWord = e.newValue;
            if (_buildReport != null)
            {
                _assetListViewer.FillViewData(_buildReport, _searchKeyWord);
                _bundleListViewer.FillViewData(_buildReport, _reportFilePath, _searchKeyWord);
            }
        }

        private void ViewModeMenuAction0(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.Summary)
            {
                _viewMode = EViewMode.Summary;
                var root = rootVisualElement;
                _viewModeMenu.text = EViewMode.Summary.ToString();
                _summaryViewer.AttachParent(root);
                _assetListViewer.DetachParent();
                _bundleListViewer.DetachParent();
            }
        }

        private void ViewModeMenuAction1(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.AssetView)
            {
                _viewMode = EViewMode.AssetView;
                var root = rootVisualElement;
                _viewModeMenu.text = EViewMode.AssetView.ToString();
                _summaryViewer.DetachParent();
                _assetListViewer.AttachParent(root);
                _bundleListViewer.DetachParent();
            }
        }

        private void ViewModeMenuAction2(DropdownMenuAction action)
        {
            if (_viewMode != EViewMode.BundleView)
            {
                _viewMode = EViewMode.BundleView;
                var root = rootVisualElement;
                _viewModeMenu.text = EViewMode.BundleView.ToString();
                _summaryViewer.DetachParent();
                _assetListViewer.DetachParent();
                _bundleListViewer.AttachParent(root);
            }
        }

        private DropdownMenuAction.Status ViewModeMenuFun0(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.Summary)
                return DropdownMenuAction.Status.Checked;
            return DropdownMenuAction.Status.Normal;
        }

        private DropdownMenuAction.Status ViewModeMenuFun1(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.AssetView)
                return DropdownMenuAction.Status.Checked;
            return DropdownMenuAction.Status.Normal;
        }

        private DropdownMenuAction.Status ViewModeMenuFun2(DropdownMenuAction action)
        {
            if (_viewMode == EViewMode.BundleView)
                return DropdownMenuAction.Status.Checked;
            return DropdownMenuAction.Status.Normal;
        }

        /// <summary>
        ///     视图模式
        /// </summary>
        private enum EViewMode
        {
            /// <summary>
            ///     概览视图
            /// </summary>
            Summary,

            /// <summary>
            ///     资源对象视图
            /// </summary>
            AssetView,

            /// <summary>
            ///     资源包视图
            /// </summary>
            BundleView
        }
    }
}
#endif