using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Odyssey
{
    public class Fader : Singleton<Fader>
    {
        public float speed = 1f;
        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
        }

        private void SetAlpha(float a)
        {
            Color color = _image.color;
            color.a = a;
            _image.color = color;
        }

        private IEnumerator FadeOutRoutine(Action onFinished)
        {
            float a = _image.color.a;
            while (a < 1)
            {
                a += speed * Time.deltaTime;
                SetAlpha(a);
                yield return null;
            }
            onFinished?.Invoke();
        }

        private IEnumerator FadeInRoutine(Action onFinished)
        {
            float a = _image.color.a;
            while (a > 0)
            {
                a -= speed * Time.deltaTime;
                SetAlpha(a);
                yield return null;
            }
            onFinished?.Invoke();
        }

        public void FadeIn(Action onFinished)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInRoutine(onFinished));
        }

        public void FadeOut(Action onFinished)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine(onFinished));
        }
    }
}
