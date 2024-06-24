using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetBundleFramework
{
    public class Test_CallBack : MonoBehaviour
    {
        private string Platform { get; set; }
        private string PrefixPath { get; set; }

        private void Start()
        {
            Platform = GetPlatform();
            PrefixPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../AssetBundle")).Replace("\\", "/");
            PrefixPath += $"/{Platform}";
            ResourceManager.instance.Init(Platform, GetFileUrl, false, 0);

            Init();
        }

        private void Update()
        {
            ResourceManager.instance.Update();
        }

        private void LateUpdate()
        {
            ResourceManager.instance.LateUpdate();
        }

        private void Init()
        {
            ResourceManager.instance.LoadWithCallback("Assets/AssetBundle/UI/UIRoot.prefab", true, uiRootResource =>
            {
                uiRootResource.Instantiate();

                Transform uiParent = GameObject.Find("Canvas").transform;

                ResourceManager.instance.LoadWithCallback("Assets/AssetBundle/UI/TestUI.prefab", true, testUIResource =>
                {
                    testUIResource.Instantiate(uiParent, false);
                });
            });
        }

        private string GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                default:
                    throw new Exception($"未支持的平台：{Application.platform}");
            }
        }

        private string GetFileUrl(string assetUrl)
        {
            return $"{PrefixPath}/{assetUrl}";
        }
    }
}