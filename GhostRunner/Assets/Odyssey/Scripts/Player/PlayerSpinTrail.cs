using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class PlayerSpinTrail : MonoBehaviour
    {
        public Transform hand;

        private Player _player;
        private TrailRenderer _trail;

        private void Start()
        {
            _trail = GetComponent<TrailRenderer>();
            _player = GetComponentInParent<Player>();

            _trail.enabled = false;
            _player.stateManager.events.onChange.AddListener(HandleActive);

            transform.parent = hand;
            transform.localPosition = Vector3.zero;
        }

        private void HandleActive()
        {
            if (_player.stateManager.IsCurrentOfType(typeof(SpinPlayerState)))
            {
                _trail.enabled = true;
            }
            else
            {
                _trail.enabled = false;
            }
        }
    }
}