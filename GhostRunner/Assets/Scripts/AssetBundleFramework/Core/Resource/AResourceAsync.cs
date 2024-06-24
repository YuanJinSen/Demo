using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    internal abstract class AResourceAsync : AResource
    {
        public abstract bool Update();

        internal abstract void LoadAssetAsync();
    }
}