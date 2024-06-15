using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleFramework
{
    public static class Builder
    {
        public readonly static Vector2 CollectProgressBar = new Vector2(0f, 0.2f);
        public readonly static Vector2 DependencyProgressBar = new Vector2(0.2f, 0.4f);
        public readonly static Vector2 CollectBundleProgressBar = new Vector2(0.4f, 0.5f);
        public readonly static Vector2 GenerateManifestProgressBar = new Vector2(0.5f, 0.6f);
        public readonly static Vector2 BuildBundleProgressBar = new Vector2(0.6f, 0.7f);
        public readonly static Vector2 ClearBundleProgressBar = new Vector2(0.7f, 0.9f);
        public readonly static Vector2 BuildManifestProgressBar = new Vector2(0.9f, 1f);
        //bundle打包Options
        public readonly static BuildAssetBundleOptions BuildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
        public readonly static ParallelOptions ParallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount * 2
        };

        public readonly static string BuildSettingPath = Path.GetFullPath("BuildSetting.xml").Replace("\\", "/");
        /// <summary>临时目录,临时文件的ab包都放在该文件夹，打包完成后会移除</summary>
        public readonly static string TempBuildPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../TempBuild")).Replace("\\", "/");
        /// <summary>临时目录</summary>
        public readonly static string TempPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Temp")).Replace("\\", "/");
        /// <summary>资源描述__文本</summary>
        public readonly static string ResourcePath_Text = $"{TempPath}/Resource.txt";
        /// <summary>资源描述__二进制</summary>
        public readonly static string ResourcePath_Binary = $"{TempPath}/Resource.bytes";
        /// <summary>Bundle描述__文本</summary>
        public readonly static string BundlePath_Text = $"{TempPath}/Bundle.txt";
        /// <summary>Bundle描述__二进制</summary>
        public readonly static string BundlePath_Binary = $"{TempPath}/Bundle.bytes";
        /// <summary>资源依赖描述__文本</summary>
        public readonly static string DependencyPath_Text = $"{TempPath}/Dependency.txt";
        /// <summary>资源依赖描述__文本</summary>
        public readonly static string DependencyPath_Binary = $"{TempPath}/Dependency.bytes";

        private readonly static Profiler ms_BuildProfiler = new Profiler(nameof(Builder));
        private readonly static Profiler ms_SwitchPlatformProfiler = ms_BuildProfiler.CreateChild(nameof(Builder));
        private readonly static Profiler ms_LoadSettingProfiler = ms_BuildProfiler.CreateChild(nameof(Builder));
        private readonly static Profiler ms_CollectProfiler = ms_BuildProfiler.CreateChild(nameof(Collect));
        private readonly static Profiler ms_CollectBuildFileProfiler = ms_CollectProfiler.CreateChild($"{nameof(Collect)}BuildFile");
        private readonly static Profiler ms_CollectDependencyProfiler = ms_CollectProfiler.CreateChild(nameof(CollectDependency));
        private readonly static Profiler ms_CollectBundleProfiler = ms_CollectProfiler.CreateChild(nameof(CollectBundle));
        private readonly static Profiler ms_GenerateManifestProfiler = ms_CollectProfiler.CreateChild(nameof(GenerateManifest));
        private readonly static Profiler ms_BuildBundleProfiler = ms_BuildProfiler.CreateChild(nameof(BuildBundle));
        private static readonly Profiler ms_ClearBundleProfiler = ms_BuildProfiler.CreateChild(nameof(ClearAssetBundle));
        private static readonly Profiler ms_BuildManifestBundleProfiler = ms_BuildProfiler.CreateChild(nameof(BuildManifest));

        public static BuildSetting buildSetting { get; private set; }
        public static string buildPath { get; set; }

#if UNITY_IOS
        pubblic const string PLATFORM = "iOS";
#elif UNITY_ANDROID
        public const string PLATFORM = "Android";
#else
        public const string PLATFORM = "Windows";
#endif
        public const string BUNDLE_SUFFIX = ".ab";
        public const string BUNDLE_MANIFEST_SUFFIX = ".manifest";

        [MenuItem("Tools/ResBuild/Windows")]
        public static void BuildWindows()
        {
            Build();
            Debug.Log($"打包完成{ms_BuildProfiler.ToString()}");
        }

        public static void Build()
        {
            ms_BuildProfiler.Start();

            ms_SwitchPlatformProfiler.Start();
            SwitchPlatform();
            ms_SwitchPlatformProfiler.Stop();

            ms_LoadSettingProfiler.Start();
            LoadSetting(BuildSettingPath);
            ms_LoadSettingProfiler.Stop();

            ms_CollectProfiler.Start();
            Dictionary<string, List<string>> bundleDic = Collect();
            ms_CollectProfiler.Stop();

            ms_BuildBundleProfiler.Start();
            BuildBundle(bundleDic);
            ms_BuildBundleProfiler.Stop();

            ms_ClearBundleProfiler.Start();
            ClearAssetBundle(buildPath, bundleDic);
            ms_ClearBundleProfiler.Stop();

            ms_BuildManifestBundleProfiler.Start();
            BuildManifest();
            ms_BuildManifestBundleProfiler.Stop();

            ms_BuildProfiler.Stop();
            EditorUtility.ClearProgressBar();
        }

        public static void SwitchPlatform()
        {
            switch (PLATFORM)
            {
                case "iOS":
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;
                case "Android":
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
                case "Windows":
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                    break;
            }
        }

        public static BuildSetting LoadSetting(string settingPath)
        {
            buildSetting = XmlUtility.Read<BuildSetting>(settingPath);
            if (buildSetting == null)
            {
                throw new Exception($"Load BuildSetting Failed, settingPath: {settingPath}");
            }
            (buildSetting as ISupportInitialize)?.EndInit();
            buildPath = Path.GetFullPath(buildSetting.buildRoot).Replace("\\", "/");
            if (buildPath.Length > 0 && !buildPath.EndsWith('/'))
            {
                buildPath += "/";
            }
            buildPath += $"{PLATFORM}/";

            return buildSetting;
        }

        public static Dictionary<string, List<string>> Collect()
        {
            //获取所有打包设置中的文件列表
            ms_CollectBuildFileProfiler.Start();
            HashSet<string> files = buildSetting.Collect();
            ms_CollectBuildFileProfiler.Stop();

            //获取所有文件的依赖关系
            ms_CollectDependencyProfiler.Start();
            Dictionary<string, List<string>> dependencyDic = CollectDependency(files);
            ms_CollectDependencyProfiler.Stop();

            Dictionary<string, EResourceType> assetDic = new Dictionary<string, EResourceType>();
            //被打包设置分析到的，为Direct
            foreach (string file in files)
            {
                assetDic.Add(file, EResourceType.Direct);
            }
            //使用依赖搜索到的，为Dependency；已经存在的就是Direct
            foreach (string key in dependencyDic.Keys)
            {
                if(!assetDic.ContainsKey(key))
                    assetDic.Add(key, EResourceType.Dependency);
            }

            ms_CollectBundleProfiler.Start();
            Dictionary<string, List<string>> bundleDic = CollectBundle(buildSetting, assetDic, dependencyDic);
            ms_CollectBundleProfiler.Stop();

            ms_GenerateManifestProfiler.Start();
            GenerateManifest(assetDic, bundleDic, dependencyDic);
            ms_GenerateManifestProfiler.Stop();

            return bundleDic;
        }

        public static Dictionary<string, List<string>> CollectDependency(ICollection<string> files)
        {
            float min = DependencyProgressBar.x;
            float max = DependencyProgressBar.y;

            Dictionary<string, List<string>> dependencyDic = new Dictionary<string, List<string>>();

            List<string> fileList = new List<string>(files);

            for (int i = 0; i < fileList.Count; i++)
            {
                string assetUrl = fileList[i];
                if (dependencyDic.ContainsKey(assetUrl)) continue;
                EditorUtility.DisplayProgressBar(nameof(Collect), "搜集依赖", min + (max - min) * i / fileList.Count);

                string[] dependencies = AssetDatabase.GetDependencies(assetUrl, false);
                List<string> dependencyList = new List<string>(dependencies.Length);
                for (int ii = 0; ii < dependencies.Length; ii++)
                {
                    string tempAssetUrl = dependencies[ii];
                    string extension = Path.GetExtension(tempAssetUrl).ToLower();
                    if (string.IsNullOrEmpty(tempAssetUrl) || extension == ".cs" || extension == ".dll")
                        continue;
                    if (!fileList.Contains(tempAssetUrl)) fileList.Add(tempAssetUrl);
                    dependencyList.Add(tempAssetUrl);   
                }
                dependencyDic.Add(assetUrl, dependencyList);
            }

            return dependencyDic;
        }

        public static Dictionary<string, List<string>> CollectBundle(BuildSetting buildSetting,
            Dictionary<string, EResourceType> assetDic, Dictionary<string, List<string>> dependencyDic)
        {
            float min = CollectBundleProgressBar.x;
            float max = CollectBundleProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(CollectBundle), "搜集Bundle", min);

            Dictionary<string, List<string>> bundleDic = new Dictionary<string, List<string>>();
            List<string> notInRuleList = new List<string>();

            int index = 0;
            foreach (var item in assetDic)
            {
                EditorUtility.DisplayProgressBar(nameof(CollectBundle), "搜集Bundle", min + (max - min) * index++ / assetDic.Count);
                string assetUrl = item.Key;
                string bundleName = buildSetting.GetBundleName(assetUrl, item.Value);
                if (bundleName == null)
                {
                    notInRuleList.Add(assetUrl);
                    continue;
                }

                List<string> list;
                if (!bundleDic.TryGetValue(bundleName, out list))
                {
                    list = new List<string>();
                    bundleDic.Add(bundleName, list);
                }
                list.Add(assetUrl);
            }
            if (notInRuleList.Count > 0)
            {
                string msg = "资源不在打包规则,或者后缀不匹配";
                foreach (string item in notInRuleList) msg += "\n" + item;
                Debug.LogError(msg);
            }

            foreach (var item in bundleDic)
            {
                item.Value.Sort();
            }

            return bundleDic;
        }

        public static void GenerateManifest(Dictionary<string, EResourceType> assetDic, 
            Dictionary<string, List<string>> bundleDic, Dictionary<string, List<string>> dependencyDic)
        {
            float min = GenerateManifestProgressBar.x;
            float max = GenerateManifestProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(GenerateManifest), "生成打包信息", min);

            if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);
            //资源映射ID
            Dictionary<string, ushort> assetIdDic = new Dictionary<string, ushort>();

            #region 生成资源描述信息
            {
                if (assetDic.Count > ushort.MaxValue)
                {
                    EditorUtility.ClearProgressBar();
                    throw new Exception($"资源个数{assetDic.Count}超出{ushort.MaxValue}");
                }

                if (File.Exists(ResourcePath_Text)) File.Delete(ResourcePath_Text);
                if (File.Exists(ResourcePath_Binary)) File.Delete(ResourcePath_Binary);

                //写入资源列表
                StringBuilder sb = new StringBuilder();
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                List<string> keys = new List<string>(assetDic.Keys);
                keys.Sort();

                bw.Write((ushort)assetDic.Count);
                for (ushort i = 0; i < keys.Count; i++)
                {
                    string assetUrl = keys[i];
                    assetIdDic.Add(assetUrl, i);
                    sb.AppendLine($"{i}:{assetUrl}");
                    bw.Write(assetUrl);
                }

                //生成资源描述文件
                ms.Flush();
                byte[] buffer = ms.GetBuffer();
                bw.Close();
                File.WriteAllBytes(ResourcePath_Binary, buffer);
                File.WriteAllText(ResourcePath_Text, sb.ToString(), Encoding.UTF8);
            }
            #endregion

            EditorUtility.DisplayProgressBar(nameof(GenerateManifest), "生成打包信息", min + (max - min) * 0.3f);

            #region 生成bundle描述信息
            {
                if (File.Exists(BundlePath_Text)) File.Delete(BundlePath_Text);
                if (File.Exists(BundlePath_Binary)) File.Delete(BundlePath_Binary);

                //写入Bundle信息
                StringBuilder sb = new StringBuilder();
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                bw.Write((ushort)bundleDic.Count);
                foreach (var kv in bundleDic)
                {
                    string bundleName = kv.Key;
                    List<string> assets = kv.Value;

                    sb.AppendLine(bundleName);
                    bw.Write(bundleName);

                    bw.Write((ushort)assets.Count);
                    for (int i = 0; i < assets.Count; i++)
                    {
                        string assetUrl = assets[i];
                        ushort assetId = assetIdDic[assetUrl];
                        sb.AppendLine($"\t{assetUrl}");
                        bw.Write(assetId);
                    }
                }

                ms.Flush();
                byte[] buffer = ms.GetBuffer();
                bw.Close();
                File.WriteAllBytes(BundlePath_Binary, buffer);
                File.WriteAllText(BundlePath_Text, sb.ToString());
            }
            #endregion

            EditorUtility.DisplayProgressBar(nameof(GenerateManifest), "生成打包信息", min + (max - min) * 0.8f);

            #region 生成资源依赖描述信息
            {
                if (File.Exists(DependencyPath_Text)) File.Delete(DependencyPath_Text);
                if (File.Exists(DependencyPath_Binary)) File.Delete(DependencyPath_Binary);

                //写入资源依赖
                StringBuilder sb = new StringBuilder();
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);

                //保存资源依赖
                List<List<ushort>> dependencyList = new List<List<ushort>>();
                foreach (var kv in dependencyDic)
                {
                    List<string> dependencyAssets = kv.Value;
                    if (dependencyAssets.Count == 0) continue;
                    string assetUrl = kv.Key;

                    List<ushort> ids = new List<ushort>();
                    ids.Add(assetIdDic[assetUrl]);
                    string content = assetUrl;
                    for (int i = 0; i < dependencyAssets.Count; i++)
                    {
                        string dependencyAssetUrl = dependencyAssets[i];
                        content += $"\t{dependencyAssetUrl}";
                        ids.Add(assetIdDic[dependencyAssetUrl]);
                    }
                    sb.AppendLine(content);

                    if (ids.Count > byte.MaxValue)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception($"资源{assetUrl}的依赖个数超出一个字节的上限:{byte.MaxValue}");
                    }
                    dependencyList.Add(ids);
                }

                //写入依赖个数
                bw.Write((ushort)dependencyList.Count);
                for (int i = 0; i < dependencyList.Count; i++)
                {
                    List<ushort> ids = dependencyList[i];
                    bw.Write((ushort)ids.Count);
                    for (int ii = 0; ii < ids.Count; ii++)
                       bw.Write(ids[ii]);
                }

                ms.Flush();
                byte[] buffer = ms.GetBuffer();
                bw.Close();
                File.WriteAllBytes(DependencyPath_Binary, buffer);
                File.WriteAllText(DependencyPath_Text, sb.ToString());
            }
            #endregion

            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar(nameof(GenerateManifest), "生成打包信息", max);
        }

        public static AssetBundleManifest BuildBundle(Dictionary<string, List<string>> bundleDic)
        {
            float min = BuildBundleProgressBar.x;
            float max = BuildBundleProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(BuildBundle), "打包AssetBundle", min);

            if (!Directory.Exists(buildPath)) Directory.CreateDirectory(buildPath);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildPath, GetBundles(bundleDic), BuildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget);

            EditorUtility.DisplayProgressBar(nameof(BuildBundle), "打包AssetBundle", max);
            return manifest;
        }

        public static AssetBundleBuild[] GetBundles(Dictionary<string, List<string>> bundleDic)
        {
            int i = 0;
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[bundleDic.Count];
            foreach (var item in bundleDic)
            {
                assetBundleBuilds[i++] = new AssetBundleBuild()
                {
                    assetBundleName = item.Key,
                    assetNames = item.Value.ToArray()
                };
            }
            return assetBundleBuilds;
        }

        public static void ClearAssetBundle(string path, Dictionary<string, List<string>> bundleDic)
        {
            float min = ClearBundleProgressBar.x;
            float max = ClearBundleProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(ClearAssetBundle), "清除多余AssetBundle文件", min);

            List<string> fileList = GetFiles(path, null, null);
            HashSet<string> fileSet = new HashSet<string>(fileList);
            foreach (string bundle in bundleDic.Keys)
            {
                fileSet.Remove($"{path}{bundle}");
                fileSet.Remove($"{path}{bundle}{BUNDLE_MANIFEST_SUFFIX}");
            }

            fileSet.Remove($"{path}{PLATFORM}");
            fileSet.Remove($"{path}{PLATFORM}{BUNDLE_MANIFEST_SUFFIX}");

            Parallel.ForEach(fileSet, ParallelOptions, File.Delete);

            EditorUtility.DisplayProgressBar(nameof(ClearAssetBundle), "清除多余AssetBundle文件", max);
        }

        public static void BuildManifest()
        {
            float min = BuildManifestProgressBar.x;
            float max = BuildManifestProgressBar.y;
            EditorUtility.DisplayProgressBar(nameof(BuildManifest), "打包Manifest文件", min);

            if (!Directory.Exists(TempBuildPath)) Directory.CreateDirectory(TempBuildPath);
            string prefix = Application.dataPath.Replace("/Assets", "/").Replace("\\", "/");
            string name = "manifest";
            AssetBundleBuild manifest = new AssetBundleBuild()
            {
                assetBundleName = $"{name}{BUNDLE_SUFFIX}",
                assetNames = new string[3] { 
                    ResourcePath_Binary.Replace(prefix,""),
                    BundlePath_Binary.Replace(prefix,""),
                    DependencyPath_Binary.Replace(prefix,""),
                }
            };

            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(TempBuildPath, new AssetBundleBuild[] { manifest }, BuildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget);

            //将文件Copy到Build目录
            if (assetBundleManifest)
            {
                string source = $"{TempBuildPath}/{name}{BUNDLE_SUFFIX}";
                string dest = $"{buildPath}/{name}{BUNDLE_SUFFIX}";
                if (File.Exists(source)) File.Copy(source, dest);
            }
            if (Directory.Exists(TempBuildPath)) Directory.Delete(TempBuildPath, true); ;
            EditorUtility.DisplayProgressBar(nameof(BuildManifest), "打包Manifest文件", max);
        }

        public static List<string> GetFiles(string path, string prefix, params string[] suffixes)
        {
            string[] files = Directory.GetFiles(path, $"*.*", SearchOption.AllDirectories);
            List<string> res = new List<string>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i].Replace('\\', '/');
                if (prefix != null&& !file.StartsWith(prefix, StringComparison.InvariantCulture))
                {
                    continue;
                }
                if (suffixes != null)
                {
                    bool exist = false;
                    for (int ii = 0; ii < suffixes.Length; ii++)
                    {
                        if (file.EndsWith(suffixes[ii], StringComparison.InvariantCulture))
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist) continue;
                }
                res.Add(file);
            }
            return res;
        }
    }
}