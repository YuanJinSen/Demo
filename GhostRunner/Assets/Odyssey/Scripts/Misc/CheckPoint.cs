using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class CheckPoint : MonoBehaviour
    {
        public Transform respawn;
        public AudioClip clip;
        public UnityEvent onActive;
        public bool actived { get; private set; }

        private Collider _collider;
        private AudioSource _audio;

        #region Unity

        private void Start()
        {
            _collider = GetComponent<Collider>();
            _audio = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!actived && other.CompareTag(GameTag.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    Active(player);
                }
            }
        }

        #endregion

        #region Private



        #endregion

        #region Public

        public void Active(Player player)
        {
            if (!actived)
            {
                actived = true;
                _collider.enabled = false;
                _audio.PlayOneShot(clip);
                player.SetRespawn(respawn);
                onActive?.Invoke();
            }
        }

        #endregion
    }
}