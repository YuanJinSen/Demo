using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class LevelScore : Singleton<LevelScore>
    {
        public bool stopTime { get; set; } = true;
        public float time { get; set; }
        public bool[] stars => _stars.Clone() as bool[];
        public UnityEvent<int> onCoinsSet;
        public UnityEvent<bool[]> onStarsSet;
        public UnityEvent onScoreLoaded;

        private Game _game;
        private GameLevel _level;
        private bool[] _stars = new bool[GameLevel.STARS_PER_LEVEL];

        private int _coins;
        public int coins
        {
            get
            {
                return _coins;
            }
            set
            {
                _coins = value;
                onCoinsSet?.Invoke(value);
            }
        }

        #region Unity

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            _game = Game.Instance;
            _level = _game?.GetCurrentLevel();

            if (_level != null)
            {
                _stars = _level.stars.Clone() as bool[];
            }
            onScoreLoaded?.Invoke();
        }

        private void Update()
        {
            if (!stopTime)
            {
                time += Time.deltaTime;
            }
        }

        #endregion

        #region Private



        #endregion

        #region Public

        public void CollectStar(int index)
        {
            _stars[index] = true;
            onStarsSet?.Invoke(_stars);
        }

        public void Consolidate()
        {
            if (_level != null)
            {
                if (_level.time == 0 || time < _level.time)
                {
                    _level.time = time;
                }
                if (coins > _level.coins)
                {
                    _level.coins = coins;
                }
                _level.stars = _stars.Clone() as bool[];
                _game.RequestSaving();
            }
        }

        #endregion
    }
}