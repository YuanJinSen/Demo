using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class CrouchPlayerState : PlayerState
    {
        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            player.ResizeCollider(player.stats.current.crouchHeight);
        }

        protected override void OnExit(Player player)
        {
            player.ResizeCollider(player.originalHeight);
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.Fall();
            player.Decelerate(player.stats.current.crouchFriction);

            Vector3 inputDir = player.inputs.GetMovementDir();

            if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
            {
                if (inputDir.sqrMagnitude > 0 && !player.holding)
                {
                    if (player.lateralVelocity.sqrMagnitude == 0)
                    {
                        player.stateManager.Change<CrawlingPlayerState>();
                    }
                }
                else if (player.inputs.GetJumpDown())
                {
                    player.Backflip(player.stats.current.backflipBackwardForce);
                }
            }
            else
            {
                player.stateManager.Change<IdlePlayerState>();
            }
        }
    }
}