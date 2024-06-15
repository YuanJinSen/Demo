using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleFramework
{
    public class BuildItem
    {
        [XmlAttribute("AssetPath")]
        public string assetPath { get; set; }

        [XmlAttribute("ResourceType")]
        public EResourceType resourceType { get; set; } = EResourceType.Direct;

        [XmlAttribute("BundleType")]
        public EBundleType bundleType { get; set; } = EBundleType.File;

        [XmlAttribute("Suffix")]
        public string suffix { get; set; } = ".prefab";

        [XmlIgnore]
        public List<string> ignorePaths { get; set; } = new List<string>();

        [XmlIgnore]
        public List<string> suffixes { get; set; } = new List<string>();

        [XmlIgnore]
        public int count { get; set; }
    }
}