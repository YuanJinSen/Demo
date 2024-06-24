using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    public abstract class ABundleAsync : ABundle
    {
        internal abstract bool Update();
    }
}