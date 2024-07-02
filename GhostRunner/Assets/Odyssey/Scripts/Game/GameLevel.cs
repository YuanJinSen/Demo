using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    [Serializable]
    public class GameLevel
    {
        public bool locked;
        public string scene;
        public string name;
        public string description;
        public Sprite image;

        public int coins { get; set; }
        public float time { get; set; }
        public bool[] stars { get; set; } = new bool[STARS_PER_LEVEL];

        public static readonly int STARS_PER_LEVEL = 3;

        public void LoadState(LevelData data)
        {
            coins = data.coins;
            locked = data.locked;
            time = data.time;
            stars = data.stars;
        }

        public LevelData ToData()
        {
            return new LevelData()
            {
                locked = this.locked,
                coins = this.coins,
                time = this.time,
                stars = this.stars
            };
        }

        public static string FormattedTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            return $"{minutes}'{seconds}\"{milliseconds}";
            //return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
        }
    }
}
