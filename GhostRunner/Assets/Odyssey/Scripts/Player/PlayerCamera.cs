using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

namespace Odyssey
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Camera")]
        public Player player;
        public float maxDistance = 15f;
        public float initAngle = 20f;
        public float heightOffset = 1f;

        [Header("Orbit")]
        public bool canOrbit = true;
        public bool canOrbitWithVelocity = true;
        public float orbitVelocityMultiplier = 5f;
        [Range(0, 90)]
        public float verticalMaxRotation = 80;
        [Range(-90, 0)]
        public float verticalMinRotation = -20;

        [Header("Follower")]
        public float verticalUpDeadzone = 0.15f;
        public float verticalDownDeadzone = 0.15f;
        public float verticalAirUpDeadzone = 10f;
        public float verticalAirDownDeadzone = 0f;
        public float maxVerticalSpeed = 18f;
        public float maxAirVerticalSpeed = 100f;

        protected CinemachineVirtualCamera _camera;
        protected Cinemachine3rdPersonFollow _cameraBody;
        protected CinemachineBrain _brain;
        protected string _targetName = "PlayerFollowerCameraTarget";
        protected Transform _target;
        protected float _cameraDistance;
        protected float _cameraTargetYaw;
        protected float _cameraTargetPitch;
        protected Vector3 _cameraTargetPosition;

        #region Unity

        protected void Awake()
        {

        }

        protected void Start()
        {
            Init();
        }

        protected void LateUpdate()
        {
            HandleOrbit();
            HandleVelocityOrbit();
            HandleOffset();
            MoveTarget();
        }

        #endregion

        #region Private

        protected void Init()
        {
            //Component
            player = FindObjectOfType<Player>();
            _camera = GetComponent<CinemachineVirtualCamera>();
            _brain = Camera.main.GetComponent<CinemachineBrain>();
            _cameraBody = _camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
            //Follower
            _target = new GameObject(_targetName).transform;
            _target.position = player.transform.position;
            //Camera
            _camera.Follow = _target;
            _camera.LookAt = player.transform;
            Reset();
        }

        protected virtual void MoveTarget()
        {
            _target.position = _cameraTargetPosition;
            _target.rotation = Quaternion.Euler(_cameraTargetPitch, _cameraTargetYaw, 0.0f);
            _cameraBody.CameraDistance = _cameraDistance;
        }

        protected virtual bool VelocityFollowerStates()
        {
            return false;
        }

        protected virtual void HandleOffset()
        {
            Vector3 target = player.unsizePosition + Vector3.up * heightOffset;
            Vector3 previousPosition = _cameraTargetPosition;
            float targetHeight = previousPosition.y;

            if (player.isGrounded || VelocityFollowerStates())
            {
                if (target.y > targetHeight + verticalUpDeadzone)
                {
                    float offset = target.y - targetHeight - verticalUpDeadzone;
                    targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
                }
                else if (target.y < targetHeight - verticalDownDeadzone)
                {
                    float offset = target.y - targetHeight + verticalDownDeadzone;
                    targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
                }
            }
            else
            {
                if(target.y > targetHeight + verticalAirUpDeadzone)
                {
                    float offset = target.y - targetHeight - verticalAirUpDeadzone;
                    targetHeight += Mathf.Min(offset, maxAirVerticalSpeed * Time.deltaTime);
                }
                else if (target.y < targetHeight - verticalAirDownDeadzone)
                {
                    float offset = target.y - targetHeight + verticalAirDownDeadzone;
                    targetHeight += Mathf.Max(offset, -maxAirVerticalSpeed * Time.deltaTime);
                }
            }
            _cameraTargetPosition.Set(target.x, targetHeight, target.z);
        }

        protected virtual void HandleVelocityOrbit()
        {
            if (canOrbitWithVelocity && player.isGrounded)
            {
                Vector3 localVelocity = _target.InverseTransformVector(player.velocity);
                _cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
            }
        }

        protected virtual void HandleOrbit()
        {
            if (canOrbit)
            {
                Vector3 dir = player.inputs.GetLookDir();
                if (dir.sqrMagnitude > 0)
                {
                    bool usingMouse = player.inputs.isLookWithMouse();
                    float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;

                    _cameraTargetYaw += dir.x * deltaTimeMultiplier;
                    _cameraTargetPitch -= dir.z * deltaTimeMultiplier;
                    _cameraTargetPitch = ClampAngle(_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
                }
            }
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle > 360)
            {
                angle -= 360;
            }
            if (angle < -360)
            {
                angle += 360;
            }
            return Mathf.Clamp(angle, min, max);
        }

        #endregion

        #region Public

        public virtual void Reset()
        {
            _cameraDistance = maxDistance;
            _cameraTargetYaw = initAngle;
            _cameraTargetPitch = player.transform.rotation.eulerAngles.y;
            _cameraTargetPosition = player.unsizePosition + Vector3.up * heightOffset;
            MoveTarget();
            _brain.ManualUpdate();
        }

        #endregion
    }
}