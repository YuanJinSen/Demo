using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class Glider : MonoBehaviour
    {
        public GameObject trails;
        public float scaleDuration;
        public AudioClip open;
        public AudioClip close;

        private Player _player;
        private AudioSource _audio;

        #region Unity

        private void Awake()
        {
            
        }

        private void Start()
        {
            _player = GetComponentInParent<Player>();
            _audio = GetComponent<AudioSource>();

            _player.playerEvents.onGlidingStart.AddListener(Show);
            _player.playerEvents.onGlidingStop.AddListener(Hide);
            transform.localScale = Vector3.zero;
            trails.SetActive(false);
        }

        #endregion

        #region Private

        private void Show()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));
            trails.SetActive(true);
            _audio.PlayOneShot(open);
        }

        private void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero));
            trails.SetActive(false);
            _audio.PlayOneShot(close);
        }

        private IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
        {
            float time = 0f;
            transform.localScale = from;
            while (time < scaleDuration)
            {
                time += Time.deltaTime;
                transform.localScale = Vector3.Lerp(from, to, time / scaleDuration);
                yield return null;
            }
            transform.localScale = to;
        }

        #endregion

        #region Public



        #endregion
    }
}