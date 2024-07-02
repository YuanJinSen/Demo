using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class PoleClimbingPlayerState : PlayerState
    {
        private float _radius;

        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            player.ResetAirDash();
            player.ResetAirSpins();
            player.ResetJumps();
            player.velocity = Vector3.zero;
            player.pole.GetDirectionToPole(player.transform, out _radius);
            player.skin.position += player.stats.current.poleClimbSkinOffset;
        }

        protected override void OnExit(Player player)
        {
            player.skin.position -= player.stats.current.poleClimbSkinOffset;
        }

        protected override void OnStep(Player player)
        {
            Vector3 poleDir = player.pole.GetDirectionToPole(player.transform, out _);
            Vector3 inputDir = player.inputs.GetMovementDir();

            player.FaceDirection(poleDir);
            player.lateralVelocity = player.transform.right * inputDir.x * player.stats.current.climbRotationSpeed;

            if (inputDir.z != 0)
            {
                float speed = inputDir.z > 0 ? player.stats.current.climbUpSpeed : -player.stats.current.climbDownSpeed;
                player.verticalVelocity = Vector3.up * speed;
            }
            else
            {
                player.verticalVelocity = Vector3.zero;
            }

            if (player.inputs.GetJumpDown())
            {
                player.FaceDirection(-poleDir);
                player.DirectionalJump(-poleDir, player.stats.current.poleJumpHeight, player.stats.current.poleJumpDistance);
                player.stateManager.Change<FallPlayerState>();
            }

            if (player.isGrounded)
            {
                player.stateManager.Change<IdlePlayerState>();
            }

            float offset = player.height * 0.5f + player.center.y;
            Vector3 center = new Vector3(player.pole.center.x, player.transform.position.y, player.pole.center.z);
            Vector3 position = center - poleDir * _radius;
            player.transform.position = player.pole.ClampPointToPoleHeight(position, offset);
        }
    }
}