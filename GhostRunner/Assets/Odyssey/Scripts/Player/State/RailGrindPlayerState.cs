using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Odyssey
{
    public class RailGrindPlayerState : PlayerState
    {
        private bool _backwards;
        private float _speed;
        private float _lastDashTime;

        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            Evaluate(player, out var point, out var forward, out var upward, out _);
            UpdatePosition(player, point, upward);

            _backwards = Vector3.Dot(player.transform.forward, forward) < 0;
            _speed = Mathf.Max(player.lateralVelocity.magnitude, player.stats.current.minGrindInitialSpeed);
            player.velocity = Vector3.zero;
            player.UseCustomCollision(player.stats.current.useCustomCollision);
        }

        protected override void OnExit(Player player)
        {
            player.ExitRail();
            player.UseCustomCollision(false);
        }

        protected override void OnStep(Player player)
        {
            player.Jump();

            if (player.onRails)
            {
                PlayerStats stats = player.stats.current;
                Evaluate(player, out var point, out var forward, out var upward, out var t);
                if (_backwards) forward *= -1;
                float factor = Vector3.Dot(Vector3.up, forward);
                float multiplier = factor < 0 ? stats.slopeDownwardForce : stats.slopeUpwardForce;

                HandleDeceleration(player);
                HandleDash(player);

                if (stats.applyDiveSlopeFactor)
                {
                    _speed -= factor * multiplier * Time.deltaTime;
                }
                _speed = Mathf.Clamp(_speed, stats.minGrindSpeed, stats.gravityTopSpeed);

                Rotate(player, forward, upward);
                player.velocity = forward * _speed;

                if (player.rails.Spline.Closed || (t > 0f && t < 0.9f))
                {
                    UpdatePosition(player, point, upward);
                }
            }
            else
            {
                player.stateManager.Change<FallPlayerState>();
            }
        }

        private void Evaluate(Player player, out Vector3 point, out Vector3 forward, out Vector3 up, out float t)
        {
            Vector3 origin = player.rails.transform.InverseTransformPoint(player.transform.position);
            SplineUtility.GetNearestPoint(player.rails.Spline, origin, out var nearest, out t);
            point = player.rails.transform.TransformPoint(nearest);
            forward = Vector3.Normalize(player.rails.EvaluateTangent(t));
            up = Vector3.Normalize(player.rails.EvaluateUpVector(t));
        }

        private void UpdatePosition(Player player, Vector3 point, Vector3 up)
        {
            float dis = player.originalHeight * 0.5f + player.stats.current.grindRadiusOffset;
            player.transform.position = point + up * dis;
            Debu.Log(player.transform.position);
        }

        private void HandleDeceleration(Player player)
        {
            if (player.stats.current.canGrindBrake && player.inputs.GetGrindBrake())
            {
                float delta = player.stats.current.grindBrakeDeceleration * Time.deltaTime;
                _speed = Mathf.MoveTowards(_speed, 0, delta);
            }
        }

        private void HandleDash(Player player)
        {
            if (player.stats.current.canGrindDash && player.inputs.GetDashDown() &&
               Time.time - _lastDashTime > player.stats.current.grindDashCoolDown)
            {
                _lastDashTime = Time.time;
                _speed = player.stats.current.grindDashForce;
                player.playerEvents.onDashStart?.Invoke();
            }
        }

        private void Rotate(Player player, Vector3 forward, Vector3 upward)
        {
            if (forward != Vector3.zero)
            {
                player.transform.rotation = Quaternion.LookRotation(forward, player.transform.up);
            }

            player.transform.rotation = Quaternion
                .FromToRotation(player.transform.up, upward) * player.transform.rotation;
        }
    }
}