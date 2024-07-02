using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    [RequireComponent(typeof(Collider))]
    public class Hazard : MonoBehaviour, IEntityContact
    {
        public bool isSolid;
        public bool damageOnlyFromAbove;
        public int damage = 1;

        protected Collider _collider;

        private void Awake()
        {
            tag = GameTag.Hazard;
            _collider = GetComponent<Collider>();
            _collider.isTrigger = !isSolid;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTag.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    TryToApplyDamageTo(player);
                }
            }
        }

        protected virtual void TryToApplyDamageTo(Player player)
        {
            if (!damageOnlyFromAbove || player.velocity.y <= 0 &&
                player.IsPointUnderStep(_collider.bounds.max))
            {
                player.ApplyDamage(damage, transform.position);
            }
        }

        public void OnEntityContact(Entity entity)
        {
            if (entity is Player player)
            {
                TryToApplyDamageTo(player);
            }
        }
    }
}