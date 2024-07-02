using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Odyssey
{
    public class HUD : MonoBehaviour
    {
        public string retriesFormat = "00";
        public string coinsFormat = "000";
        public string healthFormat = "0";

        [Header("UI Elements")]
        public Text retries;
        public Text coins;
        public Text health;
        public Text timer;
        public Image[] stars;

        private Game _game;
        private Player _player;
        private LevelScore _score;
        private float timerStep;
        private static float timeRefreshRate = .1f; 

        #region Unity

        private void Awake()
        {
            _game = Game.Instance;
            _score = LevelScore.Instance;
            _player = FindObjectOfType<Player>();

            _score.onScoreLoaded.AddListener(() =>
            {
                _score.onCoinsSet.AddListener(UpdateCoins);
                _score.onStarsSet.AddListener(UpdateStars);
                _game.onRetriesSet.AddListener(UpdateRetries);
                _player.health.onChange.AddListener(UpdateHealth);
                Refresh();
            });
        }

        private void Start()
        {
        }

        private void Update()
        {
            UpdateTimer();
        }

        #endregion

        #region Private

        private void UpdateRetries(int value)
        {
            retries.text = value.ToString();
        }

        private void UpdateCoins(int value)
        {
            coins.text = value.ToString();
        }

        private void UpdateHealth()
        {
            health.text = _player.health.Current.ToString();
        }

        private void UpdateStars(bool[] value)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = value[i];
            }
        }

        private void UpdateTimer()
        {
            timerStep += Time.deltaTime;
            if (timerStep > timeRefreshRate)
            {
                timer.text = GameLevel.FormattedTime(_score.time);
                timerStep = 0;
            }
        }

        #endregion

        #region Public

        public void Refresh()
        {
            UpdateCoins(_score.coins);
            UpdateHealth();
            UpdateRetries(_game.Retries);
            UpdateStars(_score.stars);
        }

        #endregion
    }
}