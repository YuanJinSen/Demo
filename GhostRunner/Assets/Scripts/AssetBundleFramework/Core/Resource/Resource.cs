using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    internal class Resource : AResource
    {
        public override bool keepWaiting => !done;

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

        internal override void Load()
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException($"{nameof(Resource)}.{nameof(Load)}() url:{url} is null");

            if (bundle != null)
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() bundle not null. url:{url}");

            string bundleUrl = null;
            if (!ResourceManager.instance.ResourceBunldeDic.TryGetValue(url, out bundleUrl))
                throw new Exception($"{nameof(Resource)}.{nameof(Load)}() bundleUrl is null. url:{url}");

            bundle = BundleManager.instance.Load(bundleUrl);

            LoadAsset();
        }

        internal override void LoadAsset()
        {
            if(bundle == null)
                throw new Exception($"{nameof(Resource)}.{nameof(LoadAsset)}() {nameof(bundle)} is null.");

            FreshAsyncAsset();

            if (!bundle.isStreamedSceneAssetBundle)
                asset = bundle.LoadAsset(url, typeof(Object));

            asset = bundle.LoadAsset(url, typeof(Object));

            done = true;

            if (finishedCallback != null)
            {
                var temp = finishedCallback;
                finishedCallback = null;
                temp.Invoke(this);
            }
        }

        internal override void Unload()
        {
            if(bundle == null)
                throw new Exception($"{nameof(Resource)}.{nameof(Unload)}() {nameof(bundle)} is null.");

            if (asset != null && !(asset is GameObject))
            {
                Resources.UnloadAsset(asset);
            }

            BundleManager.instance.Unload(bundle);
            bundle = null;
            finishedCallback = null;
        }
    }
}