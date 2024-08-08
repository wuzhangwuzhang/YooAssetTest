using System;
using System.Collections.Generic;

namespace YooAsset
{
    [Serializable]
    internal class DebugPackageData
    {
        /// <summary>
        ///     包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        ///     调试数据列表
        /// </summary>
        public List<DebugProviderInfo> ProviderInfos = new(1000);
    }
}