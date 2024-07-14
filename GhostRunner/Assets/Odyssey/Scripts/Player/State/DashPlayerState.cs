using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class DashPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.WallDrag(other);
            player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {
            player.verticalVelocity = Vector3.zero;
            player.lateralVelocity = player.transform.forward * player.stats.current.dashForce;
            player.playerEvents.onDashStart?.Invoke();
        }

        protected override void OnExit(Player player)
        {
            player.lateralVelocity = Vector3.ClampMagnitude(player.lateralVelocity, player.stats.current.topSpeed);
            player.playerEvents.onDashEnd?.Invoke();
        }

        protected override void OnStep(Player player)
        {
            player.Jump();
            if (timeSinceEntered > player.stats.current.dashDuration)
            {
                if (player.isGrounded)
                {
                    player.stateManager.Change<WalkPlayerState>();
                }
                else
                {
                    player.stateManager.Change<FallPlayerState>();
                }
            }
        }
    }
}