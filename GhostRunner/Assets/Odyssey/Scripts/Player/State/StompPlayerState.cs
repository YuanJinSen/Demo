using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class StompPlayerState : PlayerState
    {
        protected float _airTimer;
        protected float _groundTimer;

        protected bool _falling;
        protected bool _landed;

        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            _falling = _landed = false;
            _airTimer = _groundTimer = 0f;
            player.velocity = Vector3.zero;
            player.playerEvents.onStompStarted?.Invoke();
        }

        protected override void OnExit(Player player)
        {
            player.playerEvents.onStompEnding?.Invoke();
        }

        protected override void OnStep(Player player)
        {
            if (!_falling)
            {
                _airTimer += Time.deltaTime;

                if (_airTimer >= player.stats.current.stompAirTime)
                {
                    _falling = true;
                    player.playerEvents.onStompFalling?.Invoke();
                }
            }
            else
            {
                player.verticalVelocity += Vector3.down * player.stats.current.stompDownwardForce;
            }

            if (player.isGrounded)
            {
                if (!_landed)
                {
                    _landed = true;
                    player.playerEvents.onStompLanding?.Invoke();
                }
                
                if (_groundTimer >= player.stats.current.stompGroundTime)
                {
                    player.verticalVelocity = Vector3.up * player.stats.current.stompGroundLeapHeight;
                    player.stateManager.Change<FallPlayerState>();
                }
                _groundTimer += Time.deltaTime;
            }
        }
    }
}