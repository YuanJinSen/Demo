using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class IdlePlayerState : PlayerState
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
            player.Gravity();
            player.Fall();
            player.Jump();
            player.Spin();

            Vector3 dir = player.inputs.GetMovementCameraDir();

            if (dir.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
            {
                player.stateManager.Change<WalkPlayerState>();
            }
            else if (player.inputs.GetCrouchAndCraw())
            {
                player.stateManager.Change<CrouchPlayerState>();
            }
        }
    }
}