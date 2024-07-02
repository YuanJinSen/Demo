using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    public class Panel : MonoBehaviour, IEntityContact
    {
        public bool autoToggle;
        public bool requirePlayer;
        public bool requireStomp;
        public AudioClip activateClip;
        public AudioClip deactivateClip;
        public UnityEvent onActivate;
        public UnityEvent onDeactivate;

        private bool activated;
        private Collider _collider;
        private Collider _entityActivator;
        private Collider _otherActivator;
        private AudioSource _audio;

        #region Unity

        private void Awake()
        {
            
        }

        private void Start()
        {
            tag = GameTag.Panel;
            _collider = GetComponent<MeshCollider>();
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_entityActivator || _otherActivator)
            {
                Vector3 center = _collider.bounds.center;
                float contactOffset = Physics.defaultContactOffset + 0.1f;
                Vector3 size = _collider.bounds.size + Vector3.up * contactOffset;
                Bounds bounds = new Bounds(center, size);

                bool intersectsEntity = _entityActivator && bounds.Intersects(_entityActivator.bounds);
                bool intersectsOther = _otherActivator && bounds.Intersects(_otherActivator.bounds);

                if (intersectsEntity || intersectsOther)
                {
                    Activate();
                }
                else
                {
                    _entityActivator = intersectsEntity ? _entityActivator : null;
                    _otherActivator = intersectsOther ? _otherActivator : null;

                    if (autoToggle)
                    {
                        Deactivate();
                    }
                }
            }
        }

        #endregion

        #region Private

        private void OnCollisionStay(Collision collision)
        {
            if (!(requirePlayer || requireStomp) && !collision.collider.CompareTag(GameTag.Player))
            {
                _otherActivator = collision.collider;
            }
        }

        #endregion

        #region Public

        public void Activate()
        {
            if (!activated)
            {
                if (activateClip)
                {
                    _audio.PlayOneShot(activateClip);
                }

                activated = true;
                onActivate?.Invoke();
            }
        }

        public void Deactivate()
        {
            if (activated)
            {
                if (deactivateClip)
                {
                    _audio.PlayOneShot(deactivateClip);
                }

                activated = false;
                onDeactivate?.Invoke();
            }
        }

        public void OnEntityContact(Entity entity)
        {
            if (entity.velocity.y <= 0 && entity.IsPointUnderStep(_collider.bounds.max))
            {
                if ((!requirePlayer || entity is Player) && 
                    (!requireStomp || (entity as Player).stateManager.IsCurrentOfType(typeof(StompPlayerState))))
                {
                    _entityActivator = entity.controller;
                }
            }
        }

        #endregion
    }
}