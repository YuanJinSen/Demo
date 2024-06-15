using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    public enum EResourceType
    {
        /// <summary>在打包设置中分析到的资源</summary>
        Direct,
        /// <summary>依赖资源</summary>
        Dependency,
        /// <summary>生成的文件</summary>
        Generate,
    }
}