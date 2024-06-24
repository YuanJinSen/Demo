using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    public abstract class ABundle
    {
        internal AssetBundle assetBundle { get; set; }

        internal bool isStreamedSceneAssetBundle { get; set; }

        internal string url { get; set; }

        internal int reference { get; set; }

        internal bool done { get; set; }

        internal ABundle[] dependencies { get; set; }

        internal abstract void Load();

        internal abstract void UnLoad();

        internal abstract AssetBundleRequest LoadAssetAsync(string name, Type type);

        internal abstract Object LoadAsset(string name, Type type);

        internal void AddReference()
        {
            ++reference;
        }

        internal void ReduceReference()
        {
            --reference;
            if (reference < 0)
                throw new Exception($"{GetType()}.{nameof(ReduceReference)}() less than 0,{nameof(url)}:{url}.");
        }
    }
}