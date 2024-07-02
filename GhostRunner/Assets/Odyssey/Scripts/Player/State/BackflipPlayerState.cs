using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class BackflipPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.WallDrag(other);
            player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {
            player.SetJumps(1);
            player.playerEvents.onJump?.Invoke();
        }

        protected override void OnExit(Player player)
        {

        }

        protected override void OnStep(Player player)
        {
            player.Gravity(player.stats.current.backflipGravity);
            player.BackflipAcceleration();

            if (player.isGrounded)
            {
                player.lateralVelocity = Vector3.zero;
                player.stateManager.Change<IdlePlayerState>();
            }
            else if (player.verticalVelocity.y < 0)
            {
                player.Spin();
                player.AirDive();
                player.StompAttack();
                player.Glide();
            }
        }
    }
}