using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetBundleFramework
{
    public class ResourceManager
    {
        public readonly static ResourceManager instance = new ResourceManager();
        private const string MANIFEST_BUNDLE = "manifest.ab";
        private const string RESOURCE_ASSET_NAME = "Assets/Temp/Resource.bytes";
        private const string BUNDLE_ASSET_NAME = "Assets/Temp/Bundle.bytes";
        private const string DEPENDENCY_ASSET_NAME = "Assets/Temp/Dependency.bytes";

        private bool m_Editor;
        internal Dictionary<string, string> ResourceBunldeDic = new Dictionary<string, string>();
        internal Dictionary<string, List<string>> ResourceDependencyDic = new Dictionary<string, List<string>>();

        public void Update()
        {
            BundleManager.instance.Update();
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
            while (m_NeedUnloadList.Count > 0)
            {
                AResource resource = m_NeedUnloadList.First.Value;
                m_NeedUnloadList.RemoveFirst();
                if (resource == null)
                    continue;

                m_ResourceDic.Remove(resource.url);

                resource.Unload();

                if (resource.dependencies != null)
                {
                    AResource[] dependencies = resource.dependencies;
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        AResource temp = dependencies[i];
                        Unload(temp);
                    }
                }
            }

            BundleManager.instance.LateUpdate();
        }

        private Dictionary<string, AResource> m_ResourceDic = new Dictionary<string, AResource>();
        private LinkedList<AResource> m_NeedUnloadList = new LinkedList<AResource>();
        private List<AResourceAsync> m_AsyncList = new List<AResourceAsync>();

        public void Init(string platform, Func<string, string> getFileCallback, bool editor, ushort offset)
        {
            m_Editor = editor;
            if (m_Editor) return;

            BundleManager.instance.Init(platform, getFileCallback, offset);
            string ManifestAssetBundleFile = getFileCallback.Invoke(MANIFEST_BUNDLE);
            AssetBundle ManifestAssetBundle = AssetBundle.LoadFromFile(ManifestAssetBundleFile, 0, offset);

            TextAsset resourceTextAsset = ManifestAssetBundle.LoadAsset(RESOURCE_ASSET_NAME) as TextAsset;
            TextAsset bundleTextAsset = ManifestAssetBundle.LoadAsset(BUNDLE_ASSET_NAME) as TextAsset;
            TextAsset dependencyTextAsset = ManifestAssetBundle.LoadAsset(DEPENDENCY_ASSET_NAME) as TextAsset;

            byte[] resourceBytes = resourceTextAsset.bytes;
            byte[] bundleBytes = bundleTextAsset.bytes;
            byte[] dependencyBytes = dependencyTextAsset.bytes;

            ManifestAssetBundle.Unload(true);
            ManifestAssetBundle = null;

            Dictionary<ushort, string> assetUrlDic = new Dictionary<ushort, string>();
            #region 读取资源信息
            {
                MemoryStream ms = new MemoryStream(resourceBytes);
                BinaryReader br = new BinaryReader(ms);
                ushort count = br.ReadUInt16();
                for (ushort i = 0; i < count; i++)
                {
                    string assetUrl = br.ReadString();
                    assetUrlDic.Add(i, assetUrl);
                }
            }
            #endregion

            #region 读取bundle信息
            {
                ResourceBunldeDic.Clear();
                MemoryStream ms = new MemoryStream(bundleBytes);
                BinaryReader br = new BinaryReader(ms);
                ushort bundleCount = br.ReadUInt16();
                for (int i = 0; i < bundleCount; i++)
                {
                    string bundleUrl = br.ReadString();
                    string bundleFileUrl = bundleUrl;
                    ushort resourceCount = br.ReadUInt16();
                    for (int ii = 0; ii < resourceCount; ii++)
                    {
                        ushort assetId = br.ReadUInt16();
                        string assetUrl = assetUrlDic[assetId];
                        ResourceBunldeDic.Add(assetUrl, bundleFileUrl);
                    }
                }
            }
            #endregion

            #region 读取依赖信息
            {
                ResourceDependencyDic.Clear();
                MemoryStream ms = new MemoryStream(dependencyBytes);
                BinaryReader br = new BinaryReader(ms);
                ushort count = br.ReadUInt16();
                for (ushort i = 0; i < count; i++)
                {
                    ushort countInBundle = br.ReadUInt16();

                    ushort assetIdUrl = br.ReadUInt16();
                    string assetUrl = assetUrlDic[assetIdUrl];

                    List<string> dependencies = new List<string>(countInBundle);
                    for (int ii = 1; ii < countInBundle; ii++)
                    {
                        ushort dependencyId = br.ReadUInt16();
                        dependencies.Add(assetUrlDic[dependencyId]);
                    }
                    ResourceDependencyDic.Add(assetUrl, dependencies);
                }
            }
            #endregion
        }

        internal IResource Load(string url, bool isAsync)
        {
            return LoadInternal(url, isAsync, false);
        }

        public void LoadWithCallback(string url, bool isAsync, Action<IResource> callback)
        {
            AResource resource = LoadInternal(url, isAsync, false);
            if (resource.done)
                callback.Invoke(resource);
            else
                resource.finishedCallback += callback;
        }

        private AResource LoadInternal(string url, bool isAsync, bool isDependency)
        {
            AResource resource = null;
            if (m_ResourceDic.TryGetValue(url, out resource))
            {
                if (resource.reference == 0)
                {
                    m_NeedUnloadList.Remove(resource);
                }
                resource.AddReference();

                return resource;
            }
            if (m_Editor) resource = new EditorResource();
            else if (isAsync)
            {
                ResourceAsync resourceAsync = new ResourceAsync();
                m_AsyncList.Add(resourceAsync);
                resource = resourceAsync;
            }
            else resource = new Resource();

            resource.url = url;
            m_ResourceDic.Add(url, resource);

            List<string> dependencies = null;
            ResourceDependencyDic.TryGetValue(url, out dependencies);
            if (dependencies != null && dependencies.Count > 0)
            {
                resource.dependencies = new AResource[dependencies.Count];
                for (int i = 0; i < dependencies.Count; i++)
                {
                    string dependencyUrl = dependencies[i];
                    AResource dependency = LoadInternal(dependencyUrl, isAsync, true);
                    resource.dependencies[i] = dependency;
                }
            }

            resource.AddReference();
            resource.Load();

            return resource;
        }

        public void Unload(IResource resource)
        {
            if (resource == null)
                throw new Exception($"{nameof(ResourceManager)}.{nameof(Unload)}() resource is null");

            AResource aResource = resource as AResource;
            aResource.ReduceReference();

            if (aResource.reference == 0)
                WillUnload(aResource);
        }

        public void Unload(string assetUrl)
        {
            if (string.IsNullOrEmpty(assetUrl))
                throw new ArgumentException($"{nameof(ResourceManager)}.{nameof(Unload)}() {nameof(assetUrl)} is null.");

            AResource resource;
            if (!m_ResourceDic.TryGetValue(assetUrl, out resource))
                throw new Exception($"{nameof(ResourceManager)}.{nameof(Unload)}(),Unload [{assetUrl}] failed.");

            Unload(resource);
        }

        private void WillUnload(AResource resource)
        {
            m_NeedUnloadList.AddLast(resource);
        }
    }
}