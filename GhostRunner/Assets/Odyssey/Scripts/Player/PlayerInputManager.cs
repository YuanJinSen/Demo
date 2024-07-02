using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Odyssey
{
    public class PlayerInputManager : MonoBehaviour
    {
        private InputAction _movement;
        private InputAction _run;
        private InputAction _jump;
        private InputAction _dive;
        private InputAction _spin;
        private InputAction _look;
        private InputAction _glide;
        private InputAction _dash;
        private InputAction _grindBrake;
        private InputAction _pickAndDrop;
        private InputAction _crouch;
        private InputAction _airDive;
        private InputAction _stomp;
        private InputAction _releaseLedge;
        private InputAction _pause;

        private Camera _camera;
        private float? _lastJumpTime;
        private float _jumpBuffer = 0.15f;
        private string _mouseDeviceName = "Mouse";
        private float _movementDirectionUnlockTime;

        public InputActionAsset actions;

        #region Unity

        private void Awake()
        {
            CacheActions();
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_jump.WasPressedThisFrame())
            {
                _lastJumpTime = Time.time;
            }
        }

        private void OnEnable()
        {
            actions?.Enable();
        }

        private void OnDisable()
        {
            actions?.Disable();
        }

        #endregion

        #region Private

        protected void CacheActions()
        {
            _movement = actions["Movement"];
            _run = actions["Run"];
            _jump = actions["Jump"];
            _dive = actions["Dive"];
            _spin = actions["Spin"];
            _pickAndDrop = actions["PickAndDrop"];
            _crouch = actions["Crouch"];
            _airDive = actions["AirDive"];
            _stomp = actions["Stomp"];
            _releaseLedge = actions["ReleaseLedge"];
            _pause = actions["Pause"];
            _look = actions["Look"];
            _glide = actions["Glide"];
            _dash = actions["Dash"];
            _grindBrake = actions["Grind Brake"];
        }

        #endregion

        #region Protected

        #endregion

        #region Public

        public void LockMovementDirection(float duration = 0.25f)
        {
            _movementDirectionUnlockTime = Time.time + duration;
        }

        public Vector3 GetMovementDir()
        {
            Vector2 value = _movement.ReadValue<Vector2>();
            return GetAxisWithCrossDeadZone(value);
        }

        public Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
        {
            float deadzone = InputSystem.settings.defaultDeadzoneMin;
            axis.x = Mathf.Abs(axis.x) > deadzone ? axis.x : 0;
            axis.y = Mathf.Abs(axis.y) > deadzone ? axis.y : 0;
            return new Vector3(axis.x, 0, axis.y);
        }

        public virtual Vector3 GetMovementCameraDir()
        {
            var dir = GetMovementDir();
            if (dir.sqrMagnitude > 0)
            {
                Quaternion rot = Quaternion.AngleAxis(_camera.transform.eulerAngles.y, Vector3.up);
                dir = rot * dir;
                dir.Normalize();
            }
            return dir;
        }

        public virtual bool GetRun()
        {
            return _run.IsPressed();
        }

        public virtual bool GetJumpDown()
        {
            if (_lastJumpTime != null && Time.time - _lastJumpTime < _jumpBuffer)
            {
                _lastJumpTime = null;
                return true;
            }
            return false;
        }

        public virtual bool GetJumpUp()
        {
            return _jump.WasReleasedThisFrame();
        }

        public virtual bool isLookWithMouse()
        {
            if (_look.activeControl == null)
            {
                return false;
            }
            return _look.activeControl.device.name.Equals(_mouseDeviceName);
        }

        public virtual Vector3 GetLookDir()
        {
            Vector2 val = _look.ReadValue<Vector2>();
            if (isLookWithMouse())
            {
                return new Vector3(val.x, 0, val.y);
            }
            return GetAxisWithCrossDeadZone(val);
        }

        public virtual bool GetDive() => _dive.IsPressed();
        public virtual bool GetSpinDown() => _spin.WasPressedThisFrame();
        public virtual bool GetPickAndDropDown() => _pickAndDrop.WasPressedThisFrame();
        public virtual bool GetCrouchAndCraw() => _crouch.IsPressed();
        public virtual bool GetAirDiveDown() => _airDive.WasPressedThisFrame();
        public virtual bool GetStompDown() => _stomp.WasPressedThisFrame();
        public virtual bool GetReleaseLedgeDown() => _releaseLedge.WasPressedThisFrame();
        public virtual bool GetGlide() => _glide.IsPressed();
        public virtual bool GetDashDown() => _dash.WasPressedThisFrame();
        public virtual bool GetGrindBrake() => _grindBrake.IsPressed();
        public virtual bool GetPauseDown() => _pause.WasPressedThisFrame();

        #endregion
    }
}