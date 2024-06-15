using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace AssetBundleFramework
{
    public class BuildSetting : ISupportInitialize
    {
        [XmlAttribute("ProjectName")]
        public string projectName { get; set; }

        [XmlAttribute("SuffixList")]
        public List<string> suffixList { get; set; }

        [XmlAttribute("BuildRoot")]
        public string buildRoot { get; set; }

        [XmlElement("BuildItem")]
        public List<BuildItem> items { get; set; } = new List<BuildItem>();

        [XmlIgnore]
        public Dictionary<string, BuildItem> itemDic = new Dictionary<string, BuildItem>();

        public void BeginInit()
        {
            
        }

        public void EndInit()
        {
            buildRoot = Path.GetFullPath(buildRoot).Replace("\\", "/");
            itemDic.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                BuildItem item = items[i];
                if (item.bundleType == EBundleType.All || item.bundleType == EBundleType.Directory)
                {
                    if (!Directory.Exists(item.assetPath))
                    {
                        throw new Exception($"Path is Null: {item.assetPath}");
                    }
                }

                string[] suffixs = item.suffix.Split('|');
                for (int ii = 0; ii < suffixs.Length; ii++)
                {
                    string suffix = suffixs[ii].Trim();
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        item.suffixes.Add(suffix);
                    }
                }

                itemDic.Add(item.assetPath, item);
            }
        }

        /// <summary>
        /// ��ȡ�����ڴ�����õ��ļ��б�
        /// </summary>
        /// <returns></returns>
        public HashSet<string> Collect()
        {
            float min = Builder.CollectProgressBar.x;
            float max = Builder.CollectProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(Collect), "�Ѽ������Դ����", min);

            //����ÿ��������Ե�Ŀ¼
            //father AssetPath="Assets/AssetBundle/Atlas/"
            //son AssetPath="Assets/AssetBundle/Atlas/TypeA"
            //��ʱ��Ҫ��father.ignorePaths�����son.assetPath
            //��˼����TypeAֻ��son��ȡ
            //������father������TypeAʱ����ֹ�ظ���ȡ0000
            for (int i = 0; i < items.Count; i++)
            {
                BuildItem father = items[i];
                if (father.resourceType != EResourceType.Direct) continue;

                father.ignorePaths.Clear();
                for (int j = 0; j < items.Count; j++)
                {
                    BuildItem son = items[j];
                    if (i == j || son.resourceType != EResourceType.Direct) continue;

                    if (son.assetPath.StartsWith(father.assetPath, StringComparison.InvariantCulture))
                    {
                        father.ignorePaths.Add(son.assetPath);
                    }
                }
            }
            HashSet<string> files = new HashSet<string>();
            for (int i = 0; i < items.Count; i++)
            {
                BuildItem item = items[i];
                if (item.resourceType != EResourceType.Direct) continue;
                List<string> tempFiles = Builder.GetFiles(item.assetPath, null, item.suffixes.ToArray());
                for (int j = 0; j < tempFiles.Count; j++)
                {
                    if (!IsIgnore(items[i].ignorePaths, tempFiles[j]))
                    {
                        files.Add(tempFiles[j]);
                    }
                }
                EditorUtility.DisplayProgressBar(nameof(Collect), "�Ѽ������Դ����", min + (max - min) * (float)i / items.Count);
            }
            return files;
        }

        public bool IsIgnore(List<string> ignoreList, string file)
        {
            for (int i = 0; i < ignoreList.Count; i++)
            {
                if (string.IsNullOrEmpty(ignoreList[i])) continue;
                if (file.StartsWith(ignoreList[i], StringComparison.InvariantCulture))
                    return true;
            }
            return false;
        }

        public BuildItem GetBuildItem(string assetUrl)
        {
            BuildItem item = null;
            for (int i = 0; i < items.Count; ++i)
            {
                BuildItem tempItem = items[i];
                //ǰ���Ƿ�ƥ��
                //Debug.Log(assetUrl + tempItem.assetPath);
                if (assetUrl.StartsWith(tempItem.assetPath, StringComparison.InvariantCulture))
                {
                    //�ҵ����ȼ���ߵ�Rule,·��Խ��˵�����ȼ�Խ��
                    if (item == null || item.assetPath.Length < tempItem.assetPath.Length)
                    {
                        item = tempItem;
                    }
                }
            }
            return item;
        }

        public string GetBundleName(string assetUrl, EResourceType value)
        {
            BuildItem buildItem = GetBuildItem(assetUrl);

            if (buildItem == null)
            {
                return null;
            }

            string name;

            //��������һ��Ҫƥ���׺
            if (buildItem.resourceType == EResourceType.Dependency)
            {
                string extension = Path.GetExtension(assetUrl).ToLower();
                bool exist = false;
                for (int i = 0; i < buildItem.suffixes.Count; i++)
                {
                    if (buildItem.suffixes[i] == extension)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    return null;
                }
            }

            switch (buildItem.bundleType)
            {
                case EBundleType.All:
                    name = buildItem.assetPath;
                    if (buildItem.assetPath[buildItem.assetPath.Length - 1] == '/')
                        name = buildItem.assetPath.Substring(0, buildItem.assetPath.Length - 1);
                    name = $"{name}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                case EBundleType.Directory:
                    name = $"{assetUrl.Substring(0, assetUrl.LastIndexOf('/'))}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                case EBundleType.File:
                    name = $"{assetUrl}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                default:
                    throw new Exception($"�޷���ȡ{assetUrl}��BundleName");
            }

            buildItem.count += 1;

            return name;
        }
    }
}