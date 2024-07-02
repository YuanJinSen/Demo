using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class FallPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.WallDrag(other);
            player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {

        }

        protected override void OnExit(Player player)
        {

        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.FaceDirectionSmooth(player.lateralVelocity);
            player.Jump();
            player.SnapToGround();
            player.Spin();
            player.AirDive();
            player.StompAttack();
            player.LedgeGrab();
            player.Glide();
            player.Dash();
            //player.AccelerateToInputDirection();
            //player.PickAndThrow();

            if (player.isGrounded)
            {
                player.stateManager.Change<IdlePlayerState>();
            }
        }
    }
}