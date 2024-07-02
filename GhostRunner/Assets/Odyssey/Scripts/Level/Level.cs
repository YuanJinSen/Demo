using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class Level : Singleton<Level>
    {
        private Player _player;
        public Player Player
        {
            get
            {
                if (!_player)
                {
                    _player = FindObjectOfType<Player>();
                }
                return _player;
            }
        }
    }
}