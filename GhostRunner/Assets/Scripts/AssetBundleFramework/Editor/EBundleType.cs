using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    public enum EBundleType
    {
        /// <summary>以文件作为AB粒度</summary>
        File,
        /// <summary>以目录作为AB粒度</summary>
        Directory,
        /// <summary>以上所有作为AB粒度</summary>
        All,
    }
}