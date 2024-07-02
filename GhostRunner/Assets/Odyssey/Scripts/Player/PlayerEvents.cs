using UnityEngine.Events;
using System;

namespace Odyssey
{
    [Serializable]
    public class PlayerEvents
    {
        public UnityEvent onJump;
        public UnityEvent onHurt;
        public UnityEvent onDie;
        public UnityEvent onSpin;
        public UnityEvent onPickUp;
        public UnityEvent onThrow;
        public UnityEvent onStompStarted;
        public UnityEvent onStompFalling;
        public UnityEvent onStompLanding;
        public UnityEvent onStompEnding;
        public UnityEvent onLedgeGrabbed;
        public UnityEvent onLedgeClimbing;
        public UnityEvent onAirDive;
        public UnityEvent onBackFlip;
        public UnityEvent onGlidingStart;
        public UnityEvent onGlidingStop;
        public UnityEvent onDashStart;
        public UnityEvent onDashEnd;
    }
}