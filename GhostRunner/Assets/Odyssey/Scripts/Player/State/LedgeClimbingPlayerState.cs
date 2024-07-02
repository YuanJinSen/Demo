using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class LedgeClimbingPlayerState : PlayerState
    {
        protected IEnumerator _routine;

        public override void OnContact(Player entity, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            _routine = SetPositionRoutine(player);
            player.StartCoroutine(_routine);
        }

        protected override void OnExit(Player player)
        {
            player.StopCoroutine(_routine);
            player.ResetSkinParent();
        }

        protected override void OnStep(Player player)
        {

        }

        protected IEnumerator SetPositionRoutine(Player player)
        {
            float elapsedTime = 0f;
            float halfDuration = player.stats.current.ledgeClimbingDuration / 2;

            Vector3 initPos = player.transform.localPosition;
            Vector3 targetVerticalPos = player.transform.position + Vector3.up * (player.height + Physics.defaultContactOffset);
            Vector3 targetPos = targetVerticalPos + player.transform.forward * player.radius * 2f;

            Transform parent = player.transform.parent;
            if (parent)
            {
                targetVerticalPos = parent.InverseTransformPoint(targetVerticalPos);
                targetPos = parent.InverseTransformDirection(targetPos);
            }
            player.SetSkinParent(parent);
            player.skin.position += player.stats.current.ledgeClimbingSkinOffset;
            while (elapsedTime <= halfDuration)
            {
                elapsedTime += Time.deltaTime;
                player.transform.localPosition = Vector3.Lerp(initPos, targetVerticalPos, elapsedTime / halfDuration);
                yield return null;
            }

            elapsedTime = 0;
            player.transform.localPosition = targetVerticalPos;

            while (elapsedTime <= halfDuration)
            {
                elapsedTime += Time.deltaTime;
                player.transform.localPosition = Vector3.Lerp(targetVerticalPos, targetPos, elapsedTime / halfDuration);
                yield return null;
            }

            player.transform.localPosition = targetPos;
            player.stateManager.Change<IdlePlayerState>();
        }
    }
}