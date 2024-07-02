using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class BrakePlayerState : PlayerState
    {
        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {

        }

        protected override void OnExit(Player player)
        {

        }

        protected override void OnStep(Player player)
        {
            Vector3 inputDirection = player.inputs.GetMovementCameraDir();

            if (player.stats.current.canBackflip && player.inputs.GetJumpDown() &&
                Vector3.Dot(inputDirection, player.transform.forward) < 0)
            {
                player.Backflip(player.stats.current.backflipBackwardTurnForce);
            }
            else
            {
                player.SnapToGround();
                player.Jump();
                player.Fall();
                player.Decelerate();

                if (player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.stateManager.Change<IdlePlayerState>();
                }
            }
        }
    }
}