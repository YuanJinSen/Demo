using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class AirDivePlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            if (!player.isGrounded)
            {
                player.WallDrag(other);
                player.GrabPole(other);
            }
        }

        protected override void OnEnter(Player player)
        {
            player.velocity = Vector3.zero;
            player.lateralVelocity = player.transform.forward * player.stats.current.airDiveForwardForce;
        }

        protected override void OnExit(Player player)
        {

        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.Jump();

            if (player.stats.current.applyDiveSlopeFactor)
            {
                player.SlopeFactor(player.stats.current.slopeUpwardForce, player.stats.current.slopeDownwardForce);
            }
            player.FaceDirection(player.lateralVelocity);
            if (player.isGrounded)
            {
                Vector3 inputDir = player.inputs.GetMovementCameraDir();
                Vector3 localInputDir = player.transform.InverseTransformDirection(inputDir);
                float rotation = localInputDir.x * player.stats.current.airDiveRotationSpeed * Time.deltaTime;

                player.lateralVelocity = Quaternion.Euler(0, rotation, 0) * player.lateralVelocity;

                if (player.OnSlopingGround())
                {
                    player.Decelerate(player.stats.current.airDiveSlopeFriction);
                }
                else
                {
                    player.Decelerate(player.stats.current.airDiveFriction);

                    if (player.lateralVelocity.sqrMagnitude == 0)
                    {
                        player.verticalVelocity = Vector3.up * player.stats.current.airDiveGroundLeapHeight;
                        player.stateManager.Change<FallPlayerState>();
                    }
                }
            }
        }
    }
}