using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Odyssey
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Serializable]
        public class ForcedTranstion
        {
            public int formStateID;
            public int animatotLayer;
            public string toAnimState;
        }

        public Animator animator;
        [Header("Parameters Names")]
        public string stateName = "State";
        public string lastStateName = "Last State";
        public string lateralSpeedName = "Lateral Speed";
        public string verticalSpeedName = "Vertical Speed";
        public string lateralAnimationSpeedName = "Lateral Animation Speed";
        public string healthName = "Health";
        public string jumpCounterName = "Jump Counter";
        public string isGroundedName = "Is Grounded";
        public string isHoldingName = "Is Holding";
        public string onStateChangedName = "On State Changed";
        [Header("Setting")]
        public float minLateralAnimationSpeed = 0.5f;
        public List<ForcedTranstion> forcedTranstionList;

        protected Player _player;
        protected Dictionary<int, ForcedTranstion> _forcedTranstionDic;
        protected int _stateHash;
        protected int _lastStateHash;
        protected int _lateralSpeedHash;
        protected int _verticalSpeedHash;
        protected int _lateralAnimationSpeedHash;
        protected int _healthHash;
        protected int _jumpCounterHash;
        protected int _isGroundedHash;
        protected int _isHoldingHash;
        protected int _onStateChangedHash;

        #region Unity

        private void Awake()
        {
            
        }

        private void Start()
        {
            Init();
        }

        private void LateUpdate()
        {
            HandhleAnimaterParameters();
        }

        #endregion

        #region Private

        protected virtual void Init()
        {
            //Init Forced Transtion
            _forcedTranstionDic = new Dictionary<int, ForcedTranstion>();
            foreach (var item in forcedTranstionList)
            {
                if (!_forcedTranstionDic.ContainsKey(item.formStateID))
                {
                    _forcedTranstionDic.Add(item.formStateID, item);
                }
            }
            //Init Components
            _player = GetComponent<Player>();
            //Init Parameters
            _stateHash = Animator.StringToHash(stateName);
            _lastStateHash = Animator.StringToHash(lastStateName);
            _lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
            _verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
            _lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
            _healthHash = Animator.StringToHash(healthName);
            _jumpCounterHash = Animator.StringToHash(jumpCounterName);
            _isGroundedHash = Animator.StringToHash(isGroundedName);
            _isHoldingHash = Animator.StringToHash(isHoldingName);
            _onStateChangedHash = Animator.StringToHash(onStateChangedName);
            //Init Events
            _player.stateManager.events.onChange.AddListener(() => animator.SetTrigger(_onStateChangedHash));
            _player.stateManager.events.onChange.AddListener(HandhleForcedTranstion);
        }

        protected virtual void HandhleForcedTranstion()
        {
            int lastStateIndex = _player.stateManager.lastStateIndex;
            if (_forcedTranstionDic.ContainsKey(lastStateIndex))
            {
                ForcedTranstion obj = _forcedTranstionDic[lastStateIndex];
                int layer = obj.animatotLayer;
                animator.Play(obj.toAnimState, layer);
            }
        }

        protected virtual void HandhleAnimaterParameters()
        {
            float lateralSpeed = _player.lateralVelocity.magnitude;
            float verticalSpeed = _player.verticalVelocity.y;
            float lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / _player.stats.current.topSpeed);

            animator.SetInteger(_stateHash, _player.stateManager.currentStateIndex);
            animator.SetInteger(_lastStateHash, _player.stateManager.lastStateIndex);
            animator.SetFloat(_lateralSpeedHash, lateralSpeed);
            animator.SetFloat(_verticalSpeedHash, verticalSpeed);
            animator.SetFloat(_lateralAnimationSpeedHash, lateralAnimationSpeed);
            animator.SetInteger(_jumpCounterHash, _player.jumpCounter);
            animator.SetBool(_isGroundedHash, _player.isGrounded);
            animator.SetBool(_isHoldingHash, _player.holding);
        }

        #endregion

        #region Public



        #endregion
    }
}