using System;
using System.Linq;

namespace Odyssey
{
    [Serializable]
    public class LevelData
    {
        public bool locked;
        public int coins;
        public float time;
        public bool[] stars = new bool[GameLevel.STARS_PER_LEVEL];

        public int CollectedStars()
        {
            return stars.Where((star) => star).Count();
        }
    }
}