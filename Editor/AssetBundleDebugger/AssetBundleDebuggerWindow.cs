#if UNITY_2019_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public class AssetBundleDebuggerWindow : EditorWindow
    {
        private readonly Dictionary<int, RemotePlayerSession> _playerSessions = new();
        private DebuggerAssetListViewer _assetListViewer;
        private DebuggerBundleListViewer _bundleListViewer;
        private RemotePlayerSession _currentPlayerSession;
        private DebugReport _currentReport;
        private SliderInt _frameSlider;

        private Label _playerName;
        private int _rangeIndex;
        private string _searchKeyWord;

        private EViewMode _viewMode;
        private ToolbarMenu _viewModeMenu;

        public void OnDestroy()
        {
            // 远程调试
            EditorConnection.instance.UnregisterConnection(OnHandleConnectionEvent);
            EditorConnection.instance.UnregisterDisconnection(OnHandleDisconnectionEvent);
            EditorConnection.instance.Unregister(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
            _playerSessions.Clear();
        }


        public void CreateGUI()
        {
            try
            {
                var root = rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<AssetBundleDebuggerWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 采样按钮
                var sampleBtn = root.Q<Button>("SampleButton");
                sampleBtn.clicked += SampleBtn_onClick;

                // 导出按钮
                var exportBtn = root.Q<Button>("ExportButton");
                exportBtn.clicked += ExportBtn_clicked;

                // 用户列表菜单
                _playerName = root.Q<Label>("PlayerName");
                _playerName.text = "Editor player";

                // 视口模式菜单
                _viewModeMenu = root.Q<ToolbarMenu>("ViewModeMenu");
                _viewModeMenu.menu.AppendAction(EViewMode.AssetView.ToString(), OnViewModeMenuChange,
                    OnViewModeMenuStatusUpdate, EViewMode.AssetView);
                _viewModeMenu.menu.AppendAction(EViewMode.BundleView.ToString(), OnViewModeMenuChange,
                    OnViewModeMenuStatusUpdate, EViewMode.BundleView);
                _viewModeMenu.text = EViewMode.AssetView.ToString();

                // 搜索栏
                var searchField = root.Q<ToolbarSearchField>("SearchField");
                searchField.RegisterValueChangedCallback(OnSearchKeyWordChange);

                // 帧数相关
                {
                    _frameSlider = root.Q<SliderInt>("FrameSlider");
                    _frameSlider.label = "Frame:";
                    _frameSlider.highValue = 0;
                    _frameSlider.lowValue = 0;
                    _frameSlider.value = 0;
                    _frameSlider.RegisterValueChangedCallback(evt => { OnFrameSliderChange(evt.newValue); });

                    var frameLast = root.Q<ToolbarButton>("FrameLast");
                    frameLast.clicked += OnFrameLast_clicked;

                    var frameNext = root.Q<ToolbarButton>("FrameNext");
                    frameNext.clicked += OnFrameNext_clicked;

                    var frameClear = root.Q<ToolbarButton>("FrameClear");
                    frameClear.clicked += OnFrameClear_clicked;
                }

                // 加载视图
                _assetListViewer = new DebuggerAssetListViewer();
                _assetListViewer.InitViewer();

                // 加载视图
                _bundleListViewer = new DebuggerBundleListViewer();
                _bundleListViewer.InitViewer();

                // 显示视图
                _viewMode = EViewMode.AssetView;
                _assetListViewer.AttachParent(root);

                // 远程调试
                EditorConnection.instance.Initialize();
                EditorConnection.instance.RegisterConnection(OnHandleConnectionEvent);
                EditorConnection.instance.RegisterDisconnection(OnHandleDisconnectionEvent);
                EditorConnection.instance.Register(RemoteDebuggerDefine.kMsgSendPlayerToEditor, OnHandlePlayerMessage);
                RemoteDebuggerInRuntime.EditorHandleDebugReportCallback = OnHandleDebugReport;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        [MenuItem("YooAsset/AssetBundle Debugger", false, 104)]
        public static void OpenWindow()
        {
            var wnd = GetWindow<AssetBundleDebuggerWindow>("AssetBundle Debugger", true,
                WindowsDefine.DockedWindowTypes);
            wnd.minSize = new Vector2(800, 600);
        }

        private void OnHandleConnectionEvent(int playerId)
        {
            Debug.Log($"Game player connection : {playerId}");
            _playerName.text = $"Connected player : {playerId}";
        }

        private void OnHandleDisconnectionEvent(int playerId)
        {
            Debug.Log($"Game player disconnection : {playerId}");
            _playerName.text = $"Disconneced player : {playerId}";
        }

        private void OnHandlePlayerMessage(MessageEventArgs args)
        {
            var debugReport = DebugReport.Deserialize(args.data);
            OnHandleDebugReport(args.playerId, debugReport);
        }

        private void OnHandleDebugReport(int playerId, DebugReport debugReport)
        {
            Debug.Log($"Handle player {playerId} debug report !");
            _currentPlayerSession = GetOrCreatePlayerSession(playerId);
            _currentPlayerSession.AddDebugReport(debugReport);
            _frameSlider.highValue = _currentPlayerSession.MaxRangeValue;
            _frameSlider.value = _currentPlayerSession.MaxRangeValue;
            UpdateFrameView(_currentPlayerSession);
        }

        private void OnFrameSliderChange(int sliderValue)
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(sliderValue);
                ;
                UpdateFrameView(_currentPlayerSession, _rangeIndex);
            }
        }

        private void OnFrameLast_clicked()
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(_rangeIndex - 1);
                _frameSlider.value = _rangeIndex;
                UpdateFrameView(_currentPlayerSession, _rangeIndex);
            }
        }

        private void OnFrameNext_clicked()
        {
            if (_currentPlayerSession != null)
            {
                _rangeIndex = _currentPlayerSession.ClampRangeIndex(_rangeIndex + 1);
                _frameSlider.value = _rangeIndex;
                UpdateFrameView(_currentPlayerSession, _rangeIndex);
            }
        }

        private void OnFrameClear_clicked()
        {
            if (_currentPlayerSession != null)
            {
                _frameSlider.label = "Frame:";
                _frameSlider.value = 0;
                _frameSlider.lowValue = 0;
                _frameSlider.highValue = 0;
                _currentPlayerSession.ClearDebugReport();
                _assetListViewer.ClearView();
                _bundleListViewer.ClearView();
            }
        }

        private RemotePlayerSession GetOrCreatePlayerSession(int playerId)
        {
            if (_playerSessions.TryGetValue(playerId, out var session))
            {
                return session;
            }

            var newSession = new RemotePlayerSession(playerId);
            _playerSessions.Add(playerId, newSession);
            return newSession;
        }

        private void UpdateFrameView(RemotePlayerSession playerSession)
        {
            if (playerSession != null) UpdateFrameView(playerSession, playerSession.MaxRangeValue);
        }

        private void UpdateFrameView(RemotePlayerSession playerSession, int rangeIndex)
        {
            if (playerSession == null)
                return;

            var debugReport = playerSession.GetDebugReport(rangeIndex);
            if (debugReport != null)
            {
                _currentReport = debugReport;
                _frameSlider.label = $"Frame: {debugReport.FrameCount}";
                _assetListViewer.FillViewData(debugReport, _searchKeyWord);
                _bundleListViewer.FillViewData(debugReport, _searchKeyWord);
            }
        }

        private void SampleBtn_onClick()
        {
            // 发送采集数据的命令
            var command = new RemoteCommand();
            command.CommandType = (int)ERemoteCommand.SampleOnce;
            command.CommandParam = string.Empty;
            var data = RemoteCommand.Serialize(command);
            EditorConnection.instance.Send(RemoteDebuggerDefine.kMsgSendEditorToPlayer, data);
            RemoteDebuggerInRuntime.EditorRequestDebugReport();
        }

        private void ExportBtn_clicked()
        {
            if (_currentReport == null)
            {
                Debug.LogWarning("Debug report is null.");
                return;
            }

            var resultPath = EditorTools.OpenFolderPanel("Export JSON", "Assets/");
            if (resultPath != null)
            {
                // 注意：排序保证生成配置的稳定性
                foreach (var packageData in _currentReport.PackageDatas)
                {
                    packageData.ProviderInfos.Sort();
                    foreach (var providerInfo in packageData.ProviderInfos) providerInfo.DependBundleInfos.Sort();
                }

                var filePath = $"{resultPath}/{nameof(DebugReport)}_{_currentReport.FrameCount}.json";
                var fileContent = JsonUtility.ToJson(_currentReport, true);
                FileUtility.WriteAllText(filePath, fileContent);
            }
        }

        private void OnSearchKeyWordChange(ChangeEvent<string> e)
        {
            _searchKeyWord = e.newValue;
            if (_currentReport != null)
            {
                _assetListViewer.FillViewData(_currentReport, _searchKeyWord);
                _bundleListViewer.FillViewData(_currentReport, _searchKeyWord);
            }
        }

        private void OnViewModeMenuChange(DropdownMenuAction action)
        {
            var viewMode = (EViewMode)action.userData;
            if (_viewMode != viewMode)
            {
                _viewMode = viewMode;
                var root = rootVisualElement;
                _viewModeMenu.text = viewMode.ToString();

                if (viewMode == EViewMode.AssetView)
                {
                    _assetListViewer.AttachParent(root);
                    _bundleListViewer.DetachParent();
                }
                else if (viewMode == EViewMode.BundleView)
                {
                    _assetListViewer.DetachParent();
                    _bundleListViewer.AttachParent(root);
                }
                else
                {
                    throw new NotImplementedException(viewMode.ToString());
                }
            }
        }

        private DropdownMenuAction.Status OnViewModeMenuStatusUpdate(DropdownMenuAction action)
        {
            var viewMode = (EViewMode)action.userData;
            if (_viewMode == viewMode)
                return DropdownMenuAction.Status.Checked;
            return DropdownMenuAction.Status.Normal;
        }

        /// <summary>
        ///     视图模式
        /// </summary>
        private enum EViewMode
        {
            /// <summary>
            ///     内存视图
            /// </summary>
            MemoryView,

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