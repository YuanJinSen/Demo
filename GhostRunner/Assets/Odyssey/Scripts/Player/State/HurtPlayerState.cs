using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class HurtPlayerState : PlayerState
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

            if (player.isGrounded && player.verticalVelocity.y <= 0)
            {
                if (player.health.isEmpty)
                {
                    player.stateManager.Change<DiePlayerState>();
                }
                else
                {
                    player.stateManager.Change<IdlePlayerState>();
                }
            }
        }
    }
}