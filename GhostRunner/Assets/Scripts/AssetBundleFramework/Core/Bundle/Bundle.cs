using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetBundleFramework
{
    public class Bundle : ABundle
    {
        internal override void Load()
        {
            if (assetBundle)
                throw new Exception($"{nameof(Bundle)}.{nameof(Load)}() assetBundle not null, Url:{url}");

            string file = BundleManager.instance.GetFileUrl(url);

#if UNITY_EDITOR || UNITY_STANDALONE
            if(!File.Exists(file))
                throw new Exception($"{nameof(Bundle)}.{nameof(Load)}() file not exist, file:{file}");
#endif
            assetBundle = AssetBundle.LoadFromFile(file, 0, BundleManager.instance.offset);
            isStreamedSceneAssetBundle = assetBundle.isStreamedSceneAssetBundle;
            done = true;
        }

        internal override void UnLoad()
        {
            if (assetBundle)
                assetBundle.Unload(true);
            reference = 0;
            done = false;
            assetBundle = null;
            isStreamedSceneAssetBundle = false;
        }

        internal override UnityEngine.Object LoadAsset(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(Bundle)}.{nameof(LoadAsset)}() name is null.");

            if (assetBundle == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() Bundle is null.");

            return assetBundle.LoadAsset(name, type);
        }

        internal override AssetBundleRequest LoadAssetAsync(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(Bundle)}.{nameof(LoadAsset)}() name is null.");

            if (assetBundle == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() Bundle is null.");

            return assetBundle.LoadAssetAsync(name, type);
        }
    }
}