using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class PlayerLevelPuase : MonoBehaviour
    {
        private Player _player;
        private LevelPauser _pauser;

        #region Unity

        private void Awake()
        {
            
        }

        private void Start()
        {
            _player = GetComponent<Player>();
            _pauser = LevelPauser.Instance;
        }

        private void Update()
        {
            if (_player.inputs.GetPauseDown())
            {
                bool isPause = _pauser.isPause;
                _pauser.Pause(!isPause);
            }
        }

        #endregion

        #region Private



        #endregion

        #region Public



        #endregion
    }
}