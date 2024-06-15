using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    internal class EditorResource : AResource
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
#if UNITY_EDITOR
                    if (tempAsset && !(tempAsset is GameObject))
                    {
                        Resources.UnloadAsset(tempAsset);
                    }

                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(url);
#endif
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
            LoadAsset();
        }

        internal override void LoadAsset()
        {
#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(url);
#endif
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
            if (asset != null && !(asset is GameObject))
            {
                Resources.UnloadAsset(asset);
            }

            asset = null;
            finishedCallback = null;
            //awaiter = null;
        }
    }
}