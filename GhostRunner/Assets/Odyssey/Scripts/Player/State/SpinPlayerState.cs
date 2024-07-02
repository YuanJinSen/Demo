using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class SpinPlayerState : PlayerState
    {
        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            if (!player.isGrounded)
            {
                player.verticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
            }
        }

        protected override void OnExit(Player player)
        {

        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.AirDive();
            player.StompAttack();
            //player.AccelerateToInputDirection();

            if (timeSinceEntered >= player.stats.current.spinDuration)
            {
                if (player.isGrounded)
                {
                    player.stateManager.Change<IdlePlayerState>();
                }
                else
                {
                    player.stateManager.Change<FallPlayerState>();
                }
            }
        }
    }
}