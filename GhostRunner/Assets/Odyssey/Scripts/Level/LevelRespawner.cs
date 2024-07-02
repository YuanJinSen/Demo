

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class LevelRespawner : Singleton<LevelRespawner>
    {
        public float respawnFadeOutDelay = 1f;
        public float respawnFadeInDelay = 0.5f;
        public float gameOverFadeOutDelay = 5f;
        public float restartFadeOutDelay = 0.5f;
        public UnityEvent onRespawn;
        public UnityEvent onGameOver;

        protected PlayerCamera[] _camera;
        protected Level _level => Level.Instance;
        protected LevelScore _score => LevelScore.Instance;
        protected LevelPauser _pauser => LevelPauser.Instance;
        protected Game _game => Game.Instance;
        protected Fader _fader => Fader.Instance;

        private void Start()
        {
            _camera = FindObjectsOfType<PlayerCamera>();
            _level.Player.playerEvents.onDie.AddListener(() => Respawn(true));
        }

        #region Private

        private void ResetCameras()
        {
            foreach (var item in _camera)
            {
                item.Reset();
            }
        }

        private IEnumerator RespawnRoutine(bool retries)
        {
            _pauser.Pause(false);
            _pauser.canPause = false;
            _level.Player.inputs.enabled = false;

            if (retries && _game.Retries == 0)
            {
                _score.stopTime = true;
                yield return new WaitForSeconds(gameOverFadeOutDelay);
                GameLoader.Instance.Reload();
                onGameOver?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(respawnFadeOutDelay);

            _fader.FadeOut(() => StartCoroutine(Routine(retries)));
        }

        private IEnumerator Routine(bool retries)
        {
            if (retries)
            {
                _game.Retries--;
            }

            _level.Player.Respawn();
            _score.coins = 0;
            ResetCameras();
            onRespawn?.Invoke();

            yield return new WaitForSeconds(respawnFadeInDelay);

            _fader.FadeIn(() =>
            {
                _pauser.canPause = true;
                _level.Player.inputs.enabled = true;
            });
        }

        private IEnumerator RestartRoutine()
        {
            _pauser.Pause(false);
            _pauser.canPause = false;
            _level.Player.inputs.enabled = false;
            yield return new WaitForSeconds(restartFadeOutDelay);
            GameLoader.Instance.Reload();
        }

        #endregion

        #region Public

        public void Respawn(bool retries)
        {
            StopAllCoroutines();
            StartCoroutine(RespawnRoutine(retries));
        }

        public void Restart()
        {
            StopAllCoroutines();
            StartCoroutine(RestartRoutine());
        }

        #endregion
    }
}