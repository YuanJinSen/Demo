using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public abstract class Entity : MonoBehaviour
    {
        public Vector3 velocity;
        public Vector3 lateralVelocity
        {
            get { return new Vector3(velocity.x, 0, velocity.z); }
            set
            {
                if (velocity == null) velocity = new Vector3();
                velocity.x = value.x;
                velocity.z = value.z;
            }
        }
        public Vector3 verticalVelocity
        {
            get { return new Vector3(0, velocity.y, 0); }
            set
            {
                if (velocity == null) velocity = new Vector3();
                velocity.y = value.y;
            }
        }
        public float turningDragMultiplier { get; set; } = 1f;
        public float topSpeedMultiplier { get; set; } = 1f;
        public float accelerationMultiplier { get; set; } = 1f;
        public float decelerationMultiplier { get; set; } = 1f;
        public float gravityMultiplier { get; set; } = 1f;
        public CharacterController controller { get; protected set; }
        public bool isGrounded { get; protected set; } = true;
        public float lastGroundTime { get; protected set; }
        public float height => controller.height;
        public float radius => controller.radius;
        public Vector3 center => controller.center;
        public Vector3 position => transform.position + center;
        public Vector3 stepPosition => position + Vector3.down * (height * 0.5f - controller.stepOffset);
        public RaycastHit groundHit { get; protected set; }
        public float originalHeight { get; protected set; }
        public Vector3 unsizePosition => position + (originalHeight - height) * 0.5f * Vector3.up;
        public float groundAngle { get; protected set; }
        public Vector3 groundNormal { get; protected set; }
        public Vector3 localSlopeDirection { get; protected set; }

        protected Collider[] _contactBuffer = new Collider[10];
        protected CapsuleCollider _collider;
        protected readonly float _groundOffset = 0.1f;
        protected readonly float _penetrationOffset = -0.1f;
        protected readonly float _slopingGroundAngle = 20f;

        public virtual bool IsPointUnderStep(Vector3 point)
        {
            return point.y < stepPosition.y;
        }

        public virtual bool CapsuleCast(Vector3 dir, float dis, out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Vector3 origin = position - dir * radius + center;
            Vector3 offet = transform.up * (height * 0.5f - radius);
            Vector3 top = origin + offet;
            Vector3 buttom = origin - offet;
            return Physics.CapsuleCast(top, buttom, radius, dir, out hit, dis + radius, layer, queryTriggerInteraction);
        }

        public virtual void ResizeCollider(float height)
        {
            float delta = height - this.height;
            controller.height = height;
            controller.center += Vector3.up * delta * 0.5f;
        }
    }

    public abstract class Entity<T> : Entity where T : Entity<T>
    {
        public EntityStateManager<T> stateManager { get; protected set; }
        public EntityEvents entityEvents;

        #region Unity

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            if (controller.enabled)
            {
                HandleState();
                HandleController();
                HandleGround();
                HandleContact();
            }
        }

        #endregion

        #region Private

        protected virtual void Init()
        {
            stateManager = GetComponent<EntityStateManager<T>>();
            controller = GetComponent<CharacterController>();
            if (!controller)
            {
                controller = gameObject.AddComponent<CharacterController>();
            }
            controller.skinWidth = 0.005f;
            controller.minMoveDistance = 0;
            originalHeight = controller.height;

            _collider = gameObject.AddComponent<CapsuleCollider>();
            _collider.height = controller.height;
            _collider.radius = controller.radius;
            _collider.center = controller.center;
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        protected virtual void HandleState()
        {
            stateManager.Step();
        }

        protected virtual void HandleController()
        {
            controller.Move(velocity * Time.deltaTime);
        }

        protected virtual void UpdateGround(RaycastHit hit)
        {
            if (isGrounded)
            {
                groundHit = hit;
                groundNormal = groundHit.normal;
                groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
                localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
                transform.parent = hit.collider.CompareTag(GameTag.Platform) ? hit.transform : null;
            }
        }

        protected virtual void HandleGround()
        {
            float dis = height * 0.5f + .1f;
            if (SphereCast(Vector3.down, dis, out var hit) && verticalVelocity.y <= 0)
            {
                if (!isGrounded)
                {
                    if (EvaluateLanding(hit))
                    {
                        EnterGround(hit);
                    }
                    else
                    {
                        HandleHighLedge(hit);
                    }
                }
                else if (IsPointUnderStep(hit.point))
                {
                    UpdateGround(hit);

                    if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
                    {
                        HandleSlopeLimit(hit);
                    }
                }
                else
                {
                    HandleHighLedge(hit);
                }
            }
            else
            {
                ExitGround();
            }
        }

        protected virtual void HandleHighLedge(RaycastHit hit)
        {
            //todo
        }

        protected virtual void HandleContact()
        {
            int overlaps = OverlapEntity(_contactBuffer);
            for (int i = 0; i < overlaps; i++)
            {
                Collider contact = _contactBuffer[i];
                if (!contact.isTrigger && contact.transform != transform)
                {
                    OnContact(contact);
                    IEntityContact[] listeners = contact.GetComponents<IEntityContact>();
                    foreach (IEntityContact listener in listeners)
                    {
                        listener.OnEntityContact((T)this);
                    }
                    if (contact.bounds.min.y > controller.bounds.max.y)
                    {
                        verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
                    }
                }
            }
        }

        protected virtual void OnContact(Collider other)
        {
            if (other)
            {
                stateManager.OnContact(other);
            }
        }

        protected virtual void EnterGround(RaycastHit hit)
        {
            if (!isGrounded)
            {
                isGrounded = true;
                groundHit = hit;
                entityEvents.onGroundEnter?.Invoke();
            }
        }

        protected virtual void ExitGround()
        {
            if (isGrounded)
            {
                isGrounded = false;
                transform.parent = null;
                lastGroundTime = Time.time;
                verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
                entityEvents.onGroundExit?.Invoke();
            }
        }

        protected virtual bool EvaluateLanding(RaycastHit hit)
        {
            return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
        }

        protected virtual void HandleSlopeLimit(RaycastHit hit) { }

        #endregion

        #region Public

        public virtual void Accelerate(Vector3 dir, float turningDrag, float acceleration, float topSpeed)
        {
            float speed = Vector3.Dot(dir, lateralVelocity);
            Vector3 velocity = dir * speed;
            Vector3 turningVelocity = lateralVelocity - velocity;
            float turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            float targetTopSpeed = topSpeed * topSpeedMultiplier;

            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }
            velocity = dir * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = velocity + turningVelocity;
        }

        public virtual bool SphereCast(Vector3 dir, float dis, out RaycastHit hit,
            int layer = Physics.DefaultRaycastLayers, QueryTriggerInteraction interaction = QueryTriggerInteraction.Ignore)
        {
            dis = Mathf.Abs(dis - radius);
            return Physics.SphereCast(position, radius, dir, out hit, dis, layer, interaction);
        }

        public virtual void FaceDirection(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = rotation;
            }
        }

        public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
        {
            if (direction != Vector3.zero)
            {
                Quaternion rotation = transform.rotation;
                float rotationDelta = degreesPerSecond * Time.deltaTime;
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
            }
        }

        public virtual void Decelerate(float deceleration)
        {
            float delta = deceleration * decelerationMultiplier * Time.deltaTime;
            lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
        }

        public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
        {
            float contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
            float overlapRadius = radius + contactOffset;
            float offset = (height + contactOffset) * 0.5f - overlapRadius;
            Vector3 top = position + Vector3.up * offset;
            Vector3 buttom = position + Vector3.down * offset;
            return Physics.OverlapCapsuleNonAlloc(top, buttom, overlapRadius, result);
        }

        public virtual bool OnSlopingGround()
        {
            if (isGrounded && groundAngle > _slopingGroundAngle)
            {
                if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    return Vector3.Angle(hit.normal, Vector3.up) > _slopingGroundAngle;
                else
                    return true;
            }

            return false;
        }

        #endregion
    }
}