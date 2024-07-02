using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Odyssey
{
    public class Sign : MonoBehaviour
    {
        public RectTransform canvas;
        public float scaleDuration;
        public Text text;
        [TextArea(15, 20)]
        public string content;
        public UnityEvent onShow;
        public UnityEvent onHide;

        private Vector3 _initScale;
        private bool _showing;

        #region Unity

        private void Awake()
        {
            _initScale = canvas.localScale;
            canvas.localScale = Vector3.zero;
            text.text = content;
        }

        #endregion

        #region Private

        public void Show()
        {
            if (_showing) return;
            _showing = true;
            onShow?.Invoke();
            StopAllCoroutines();
            StartCoroutine(Scale(Vector3.zero, _initScale));
        }

        public void Hide()
        {
            if (!_showing) return;
            _showing = false;
            onHide?.Invoke();
            StopAllCoroutines();
            StartCoroutine(Scale(canvas.transform.localScale, Vector3.zero));
        }

        protected IEnumerator Scale(Vector3 from, Vector3 to)
        {
            float elapsedTime = 0.0f;
            Vector3 scale;
            while (elapsedTime < scaleDuration)
            {
                scale = Vector3.Lerp(from, to, elapsedTime / scaleDuration);
                canvas.transform.localScale = scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvas.transform.localScale = to;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTag.Player))
            {
                Vector3 p = (other.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, p);
                if (dot > 0)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Hide();
        }

        #endregion
    }
}