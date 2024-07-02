using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class Health : MonoBehaviour
    {
        public int initHealth = 3;
        public int max = 3;
        public float coolDown = 1f;
        public UnityEvent onChange;
        public UnityEvent onDamage;
        public bool isEmpty => Current == 0;
        public bool isRecovering => Time.time < _lastDamageTime + coolDown;

        private float _lastDamageTime;
        private int _current;
        public int Current
        {
            get { return _current; }
            private set
            {
                if (_current != value)
                {
                    _current = Mathf.Clamp(value, 0, max);
                    onChange?.Invoke();
                }
            }
        }

        private void Awake()
        {
            Current = initHealth;
        }

        public void Reset()
        {
            Current = initHealth;
        }

        public void Set(int amount)
        {
            Current = amount;
        }

        public void Increase(int amount)
        {
            Current += amount;
        }

        public void Damage(int amount)
        {
            if (!isRecovering)
            {
                Current -= Mathf.Abs(amount);
                _lastDamageTime = Time.time;
                onDamage?.Invoke();
            }
        }
    }
}