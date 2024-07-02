using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Odyssey
{
    public enum Mode
    {
        Binary,
        Json,
        PlayerPrefs
    }

    public class GameSaver : Singleton<GameSaver>
    {
        public Mode mode = Mode.Binary;
        public string binaryFileExtension = ".data";
        public string fileName = "save";

        protected static readonly int TOTAL_SLOTS = 5;

        public virtual GameData[] LoadList()
        {
            GameData[] list = new GameData[TOTAL_SLOTS];

            for (int i = 0; i < TOTAL_SLOTS; i++)
            {
                list[i] = Load(i);
            }

            return list;
        }

        private string GetFillPath(int i)
        {
            string extension = mode == Mode.Json ? ".json" : binaryFileExtension;
            return $"{Application.persistentDataPath}/{fileName}_{i}{extension}";
        }

        public void Save(GameData data, int i)
        {
            switch (mode)
            {
                case Mode.Json:
                    SaveJson(data, i);
                    break;
                case Mode.PlayerPrefs:
                    SavePlayerPrefs(data, i);
                    break;
                case Mode.Binary:
                default:
                    SaveBinary(data, i);
                    break;
            }
        }

        private void SaveJson(GameData data, int i)
        {
            string path = GetFillPath(i);
            string json = data.ToJson();
            File.WriteAllText(path, json);
        }

        private void SavePlayerPrefs(GameData data, int i)
        {
            string json = data.ToJson();
            string key = i.ToString();
            PlayerPrefs.SetString(key, json);
        }

        private void SaveBinary(GameData data, int i)
        {
            string path = GetFillPath(i);
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        public GameData Load(int i)
        {
            switch (mode)
            {
                case Mode.Json:
                    return LoadJson(i);
                case Mode.PlayerPrefs:
                    return LoadPlayerPrefs(i);
                case Mode.Binary:
                default:
                    return LoadBinary(i);
            }
        }

        private GameData LoadJson(int i)
        {
            string path = GetFillPath(i);
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                return GameData.FormJson(data);
            }
            Debug.LogError("未发现文件");
            return null;
        }

        private GameData LoadBinary(int i)
        {
            string path = GetFillPath(i);
            if (File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(path, FileMode.Open);
                var obj = formatter.Deserialize(stream);
                stream.Close();
                return obj as GameData;
            }
            return null;
        }

        private GameData LoadPlayerPrefs(int i)
        {
            string key = i.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return GameData.FormJson(json);
            }
            return null;
        }

        public void Delete(int i)
        {
            switch (mode)
            {
                case Mode.PlayerPrefs:
                    DeletePlayerPrefs(i);
                    break;
                case Mode.Json:
                case Mode.Binary:
                default:
                    DeleteFile(i);
                    break;
            }
        }

        private void DeleteFile(int i)
        {
            string path = GetFillPath(i);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void DeletePlayerPrefs(int i)
        {
            string key = i.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }
}