using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleFramework
{
    internal class BundleManager
    {
        public readonly static BundleManager instance = new BundleManager();

        internal ushort offset;
        private Func<string, string> m_GetFileCallback;
        private AssetBundleManifest m_AssetBundleManifest;
        private Dictionary<string, ABundle> m_BundleDic = new Dictionary<string, ABundle>();
        private LinkedList<ABundle> m_NeedUnloadList = new LinkedList<ABundle>();
        private List<ABundleAsync> m_AsyncList = new List<ABundleAsync>();

        public void Init(string platform, Func<string, string> getFileCallback, ushort offset)
        {
            m_GetFileCallback = getFileCallback;
            this.offset = offset;

            string ManifestAssetBundleFile = getFileCallback.Invoke(platform);
            AssetBundle ManifestAssetBundle = AssetBundle.LoadFromFile(ManifestAssetBundleFile);
            UnityEngine.Object[] objs = ManifestAssetBundle.LoadAllAssets();
            if (objs.Length == 0)
                throw new Exception("ManifestAssetBundle Load Fail");
            m_AssetBundleManifest = objs[0] as AssetBundleManifest;
        }

        public void Update()
        {
            for (int i = 0; i < m_AsyncList.Count; i++)
            {
                if (m_AsyncList[i].Update())
                {
                    m_AsyncList.RemoveAt(i);
                    i--;
                }
            }
        }

        public void LateUpdate()
        {
            if (m_NeedUnloadList.Count == 0)
                return;

            while (m_NeedUnloadList.Count > 0)
            {
                ABundle bundle = m_NeedUnloadList.First.Value;
                m_NeedUnloadList.RemoveFirst();
                if (bundle == null)
                    continue;

                m_BundleDic.Remove(bundle.url);

                if (!bundle.done && bundle is BundleAsync)
                {
                    BundleAsync bundleAsync = bundle as BundleAsync;
                    if (m_AsyncList.Contains(bundleAsync))
                        m_AsyncList.Remove(bundleAsync);
                }

                bundle.UnLoad();

                if (bundle.dependencies != null)
                {
                    ABundle[] dependencies = bundle.dependencies;
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        ABundle temp = dependencies[i];
                        Unload(temp);
                    }
                }
            }
        }

        internal string GetFileUrl(string url)
        {
            if (m_GetFileCallback == null)
                throw new Exception($"{nameof(BundleManager)}.{nameof(GetFileUrl)}() {nameof(m_GetFileCallback)} is null.");

            //交到外部处理
            string name = m_GetFileCallback.Invoke(url);
            return name;
        }

        public ABundle Load(string url)
        {
            return LoadInternal(url, false);
        }

        public ABundle LoadAsync(string url)
        {
            return LoadInternal(url, true);
        }

        public ABundle LoadInternal(string url, bool isAsync)
        {
            ABundle bundle;
            if (m_BundleDic.TryGetValue(url, out bundle))
            {
                if (bundle.reference == 0)
                {
                    m_NeedUnloadList.Remove(bundle);
                }
                bundle.AddReference();
                return bundle;
            }
            
            if (isAsync)
            {
                BundleAsync bundleAsync = new BundleAsync();
                bundleAsync.url = url;
                m_AsyncList.Add(bundleAsync);
                bundle = bundleAsync;
            }
            else
            {
                bundle = new Bundle();
                bundle.url = url;
            }

            m_BundleDic.Add(url, bundle);

            //加载依赖
            string[] dependencies = m_AssetBundleManifest.GetDirectDependencies(url);
            if (dependencies.Length > 0)
            {
                bundle.dependencies = new ABundle[dependencies.Length];
                for (int i = 0; i < dependencies.Length; i++)
                {
                    string dependency = dependencies[i];
                    bundle.dependencies[i] = LoadInternal(dependency, isAsync);
                }
            }

            bundle.AddReference();
            bundle.Load();

            return bundle;
        }

        public void Unload(ABundle bundle)
        {
            if(bundle == null)
                throw new Exception($"{nameof(BundleManager)}.{nameof(Unload)}() bundle is null.");

            bundle.ReduceReference();

            if (bundle.reference == 0)
                WhileUnload(bundle);
        }

        public void WhileUnload(ABundle bundle)
        {
            m_NeedUnloadList.AddLast(bundle);
        }
    }
}