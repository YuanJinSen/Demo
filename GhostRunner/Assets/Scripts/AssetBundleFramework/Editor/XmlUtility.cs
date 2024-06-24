using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace AssetBundleFramework
{
    public static class XmlUtility
    {
        public static T Read<T>(string fileName) where T : class
        {
            FileStream stream = null;
            if (!File.Exists(fileName)) return default(T);

            try
            {
                stream = File.OpenRead(fileName);

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                XmlReader xmlReader = XmlReader.Create(stream);
                T instance = (T)xmlSerializer.Deserialize(xmlReader);

                stream.Close();
                return instance;
            }
            catch
            {
                if (stream != null) stream.Close();
                return default(T);
            }
        }
    }
}