using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class LevelFinisher : Singleton<LevelFinisher>
    {
        public float loadingDelay = 1f;
        public bool unlockNextLevel;
        public string nextScene;
        public string exitScene = "LevelSelect";
        public UnityEvent onExit;
        public UnityEvent onFinish;

        private Game _game => Game.Instance;
        private GameLoader _loader => GameLoader.Instance;
        private Level _level => Level.Instance;
        private LevelScore _score => LevelScore.Instance;
        private LevelPauser _pauser => LevelPauser.Instance;

        #region Private

        private IEnumerator ExitRoutine()
        {
            _pauser.Pause(false);
            _pauser.canPause = false;
            _level.Player.inputs.enabled = false;
            yield return new WaitForSeconds(loadingDelay);
            Game.LockCursor(false);
            _loader.Load(exitScene);
            onExit?.Invoke();
        }

        private IEnumerator FinishRoutine()
        {
            _pauser.Pause(false);
            _pauser.canPause = false;
            _score.stopTime = true;
            _level.Player.inputs.enabled = false;

            yield return new WaitForSeconds(loadingDelay);

            if (unlockNextLevel)
            {
                _game.UnlockNextLevel();
            }

            Game.LockCursor(false);
            _score.Consolidate();
            _loader.Load(nextScene);
            onFinish?.Invoke();
        }

        #endregion

        #region Public

        public void Exit()
        {
            StopAllCoroutines();
            StartCoroutine(ExitRoutine());
        }

        public void Finish()
        {
            StopAllCoroutines();
            StartCoroutine(FinishRoutine());
        }

        #endregion
    }
}