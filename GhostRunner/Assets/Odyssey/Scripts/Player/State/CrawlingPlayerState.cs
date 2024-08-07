using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class CrawlingPlayerState : PlayerState
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
            player.Jump();

            Vector3 inputDir = player.inputs.GetMovementCameraDir();
            if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
            {
                if (inputDir.sqrMagnitude > 0 || !player.canStandUp)
                {
                    player.CrawlingAccelerate(inputDir);
                    player.FaceDirectionSmooth(player.lateralVelocity);

                }
                else if(player.lateralVelocity.sqrMagnitude > 0)
                {
                    player.Decelerate(player.stats.current.crawlingFriction);
                }
                else
                {
                    player.stateManager.Change<CrouchPlayerState>();
                }
            }
            else
            {
                player.stateManager.Change<IdlePlayerState>();
            }
        }
    }
}