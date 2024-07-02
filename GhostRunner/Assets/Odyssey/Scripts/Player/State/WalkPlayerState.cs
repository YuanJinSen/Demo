using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class WalkPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
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
            player.Fall();
            player.Jump(); 
            player.SnapToGround();
            player.Spin();
            player.Dash();
            //player.PickAndThrow();
            //player.RegularSlopeFactor();

            Vector3 dir = player.inputs.GetMovementCameraDir();

            if (dir.sqrMagnitude > 0)
            {
                float dot = Vector3.Dot(dir, player.lateralVelocity);
                if (dot >= player.stats.current.brakeThreshold)
                {
                    player.Accelerate(dir);
                    player.FaceDirectionSmooth(dir);
                }
                else
                {
                    player.stateManager.Change<BrakePlayerState>();
                }
            }
            else
            {
                player.Friction();
                if (player.lateralVelocity == Vector3.zero)
                {
                    player.stateManager.Change<IdlePlayerState>();
                }
            }

            if (player.inputs.GetCrouchAndCraw())
            {
                player.stateManager.Change<CrouchPlayerState>();
            }
        }
    }
}