using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Odyssey
{
    [RequireComponent(typeof(BoxCollider))]
    public class ItemBox : MonoBehaviour, IEntityContact
    {
        public Collectable[] collectables;
        public MeshRenderer itemBoxRander;
        public Material emptyItemBoxMaterial;
        public UnityEvent onCollect;
        public UnityEvent onDisable;

        protected BoxCollider _collider;
        protected Vector3 _initScale;
        protected bool _enable = true;
        protected int _index;

        #region Unity

        protected void Awake()
        {
            
        }

        protected void Start()
        {
            _collider = GetComponent<BoxCollider>();
            _initScale = transform.localScale;
            Init();
        }

        protected void Update()
        {

        }

        #endregion

        #region Private

        protected void Init()
        {
            foreach (Collectable item in collectables)
            {
                if (!item.isHidden)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    item.collectOnContact = false;
                }
            }
        }

        #endregion

        #region Public

        public virtual void OnEntityContact(Entity entity)
        {
            if (entity is Player player)
            {
                if (entity.velocity.y > 0 && entity.position.y < _collider.bounds.min.y)
                {
                    Collect(player);
                }
            }
        }

        public virtual void Collect(Player player)
        {
            if (_enable && _index < collectables.Length)
            {
                if (collectables[_index].isHidden)
                {
                    collectables[_index].Collect(player);
                }
                else
                {
                    collectables[_index].gameObject.SetActive(true);
                }

                _index = Mathf.Clamp(_index + 1, 0, collectables.Length);
                onCollect?.Invoke();

                if (_index == collectables.Length)
                {
                    Disable();
                }
            }
        }

        public void Disable()
        {
            if (_enable)
            {
                _enable = false;
                itemBoxRander.sharedMaterial = emptyItemBoxMaterial;
                onDisable?.Invoke();
            }
        }

        #endregion
    }
}