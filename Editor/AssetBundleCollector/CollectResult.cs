﻿using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class CollectResult
    {
        public CollectResult(CollectCommand command)
        {
            Command = command;
        }

        /// <summary>
        ///     收集命令
        /// </summary>
        public CollectCommand Command { private set; get; }

        /// <summary>
        ///     收集的资源信息列表
        /// </summary>
        public List<CollectAssetInfo> CollectAssets { private set; get; }

        public void SetCollectAssets(List<CollectAssetInfo> collectAssets)
        {
            CollectAssets = collectAssets;
        }
    }
}