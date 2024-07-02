using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class GlidingPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.WallDrag(other);
            player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {
            player.verticalVelocity = Vector3.zero;
            player.playerEvents.onGlidingStart?.Invoke();
        }

        protected override void OnExit(Player player)
        {
            player.playerEvents.onGlidingStop?.Invoke();
        }

        protected override void OnStep(Player player)
        {
            player.LedgeGrab();
            player.FaceDirection(player.lateralVelocity);

            Vector3 inputDir = player.inputs.GetMovementCameraDir();

            HandleGlidingGravity(player);
            player.Accelerate(inputDir, player.stats.current.glidingTurningDrag,
                player.stats.current.airAcceleration, player.stats.current.topSpeed);

            if (player.isGrounded)
            {
                player.stateManager.Change<IdlePlayerState>();
            }
            else if (!player.inputs.GetGlide())
            {
                player.stateManager.Change<FallPlayerState>();
            }
        }

        private void HandleGlidingGravity(Player player)
        {
            float y = player.verticalVelocity.y;
            y -= player.stats.current.glidingGravity * Time.deltaTime;
            y = Mathf.Max(y, -player.stats.current.glidingMaxFallSpeed);
            player.verticalVelocity = new Vector3(0, y, 0);
        }
    }
}