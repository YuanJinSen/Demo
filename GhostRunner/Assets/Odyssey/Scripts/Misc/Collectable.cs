using System.Collections;
using UnityEngine;

namespace Odyssey
{
    [RequireComponent(typeof(Collider))]
    public class Collectable : MonoBehaviour
    {
        [Header("General")]
        public GameObject display;
        public bool collectOnContact = true;
        public float ghostingDuration = 0.5f;
        public int times = 1;
        public AudioClip clip;
        public AudioClip bounceClip;
        public ParticleSystem particle;
        [Header("Visibility")]
        public bool isHidden;
        public float quickShowHeight = 2f;
        public float quickShowDuration = .25f;
        public float hideDuration = .5f;
        [Header("Physics")]
        public bool usePhysice;
        public float gravity = 15f;
        public Vector3 initVelocity = new Vector3(0, 12, 0);
        public bool randomizeInitDir = true;
        public float collisionRadius = 0.5f;
        public float bounciness = 0.9f;
        public float minForceToStopPhysice = 3f;
        public float maxBounceVelocityY = 10f;
        [Header("LifeTime")]
        public bool hasLifeTime;
        public float lifeTimeDuration;
        public PlayerEvent onCollect;

        protected AudioSource _audio;
        protected Collider _collider;
        protected Vector3 _velocity;
        protected bool _vanished;
        protected bool _ghosting;
        protected float _elapsedLifeTime;
        protected float _elapsedGhostingTime;
        protected const int _verticalMinRotation = 0;
        protected const int _verticalMaxRotation = 30;
        protected const int _horizontalMinRandom = 0;
        protected const int _horizontalMaxRandom = 360;

        #region Unity

        protected void Awake()
        {
            Init();
        }

        protected void Update()
        {
            if (!_vanished)
            {
                HandleGhosting();
                HandleLifeTime();
                if (usePhysice)
                {
                    HandleMovement();
                    HandleSweep();
                }
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (collectOnContact && other.CompareTag(GameTag.Player))
            {
                if (other.TryGetComponent<Player>(out var player))
                {
                    Collect(player);
                }
            }
        }

        protected void OnDrawGizmos()
        {
            if (usePhysice)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position, collisionRadius);
            }
        }

        #endregion

        #region Public

        public virtual void Collect(Player player)
        {
            if (!_vanished && !_ghosting)
            {
                if (!isHidden)
                {
                    Vanish();
                    particle?.Play();
                }
                else
                {
                    StartCoroutine(QuickShowRoutine());
                }
                StartCoroutine(CollectRoutine(player));
            }
        }

        public virtual void Vanish()
        {
            if (!_vanished)
            {
                _vanished = true;
                _elapsedLifeTime = 0;
                display.SetActive(false);
                _collider.enabled = false;
            }
        }

        #endregion

        #region Private

        protected virtual void HandleGhosting()
        {
            if (_ghosting)
            {
                _elapsedGhostingTime += Time.deltaTime;
                if (_elapsedGhostingTime >= ghostingDuration)
                {
                    _elapsedGhostingTime = 0;
                    _ghosting = false;
                }
            }
        }

        protected virtual void HandleLifeTime()
        {
            if (hasLifeTime)
            {
                _elapsedLifeTime += Time.deltaTime;
                if (_elapsedLifeTime >= lifeTimeDuration)
                {
                    Vanish();
                }
            }
        }

        protected virtual void HandleMovement()
        {
            _velocity.y -= gravity * Time.deltaTime;
        }

        protected virtual void HandleSweep()
        {
            Vector3 dir = _velocity.normalized;
            float magnitude = dir.magnitude;
            float dis = magnitude * Time.deltaTime;

            if (Physics.SphereCast(transform.position, collisionRadius, dir, out var hit, dis,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag(GameTag.Player))
                {
                    Vector3 bounceDir = Vector3.Reflect(dir, hit.normal);
                    _velocity = magnitude * bounciness * bounceDir;
                    _velocity.y = Mathf.Min(_velocity.y, maxBounceVelocityY);
                    _audio.Stop();
                    _audio.PlayOneShot(bounceClip);

                    if (_velocity.y < minForceToStopPhysice)
                    {
                        usePhysice = false;
                    }
                }
            }

            transform.position += _velocity * Time.deltaTime;
        }

        protected virtual IEnumerator QuickShowRoutine()
        {
            float elapsedTime = 0f;
            Vector3 initPos = transform.position;
            Vector3 targetPos = initPos + Vector3.up * quickShowHeight;

            display.SetActive(true);
            _collider.enabled = false;

            while (elapsedTime < quickShowDuration)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(initPos, targetPos, elapsedTime / quickShowDuration);
                yield return null;
            }
            transform.position = targetPos;
            yield return new WaitForSeconds(hideDuration);
            transform.position = initPos;
            Vanish();
        }

        protected virtual IEnumerator CollectRoutine(Player player)
        {
            for (int i = 0; i < times; i++)
            {
                _audio.Stop();
                _audio.PlayOneShot(clip);
                onCollect.Invoke(player);
                yield return new WaitForSeconds(.1f);
            }
        }

        protected virtual void Init()
        {
            //Component
            if (!TryGetComponent(out _audio))
            {
                _audio = gameObject.AddComponent<AudioSource>();
            }
            _collider = gameObject.GetComponent<Collider>();
            //Transform
            transform.parent = null;
            transform.rotation = Quaternion.identity;
            //Display
            display.SetActive(!isHidden);
            //Velocity
            Vector3 dir = initVelocity.normalized;
            float force = initVelocity.magnitude;
            if (randomizeInitDir)
            {
                float randomY = Random.Range(_horizontalMinRandom, _horizontalMaxRandom);
                float randomZ = Random.Range(_verticalMinRotation, _verticalMaxRotation);
                dir = Quaternion.Euler(0, 0, randomZ) * dir;
                dir = Quaternion.Euler(0, randomY, 0) * dir;
            }
            _velocity = dir * force;
        }

        #endregion
    }
}