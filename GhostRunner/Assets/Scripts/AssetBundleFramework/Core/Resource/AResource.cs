using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleFramework
{
    internal abstract class AResource : CustomYieldInstruction, IResource
    {
        public string url { get; set; }

        public virtual Object asset { get; protected set; }

        /// <summary>“˝”√ ˝</summary>
        internal int reference { get; set; }

        /// <summary>“¿¿µ</summary>
        public AResource[] dependencies { get; set; }

        internal ABundle bundle { get; set; }

        internal bool done { get; set; }

        internal Action<AResource> finishedCallback { get; set; }

        public Object GetAsset()
        {
            return asset;
        }

        public abstract T GetAsset<T>() where T : Object;

        public GameObject Instantiate()
        {
            Object obj = asset;
            if (!obj || !(obj is GameObject)) return null;
            return Object.Instantiate(obj) as GameObject;
        }

        public GameObject Instantiate(Transform parent, bool isWorldSpace)
        {
            Object obj = asset;
            if (!obj || !(obj is GameObject)) return null;
            return Object.Instantiate(obj, parent, isWorldSpace) as GameObject;
        }

        internal void AddReference()
        {
            ++reference;
        }

        internal void ReduceReference()
        {
            --reference;

            if (reference < 0)
            {
                throw new Exception($"{GetType()}.{nameof(ReduceReference)}() less than 0,{nameof(url)}:{url}.");
            }
        }

        internal abstract void Load();

        internal abstract void Unload();

        internal abstract void LoadAsset();

        internal void FreshAsyncAsset()
        {
            if (done) return;

            if (dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    AResource resource = dependencies[i];
                    resource.FreshAsyncAsset();
                }
            }

            if (this is AResourceAsync)
            {
                LoadAsset();
            }
        }

        public GameObject Instantiate(bool autoUnload)
        {
            throw new NotImplementedException();
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation, bool autoUnload)
        {
            throw new NotImplementedException();
        }

        public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace, bool autoUnload)
        {
            throw new NotImplementedException();
        }
    }
}