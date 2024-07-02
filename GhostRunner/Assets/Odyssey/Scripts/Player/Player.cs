using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class Player : Entity<Player>
    {
        public PlayerEvents playerEvents;
        public Transform pickableSlot;
        public Transform skin;
        public PlayerInputManager inputs { get; protected set; }
        public PlayerStatsManager stats { get; protected set; }
        public int jumpCounter { get; protected set; }
        public bool holding { get; protected set; }
        public bool onWater { get; protected set; }
        public Health health { get; protected set; }
        public Collider water { get; protected set; }
        public Pickable pickable { get; protected set; }
        public Pole pole { get; protected set; }
        public int airSpinCounter { get; protected set; }
        public int airDashCounter { get; protected set; }
        public Vector3 lastWallNormal { get; protected set; }
        public float positionDelta { get; protected set; }
        public float lastDashTime { get; protected set; }
        public bool canStandUp => !SphereCast(Vector3.up, originalHeight, out _);

        protected Vector3 _respawnPosition;
        protected Quaternion _respawnRotation;
        protected Vector3 _skinInitialPosition;
        protected Quaternion _skinInitialRotation;

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            inputs = GetComponent<PlayerInputManager>();
            stats = GetComponent<PlayerStatsManager>();
            health = GetComponent<Health>();

            tag = GameTag.Player;
            SetRespawn(transform);
            if (skin)
            {
                _skinInitialPosition = skin.localPosition;
                _skinInitialRotation = skin.localRotation;
            }

            entityEvents.onGroundEnter.AddListener(() =>
            {
                ResetJumps();
                ResetAirSpins();
                ResetAirDash();
            });

            entityEvents.onRailsEnter.AddListener(() =>
            {
                ResetJumps();
                ResetAirSpins();
                ResetAirDash();
                //StartGrind();
            });
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTag.VolumeWater))
            {
                if (!onWater && other.bounds.Contains(unsizePosition))
                {
                    EnterWater(other);
                }
                else if (onWater)
                {
                    Vector3 point = position + Vector3.down * 0.25f;
                    if (!other.bounds.Contains(point))
                    {
                        ExitWater();
                    }
                }
            }
        }

        #endregion

        #region Public

        public void SetJumps(int amount)
        {
            jumpCounter = amount;
        }

        public virtual void SetSkinParent(Transform parent)
        {
            if (skin)
            {
                skin.parent = parent;
            }
        }

        public void ResetSkinParent()
        {
            if (skin)
            {
                skin.parent = transform;
                skin.localPosition = _skinInitialPosition;
                skin.localRotation = _skinInitialRotation;
            }
        }

        public bool FitsIntoPosition(Vector3 position)
        {
            float radius = controller.radius - controller.skinWidth;
            float offset = height * 0.5f - radius;
            Vector3 top = position + Vector3.up * offset;
            Vector3 bottom = position - Vector3.up * offset;

            return !Physics.CheckCapsule(top, bottom, radius,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        public void LedgeGrab()
        {
            if (stats.current.canLedgeHang && verticalVelocity.y < 0 && !holding &&
                stateManager.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
                DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
            {
                if (hit.collider is SphereCollider || hit.collider is CapsuleCollider) return;

                float ledgeDis = radius + stats.current.ledgeMaxForwardDistance;
                Vector3 lateralOffset = transform.forward * ledgeDis;
                Vector3 verticalOffset = center + Vector3.up * height * 0.5f;
                velocity = Vector3.zero;
                transform.parent = hit.collider.CompareTag(GameTag.Platform) ? hit.transform : null;
                transform.position = hit.point - lateralOffset - verticalOffset;
                stateManager.Change<LedgeHangingPlayerState>();
                playerEvents.onLedgeGrabbed?.Invoke();
            }
        }

        public void SlopeFactor(float upwardForce, float downwardForce)
        {
            if(!isGrounded || !OnSlopingGround()) return;

            float factor = Vector3.Dot(Vector3.up, groundNormal);
            bool downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
            float multiplier = downwards ? downwardForce : upwardForce;
            float delta = factor * multiplier * Time.deltaTime;
            lateralVelocity += localSlopeDirection * delta;
        }

        public void AirDive()
        {
            if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
            {
                stateManager.Change<AirDivePlayerState>();
                playerEvents.onAirDive?.Invoke();
            }
        }

        public void WaterAcceleration(Vector3 dir)
        {
            Accelerate(dir, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);
        }

        public void WaterFaceDirection(Vector3 dir)
        {
            FaceDirection(dir, stats.current.waterRotationSpeed);
        }

        public void GrabPole(Collider other)
        {
            if (stats.current.canPoleClimb && verticalVelocity.y <= 0 && !holding && other.TryGetComponent<Pole>(out var pole))
            {
                this.pole = pole;
                stateManager.Change<PoleClimbingPlayerState>();
            }
        }

        public void DirectionalJump(Vector3 dir, float height, float distance)
        {
            jumpCounter++;
            verticalVelocity = Vector3.up * height;
            lateralVelocity = dir * distance;
            playerEvents.onJump?.Invoke();
        }

        public void ResetAirSpins()
        {
            airSpinCounter = 0;
        }

        public void ResetAirDash()
        {
            airDashCounter = 0;
        }

        public void ResetJumps()
        {
            jumpCounter = 0;
        }

        public void Accelerate(Vector3 dir)
        {
            bool isRunInGround = isGrounded && inputs.GetRun();
            float turningDrag = isRunInGround ? stats.current.runningTurnSpeed : stats.current.turnSpeed;
            float acceleration = isRunInGround ? stats.current.runningAcceleration : stats.current.acceleration;
            float topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;
            float finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;

            Accelerate(dir, turningDrag, finalAcceleration, topSpeed);
        }

        public void FaceDirectionSmooth(Vector3 direction)
        {
            FaceDirection(direction, stats.current.rotationSpeed);
        }

        public void Decelerate()
        {
            Decelerate(stats.current.deceleration);
        }

        public void Friction()
        {
            Decelerate(stats.current.friction);
        }

        public void Gravity()
        {
            float speed = verticalVelocity.y;
            if (!isGrounded && speed > -stats.current.gravityTopSpeed)
            {
                float force = speed > 0 ? stats.current.gravity : stats.current.fullGravity;
                speed -= force * gravityMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -stats.current.gravityTopSpeed, stats.current.gravityTopSpeed);
                verticalVelocity = new Vector3(0, speed, 0);
            }
        }

        public void Gravity(float gravity)
        {
            if (!isGrounded)
            {
                verticalVelocity -= new Vector3(0, gravity * gravityMultiplier * Time.deltaTime, 0);
            }
        }

        public void Fall()
        {
            if (!isGrounded)
            {
                stateManager.Change<FallPlayerState>();
            }
        }

        public void Jump(float height)
        {
            jumpCounter++;
            verticalVelocity = Vector3.up * height;
            stateManager.Change<FallPlayerState>();
            playerEvents.onJump?.Invoke();
        }

        public void Jump()
        {
            bool canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJump);
            bool canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);

            if (isGrounded || canMultiJump || canCoyoteJump)
            {
                if (inputs.GetJumpDown())
                {
                    Jump(stats.current.minJumpHeight);
                }
            }

            if (inputs.GetJumpUp() && jumpCounter > 0 && verticalVelocity.y > stats.current.minJumpHeight)
            {
                verticalVelocity = Vector3.up * stats.current.minJumpHeight;
            }
        }

        public void PushRigidbody(Collider other)
        {
            if (IsPointUnderStep(other.bounds.max) && other.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                Vector3 force = lateralVelocity * stats.current.pushForce;
                rigidbody.velocity = force / rigidbody.mass * Time.deltaTime;
            }
        }

        public void SetRespawn(Transform respawn)
        {
            _respawnPosition = respawn.position;
            _respawnRotation = respawn.rotation;
        }

        public void Respawn()
        {
            health.Reset();
            transform.SetPositionAndRotation(_respawnPosition, _respawnRotation);
            stateManager.Change<IdlePlayerState>();
        }

        public void ApplyDamage(int amount, Vector3 origin)
        {
            if (!health.isEmpty && !health.isRecovering)
            {
                health.Damage(amount);
                Vector3 dir = origin - transform.position;
                dir.y = 0;
                dir = dir.normalized;
                FaceDirection(dir);
                lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

                if (!onWater)
                {
                    verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                    stateManager.Change<HurtPlayerState>();
                }

                playerEvents.onHurt?.Invoke();

                if (health.isEmpty)
                {
                    Throw();
                    playerEvents.onDie?.Invoke();
                }
            }
        }

        public void Throw()
        {
            if (holding)
            {
                var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
                pickable.Release(transform.forward, force);
                pickable = null;
                holding = false;
                playerEvents.onThrow?.Invoke();
            }
        }

        public void SnapToGround()
        {
            //todo
        }

        public void EnterWater(Collider water)
        {
            if (!onWater && !health.isEmpty)
            {
                Throw();
                onWater = true;
                this.water = water;
                stateManager.Change<SwimPlayerState>();
            }
        }

        public void ExitWater()
        {
            if (onWater)
            {
                onWater = false;
            }
        }

        public void WallDrag(Collider other)
        {
            if (stats.current.canWallDrag && verticalVelocity.y <= 0 && !holding && !other.TryGetComponent<Rigidbody>(out _))
            {
                if (CapsuleCast(transform.forward, 0.25f, out var hit, stats.current.wallDragLayers)
                    && !DetectingLedge(0.25f, height, out _))
                {
                    if (other.CompareTag(GameTag.Platform))
                    {
                        transform.parent = other.transform;
                    }
                    lastWallNormal = hit.normal;
                    stateManager.Change<WallDragPlayerState>();
                }
            }
        }

        protected bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
        {
            float contactOffset = Physics.defaultContactOffset + positionDelta;
            float ledgeMaxDistance = radius + forwardDistance;
            float ledgeHeightOffset = height * 0.5f + contactOffset;
            Vector3 upwardOffset = transform.up * ledgeHeightOffset;
            Vector3 forwardOffset = transform.forward * ledgeMaxDistance;

            if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(position + forwardOffset * .01f, transform.up, ledgeHeightOffset,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                ledgeHit = new RaycastHit();
                return false;
            }

            Vector3 origin = position + upwardOffset + forwardOffset;
            float distance = downwardDistance + contactOffset;

            return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance,
                stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
        }

        public void Backflip(float force)
        {
            if (stats.current.canBackflip && !holding)
            {
                verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
                lateralVelocity = -transform.forward * force;
                stateManager.Change<BackflipPlayerState>();
                playerEvents.onBackFlip?.Invoke();
            }
        }

        public void BackflipAcceleration()
        {
            Vector3 dir = inputs.GetMovementCameraDir();
            Accelerate(dir, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
        }

        public void CrawlingAccelerate(Vector3 direction)
        {
            Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);
        }

        public void Spin()
        {
            bool canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;

            if (inputs.GetSpinDown() && stats.current.canSpin && canAirSpin && !holding)
            {
                if (!isGrounded)
                {
                    airSpinCounter++;
                }
                stateManager.Change<SpinPlayerState>();
                playerEvents.onSpin?.Invoke();
            }
        }

        public void StompAttack()
        {
            if (!isGrounded && inputs.GetStompDown() && !holding && stats.current.canStompAttack)
            {
                stateManager.Change<StompPlayerState>();
            }
        }

        public void Glide()
        {
            if (!isGrounded && inputs.GetGlide() && verticalVelocity.y <= 0 && stats.current.canGlide)
            {
                stateManager.Change<GlidingPlayerState>();
            }
        }

        public void Dash()
        {
            bool canAirDash = stats.current.canAirDash && !isGrounded 
                && airDashCounter < stats.current.allowedAirDashes;
            bool canGroundDash = stats.current.canGroundDash && isGrounded
                && Time.time - lastDashTime > stats.current.groundDashCoolDown;

            if (inputs.GetDashDown() && (canAirDash || canGroundDash))
            {
                if (!isGrounded)
                {
                    airDashCounter++;
                }
                lastDashTime = Time.time;
                stateManager.Change<DashPlayerState>();
            }
        }

        #endregion
    }
}