using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    public class BundleAsync : ABundleAsync
    {
        private AssetBundleCreateRequest m_AssetBundleCreateRequest;

        internal override void Load()
        {
            if(m_AssetBundleCreateRequest != null)
                throw new Exception($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(m_AssetBundleCreateRequest)} not null, {this}.");

            string file = BundleManager.instance.GetFileUrl(url);

#if UNITY_EDITOR || UNITY_STANDALONE
            if (!File.Exists(file))
                throw new Exception($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(file)} not exist, file:{file}.");
#endif
            m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(file);
        }

        internal override void UnLoad()
        {
            if (assetBundle)
            {
                assetBundle.Unload(true);
            }
            else
            {
                if (m_AssetBundleCreateRequest != null)
                    assetBundle = m_AssetBundleCreateRequest.assetBundle;
                if(assetBundle)
                    assetBundle.Unload(true);
            }

            m_AssetBundleCreateRequest = null;
            reference = 0;
            done = false;
            assetBundle = null;
            isStreamedSceneAssetBundle = false;
        }

        internal override Object LoadAsset(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(Bundle)}.{nameof(LoadAsset)}() name is null.");

            if (m_AssetBundleCreateRequest == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() AssetBundleCreateRequest is null.");

            if (assetBundle == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() Bundle is null.");

            return assetBundle.LoadAsset(name, type);
        }

        internal override AssetBundleRequest LoadAssetAsync(string name, Type type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(Bundle)}.{nameof(LoadAsset)}() name is null.");

            if (m_AssetBundleCreateRequest == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() AssetBundleCreateRequest is null.");

            if (assetBundle == null)
                throw new NullReferenceException($"{nameof(Bundle)}.{nameof(LoadAsset)}() Bundle is null.");

            return assetBundle.LoadAssetAsync(name, type);
        }

        internal override bool Update()
        {
            if (done)
                return true;

            if (dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (!dependencies[i].done)
                        return false;
                }
            }

            if (!m_AssetBundleCreateRequest.isDone)
                return false;

            done = true;

            assetBundle = m_AssetBundleCreateRequest.assetBundle;
            isStreamedSceneAssetBundle = assetBundle.isStreamedSceneAssetBundle;

            if (reference == 0)
                UnLoad();

            return true;
        }
    }
}