using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    [RequireComponent(typeof(Animator))]
    public class UIAinmator : MonoBehaviour
    {
        public bool hideOnAwake;
        public string hideTigger = "Hide";
        public string showTigger = "Show";
        public UnityEvent onShow;
        public UnityEvent onHide;

        protected Animator _animator;

        #region Unity

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (hideOnAwake)
            {
                _animator.Play(hideTigger, 0, 1);
            }
        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        #endregion

        #region Private



        #endregion

        #region Public

        public virtual void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public virtual void Show()
        {
            _animator.SetTrigger(showTigger);
            onShow?.Invoke();
        }

        public virtual void Hide()
        {
            _animator.SetTrigger(hideTigger);
            onHide?.Invoke();
        }

        #endregion
    }
}