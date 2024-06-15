using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    internal class ResourceAsync : AResourceAsync
    {
        public override bool keepWaiting => !done;
        private AssetBundleRequest m_AssetBundleRequest;

        public override bool Update()
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

            if (!bundle.done)
                return false;

            if (m_AssetBundleRequest == null)
                LoadAssetAsync();
            if (m_AssetBundleRequest != null && !m_AssetBundleRequest.isDone)
                return false;

            LoadAsset();

            return true;
        }

        internal override void Load()
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException($"{nameof(Resource)}.{nameof(Load)}() url:{url} is null");

            if (bundle != null)
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() bundle not null");

            string bundleUrl = null;
            if (!ResourceManager.instance.ResourceBunldeDic.TryGetValue(url, out bundleUrl))
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() bundleUrl is null");

            bundle = BundleManager.instance.LoadAsync(bundleUrl);
        }

        internal override void Unload()
        {
            if (bundle == null)
                throw new Exception($"{nameof(Resource)}.{nameof(Unload)}() {nameof(bundle)} is null.");

            if (asset != null && !(asset is GameObject))
            {
                Resources.UnloadAsset(asset);
            }
            asset = null;
            m_AssetBundleRequest = null;
            BundleManager.instance.Unload(bundle);
            bundle = null;
            finishedCallback = null;
        }

        internal override void LoadAsset()
        {
            if(bundle == null)
                throw new ArgumentException($"{nameof(Resource)}.{nameof(LoadAsset)}() bundle is null,url:{url}");

            if (!bundle.isStreamedSceneAssetBundle)
            {
                if (m_AssetBundleRequest != null)
                {
                    asset = m_AssetBundleRequest.asset;
                }
                else
                {
                    asset = bundle.LoadAsset(url, typeof(Object));
                }
            }
            done = true;

            if (finishedCallback != null)
            {
                var temp = finishedCallback;
                finishedCallback = null;
                temp.Invoke(this);
            }
        }

        internal override void LoadAssetAsync()
        {
            if (bundle == null)
                throw new ArgumentException($"{nameof(Resource)}.{nameof(LoadAsset)}() bundle is null,url:{url}");

            m_AssetBundleRequest = bundle.LoadAssetAsync(url, typeof(Object));
        }

        public override T GetAsset<T>()
        {
            Object tempAsset = asset;
            Type type = typeof(T);
            if (type == typeof(Sprite))
            {
                if (asset is Sprite)
                {
                    return tempAsset as T;
                }
                else
                {
                    if (tempAsset && !(tempAsset is GameObject))
                    {
                        Resources.UnloadAsset(tempAsset);
                    }

                    asset = bundle.LoadAsset(url, type);
                    return asset as T;
                }
            }
            else
            {
                return tempAsset as T;
            }
        }
    }
}