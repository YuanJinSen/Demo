using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Odyssey
{
    [Serializable]
    public class GameData
    {
        public int retries;
        public LevelData[] levels;
        public string createAt;
        public string updateAt;

        public int TotalStars()
        {
            return levels.Aggregate(0, (acc, level) => 
            {
                int stars = level.CollectedStars();
                return acc + stars;
            });
        }

        public int TotalCoins()
        {
            return levels.Aggregate(0, (acc, level) => acc + level.coins);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static GameData FormJson(string data)
        {
            return JsonUtility.FromJson<GameData>(data);
        }

        public static GameData Create()
        {
            string now = DateTime.Now.ToString();
            return new GameData
            {
                retries = Game.Instance.initRetries,
                createAt = now,
                updateAt = now,
                levels = Game.Instance.levels.Select((level) =>
                {
                    return new LevelData { locked = level.locked };
                }).ToArray()
            };
        }

    }
}
