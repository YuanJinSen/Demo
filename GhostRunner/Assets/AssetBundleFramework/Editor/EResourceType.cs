using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    public enum EResourceType
    {
        /// <summary>�ڴ�������з���������Դ</summary>
        Direct,
        /// <summary>������Դ</summary>
        Dependency,
        /// <summary>���ɵ��ļ�</summary>
        Generate,
    }
}