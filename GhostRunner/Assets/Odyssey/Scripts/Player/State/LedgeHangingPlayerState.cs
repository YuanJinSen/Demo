using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class LedgeHangingPlayerState : PlayerState
    {
        protected bool _keepParent;
        protected Coroutine _clearParentRoutine;

        protected const float k_clearParentDelay = 0.25f;

        public override void OnContact(Player player, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            if (_clearParentRoutine != null)
            {
                player.StopCoroutine(_clearParentRoutine);
            }
            _keepParent = false;
            player.skin.position += player.stats.current.ledgeHangingSkinOffset;
            player.ResetAirDash();
            player.ResetAirSpins();
            player.ResetJumps();
        }

        protected override void OnExit(Player player)
        {
            player.StartCoroutine(ClearParentRoutine(player));
            player.skin.position -= player.stats.current.ledgeHangingSkinOffset;
        }

        protected override void OnStep(Player player)
        {
            float ledgeTopMaxDis = player.radius + player.stats.current.ledgeMaxForwardDistance;
            float ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardDistance;
            Vector3 topOrigin = player.position + Vector3.up * ledgeTopHeightOffset + player.transform.forward * ledgeTopMaxDis;
            Vector3 sideOrigin = player.position + Vector3.up * player.height * 0.5f + Vector3.down * player.stats.current.ledgeSideHeightOffset;
            float rayDis = player.radius + player.stats.current.ledgeSideMaxDistance;
            float rayRadius = player.stats.current.ledgeSideCollisionRadius;

            if (Physics.SphereCast(sideOrigin, rayRadius, player.transform.forward, out var sideHit,
                rayDis, player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore) &&
                Physics.Raycast(topOrigin, Vector3.down, out var topHit, player.height,
                player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
            {
                Vector3 inputDirection = player.inputs.GetMovementDir();
                Vector3 ledgeSideOrigin = sideOrigin + player.transform.right * Mathf.Sign(inputDirection.x) * player.radius;
                float ledgeHeight = topHit.point.y - player.height * 0.5f;
                Vector3 sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;
                float destinationHeight = player.height * 0.5f + Physics.defaultContactOffset;
                Vector3 climbDestination = topHit.point + Vector3.up * destinationHeight +
                    player.transform.forward * player.radius;

                player.FaceDirection(sideForward);

                if (Physics.Raycast(ledgeSideOrigin, sideForward, rayDis,
                    player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
                {
                    player.lateralVelocity = player.transform.right * inputDirection.x * player.stats.current.ledgeMovementSpeed;
                }
                else
                {
                    player.lateralVelocity = Vector3.zero;
                }

                player.transform.position = new Vector3(sideHit.point.x, ledgeHeight, sideHit.point.z) - sideForward * player.radius - player.center;

                if (player.inputs.GetReleaseLedgeDown())
                {
                    player.FaceDirection(-sideForward);
                    player.stateManager.Change<FallPlayerState>();
                }
                else if (player.inputs.GetJumpDown())
                {
                    player.Jump(player.stats.current.maxJumpHeight);
                    player.stateManager.Change<FallPlayerState>();
                }
                else if (inputDirection.z > 0 && player.stats.current.canClimbLedges &&
                        ((1 << topHit.collider.gameObject.layer) & player.stats.current.ledgeClimbingLayers) != 0 &&
                        player.FitsIntoPosition(climbDestination))
                {
                    _keepParent = true;
                    player.stateManager.Change<LedgeClimbingPlayerState>();
                    player.playerEvents.onLedgeClimbing?.Invoke();
                }
            }
            else
            {
                player.stateManager.Change<FallPlayerState>();
            }
        }

        protected IEnumerator ClearParentRoutine(Player player)
        {
            if (_keepParent) yield break;

            yield return new WaitForSeconds(k_clearParentDelay);

            player.transform.parent = null;
        }
    }
}