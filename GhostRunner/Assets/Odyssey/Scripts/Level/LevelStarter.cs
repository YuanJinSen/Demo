using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class LevelStarter : MonoBehaviour
    {
        public float enabledPlayerDelay = 1f;
        public UnityEvent onStart;

        private Level _level => Level.Instance;
        private LevelScore _score => LevelScore.Instance;
        private LevelPauser _pauser => LevelPauser.Instance;

        #region Unity

        private void Start()
        {
            StartCoroutine(StartRoutine());
        }

        #endregion

        #region Private

        private IEnumerator StartRoutine()
        {
            Game.LockCursor(true);
            _level.Player.controller.enabled = false;
            _level.Player.inputs.enabled = false;
            yield return new WaitForSeconds(enabledPlayerDelay);
            _score.stopTime = false;
            _level.Player.controller.enabled = true;
            _level.Player.inputs.enabled = true;
            _pauser.canPause = true;
            onStart?.Invoke();
        }

        #endregion
    }
}