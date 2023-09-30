// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// Interface to inherit any sensitive moving object on which to maintain control from.
    /// <para/>
    /// Provides multiple common utilities to properly move an object in space.
    /// </summary>
    public interface IMovable3D {
        #region Content
        /// <summary>
        /// This object <see cref="UnityEngine.Rigidbody"/>.
        /// </summary>
        Rigidbody Rigidbody { get; }

        // -----------------------

        /// <summary>
        /// Set this object world position.
        /// <br/> Use this instead of setting <see cref="Transform.position"/>.
        /// </summary>
        void SetPosition(Vector3 _position);

        /// <summary>
        /// Set this object world rotation.
        /// <br/> Use this instead of setting <see cref="Transform.rotation"/>.
        /// </summary>
        void SetRotation(Quaternion _rotation);
        #endregion
    }

    /// <summary>
    /// <see cref="Movable3D"/> global velocity wrapper.
    /// </summary>
    [Serializable]
    public class Velocity {
        #region Velocity
        /// <summary>
        /// Velocity of the object itself, in absolute world coordinates
        /// <br/> (non object-oriented).
        /// <para/>
        /// In unit/second.
        /// </summary>
        [Tooltip("Velocity of the object itself, in absolute world coordinates\n\nIn unit/second")]
        public Vector3 Movement = Vector3.zero;

        /// <summary>
        /// Velocity of the object itself, in absolute world coordinates
        /// <br/> (non object-oriented).
        /// <para/>
        /// In unit/frame.
        /// </summary>
        [Tooltip("Instant velocity applied on the object, for this frame only, in absolute world coordinates\n\nIn unit/frame")]
        public Vector3 InstantMovement = Vector3.zero;

        /// <summary>
        /// External velocity applied on the object, in absolute world coordinates
        /// <br/> (non object-oriented).
        /// <para/>
        /// In unit/second.
        /// </summary>
        [Tooltip("External velocity applied on the object, in absolute world coordinates\n\nIn unit/second")]
        public Vector3 Force = Vector3.zero;

        /// <summary>
        /// Instant velocity applied on the object, for this frame only, in absolute world coordinates
        /// <br/> (non object-oriented).
        /// <para/>
        /// In unit/frame.
        /// </summary>
        [Tooltip("Instant velocity applied on the object, for this frame only, in absolute world coordinates\n\nIn unit/frame")]
        public Vector3 Instant = Vector3.zero;
        #endregion

        #region Utility
        internal void ComputeImpact(RaycastHit _hit) {
            Force = Force.PerpendicularSurface(_hit.normal);
        }

        internal void ResetFrameVelocity() {
            Movement = InstantMovement
                     = Instant
                     = Vector3.zero;
        }

        internal void Reset() {
            Movement = InstantMovement 
                     = Force
                     = Instant
                     = Vector3.zero;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="Velocity"/> frame wrapper.
    /// </summary>
    [Serializable]
    public struct FrameVelocity {
        #region Velocity
        public Vector3 Movement;
        public Vector3 Force;
        public Vector3 Instant;
        public Quaternion Rotation;
        #endregion
    }

    /// <summary>
    /// Object-related gravity mode.
    /// </summary>
    public enum GravityMode {
        /// <summary>
        /// Uses the world global vectors for gravity.
        /// </summary>
        World = 0,

        /// <summary>
        /// Uses the surface of the nearest ground as the reference gravity vector.
        /// </summary>
        Dynamic = 1,
    }

    /// <summary>
    /// Base class for every moving object of the game using complex velocity and collision detections.
    /// </summary>
    [SelectionBase, RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Physics 3D/Movable 3D"), DisallowMultipleComponent]
    #pragma warning disable
    public class Movable3D : EnhancedBehaviour, IMovable3D, IMovableUpdate, ITriggerActor {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Movable;

        #region Global Members
        [Section("Movable"), PropertyOrder(0)]

        [Tooltip("Collider used for detecting physics collisions")]
        [SerializeField] protected new PhysicsCollider3D collider   = new PhysicsCollider3D();

        [Tooltip("Collider used for detecting triggers")]
        [SerializeField] protected PhysicsCollider3D trigger        = new PhysicsCollider3D();

        // -----------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f), PropertyOrder(2)]

        [Tooltip("Current speed of this object")]
        [SerializeField, Enhanced, ReadOnly("IsSpeedEditable")] protected float speed = 1f;

        [Tooltip("Current coefficient applied on this object Velocity")]
        [SerializeField, Enhanced, ReadOnly] protected float velocityCoef = 1f;

        [Space(10f)]

        [Tooltip("Sends a log about this object Frame Velocity every frame")]
        [SerializeField] private bool debugVelocity = false;

        [Tooltip("Makes sure that when this object stops moving, its Velocity is equalized based on the previous frame instead of continuing on its actual Force")]
        [SerializeField] protected bool equalizeVelocity = false;

        [Tooltip("If true, continuously refresh this object position every frame, even if no velocity was applied")]
        [SerializeField] protected bool refreshContinuously = false;

        [Space(10f)]

        /// <summary>
        /// Global velocity of this object.
        /// </summary>
        public Velocity Velocity = new Velocity();

        // -----------------------

        [Space(10f, order = 0),         HorizontalLine(SuperColor.Grey, 1f, order = 1), Space(10f, order = 2)]
        [Title("Gravity", order = 4),   Space(5f, order = 5)]

        [Tooltip("Applies gravity on this object, every frame")]
        [SerializeField] private bool useGravity = true;

        [Space(5f)]

        [Tooltip("Mode used to apply gravity on this object")]
        [SerializeField] private GravityMode gravityMode = GravityMode.World;

        [Tooltip("Direction in which to apply gravity on this object, in absolute world coordinates")]
        [SerializeField] private Vector3 gravitySense = Vector3.down;

        [Space(5f)]

        [Tooltip("Coefficient applied to this object gravity")]
        [SerializeField] private float gravityFactor = 1f;

        [Space(20f, order = 0), Title("Ground", order = 1), Space(5f, order = 2)]

        [Tooltip("Is this object currently on a ground surface?")]
        [SerializeField, Enhanced, ReadOnly(true)] private bool isGrounded = false;

        [Tooltip("Normal of this object current ground surface")]
        [SerializeField, Enhanced, ReadOnly] private Vector3 groundNormal = Vector3.up;

        [Space(10f)]

        [Tooltip("Percentage on which to orientate this object according to its current ground surface")]
        [SerializeField, Enhanced, Range(0f, 1f)] protected float groundOrientationFactor = 1f;

        [Tooltip("Speed used to orientate this object according to its current ground surface\n\nIn quarter-circle per second")]
        [SerializeField, Enhanced, Range(0f, 100f)] protected float groundOrientationSpeed = 1f;

        // -----------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Frame displacement Velocity at the last frame")]
        [SerializeField, Enhanced, ReadOnly] protected FrameVelocity lastFrameVelocity = new FrameVelocity();

        [Space(5f)]

        [Tooltip("Last recorded position of this object")]
        [SerializeField, Enhanced, ReadOnly] protected Vector3 lastPosition = new Vector3();

        // -----------------------

        [SerializeField, HideInInspector] protected new Rigidbody rigidbody = null;
        [SerializeField, HideInInspector] protected new Transform transform = null;
        [SerializeField, HideInInspector] protected Collider[] ignoredColliders = new Collider[0];

        private bool shouldBeRefreshed = false;

        // -------------------------------------------
        // Properties
        // -------------------------------------------

        /// <summary>
        /// Collider used for detecting physics collisions.
        /// </summary>
        public PhysicsCollider3D Collider {
            get { return collider; }
        }

        /// <summary>
        /// Collider used for detecting triggers.
        /// </summary>
        public PhysicsCollider3D Trigger {
            get { return trigger; }
        }

        /// <summary>
        /// Current speed of this object.
        /// </summary>
        public float Speed {
            get { return speed; }
        }

        /// <summary>
        /// Is this object currently on a ground surface?
        /// </summary>
        public bool IsGrounded {
            get { return isGrounded; }
        }

        /// <summary>
        /// Mode used to apply gravity on this object.
        /// </summary>
        public GravityMode GravityMode {
            get { return gravityMode; }
            set {
                gravityMode = value;
                this.LogMessage($"New GravityMode assigned: {value.ToString().Bold()}");
            }
        }

        /// <summary>
        /// Direction in which to apply gravity on this object, in absolute world coordinates.
        /// </summary>
        public Vector3 GravitySense {
            get { return gravitySense; }
        }

        /// <summary>
        /// Normal on this object current ground surface.
        /// </summary>
        public Vector3 GroundNormal {
            get { return groundNormal; }
        }

        /// <summary>
        /// If true, applies gravity on this object every frame.
        /// </summary>
        public bool UseGravity {
            get { return useGravity; }
            set { useGravity = value; }
        }

        /// <summary>
        /// Coefficient applied to this object gravity.
        /// </summary>
        public float GravityFactor {
            get { return gravityFactor; }
            set { gravityFactor = value; }
        }

        /// <summary>
        /// Frame displacement velocity at the last frame.
        /// </summary>
        public FrameVelocity PreviousFrameVelocity {
            get { return lastFrameVelocity; }
        }

        // -----------------------

        /// <summary>
        /// <see cref="CollisionSystem3DType"/> used to calculate how this object moves and collides with other objects in space.
        /// </summary>
        public virtual CollisionSystem3DType CollisionType {
            get { return collisionController.CollisionType; }
        }

        /// <summary>
        /// Maximum height used to climb steps and surfaces (Creature collisions only).
        /// </summary>
        public virtual float ClimbHeight {
            get { return Physics3DSettings.I.ClimbHeight; }
        }

        /// <summary>
        /// Maximum height used for snapping to the nearest surface (Creature collisions only).
        /// </summary>
        public virtual float SnapHeight {
            get { return Physics3DSettings.I.SnapHeight; }
        }

        public Rigidbody Rigidbody {
            get { return rigidbody; }
        }

        public override Transform Transform {
            get { return transform; }
        }

        /// <summary>
        /// Whether this object speed value should be editable in the inspector or not.
        /// </summary>
        public virtual bool IsSpeedEditable {
            get { return true; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Enable colliders.
            EnableColliders(true);
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            collider.Initialize(GetColliderMask());
            trigger.Initialize(GetTriggerMask());

            rigidbody.isKinematic = true;
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Clear state.
            ExitTriggers();

            // Disable colliders.
            EnableColliders(false);
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            // Editor required components validation.
            if (Application.isPlaying) {
                return;
            }

            if (!transform) {
                transform = GetComponent<Transform>();
            }

            if (!rigidbody) {
                rigidbody = GetComponent<Rigidbody>();
            }

            ignoredColliders = GetComponentsInChildren<Collider>();
        }
        #endif
        #endregion

        #region Controller
        private IMovable3DColliderController colliderController         = DefaultMovable3DController.Instance;
        private IMovable3DVelocityController velocityController         = DefaultMovable3DController.Instance;
        private IMovable3DUpdateController updateController             = DefaultMovable3DController.Instance;
        private IMovable3DComputationController computationController   = DefaultMovable3DController.Instance;
        private IMovable3DCollisionController collisionController       = DefaultMovable3DController.Instance;
        private IMovable3DTriggerController triggerController           = DefaultMovable3DController.Instance;

        // -----------------------

        /// <summary>
        /// Registers a controller for this object.
        /// </summary>
        /// <typeparam name="T">Object type to register.</typeparam>
        /// <param name="_object">Controller to register.</param>
        public virtual void RegisterController<T>(T _object) {
            if (_object is IMovable3DColliderController _collider) {
                colliderController = _collider;
            }

            if (_object is IMovable3DVelocityController _velocity) {
                velocityController = _velocity;
            }

            if (_object is IMovable3DUpdateController _update) {
                updateController = _update;
            }

            if (_object is IMovable3DComputationController _computation) {
                computationController = _computation;
            }

            if (_object is IMovable3DCollisionController _collision) {
                collisionController = _collision;
            }

            if (_object is IMovable3DTriggerController _trigger) {
                triggerController = _trigger;
            }
        }

        /// <summary>
        /// Unregisters a controller from this object.
        /// </summary>
        /// <typeparam name="T">Object type to unregister.</typeparam>
        /// <param name="_object">Controller to unregister.</param>
        public virtual void UnregisterController<T>(T _object) {
            if ((_object is IMovable3DColliderController _collider) && (colliderController == _collider)) {
                colliderController = DefaultMovable3DController.Instance;
            }

            if ((_object is IMovable3DVelocityController _velocity) && (velocityController == _velocity)) {
                velocityController = DefaultMovable3DController.Instance;
            }

            if ((_object is IMovable3DUpdateController _update) && (updateController == _update)) {
                updateController = DefaultMovable3DController.Instance;
            }

            if ((_object is IMovable3DComputationController _computation) && (computationController == _computation)) {
                computationController = DefaultMovable3DController.Instance;
            }

            if ((_object is IMovable3DCollisionController _collision) && (collisionController == _collision)) {
                collisionController = DefaultMovable3DController.Instance;
            }

            if ((_object is IMovable3DTriggerController _trigger) && (triggerController == _trigger)) {
                triggerController = DefaultMovable3DController.Instance;
            }
        }
        #endregion

        #region Collider
        /// <summary>
        /// Get the default collision mask used for this object physics collisions.
        /// </summary>
        public int GetColliderMask() {
            Collider _collider = collider.Collider;
            int _mask = colliderController.GetColliderMask(_collider);

            if (_mask == -1) {
                _mask = Physics3DUtility.GetLayerCollisionMask(_collider.gameObject);
            }

            return _mask;
        }

        /// <summary>
        /// Get the default collision mask used for this object trigger collisions.
        /// </summary>
        public int GetTriggerMask() {
            Collider _collider = trigger.Collider;
            int _mask = colliderController.GetTriggerMask(_collider);

            if (_mask == -1) {
                _mask = Physics3DUtility.GetLayerCollisionMask(_collider.gameObject);
            }

            return _mask;
        }

        /// <summary>
        /// Overrides this object physics collision mask.
        /// </summary>
        /// <param name="_mask">New collision mask value.</param>
        public void SetColliderMask(int _mask) {
            collider.CollisionMask = _mask;
        }

        /// <summary>
        /// Overrides this object trigger collision mask.
        /// </summary>
        /// <param name="_mask">New collision mask value.</param>
        public void SetTriggerMask(int _mask) {
            trigger.CollisionMask = _mask;
        }
        #endregion

        #region Velocity
        private readonly EnhancedCollection<float> velocityCoefBuffer = new EnhancedCollection<float>();

        /// <summary>
        /// Total count of velocity coefficients currently applied.
        /// </summary>
        public int VelocityCoefCount {
            get { return velocityCoefBuffer.Count; }
        }

        // -----------------------

        /// <summary>
        /// Adds a relative movement velocity to this object:
        /// <para/>
        /// Velocity of the object itself, in local coordinates.
        /// <para/> In unit/second.
        /// </summary>
        public virtual void AddRelativeMovementVelocity(Vector3 _movement) {
            AddMovementVelocity(GetWorldVector(_movement));
        }

        /// <summary>
        /// Adds a movement velocity to this object:
        /// <para/> <inheritdoc cref="Velocity.Movement" path="/summary"/>
        /// </summary>
        public virtual void AddMovementVelocity(Vector3 _movement) {
            Velocity.Movement += _movement;
        }

        /// <summary>
        /// Adds an instant movement velocity to this object:
        /// <para/> <inheritdoc cref="Velocity.InstantMovement" path="/summary"/>
        /// </summary>
        public virtual void AddInstantMovementVelocity(Vector3 _movement) {
            Velocity.InstantMovement += _movement;
        }

        /// <summary>
        /// Adds a force velocity to this object:
        /// <para/> <inheritdoc cref="Velocity.Force" path="/summary"/>
        /// </summary>
        public virtual void AddForceVelocity(Vector3 _force) {
            Velocity.Force += _force;
        }

        /// <summary>
        /// Adds an instant force velocity to this object:
        /// <para/> <inheritdoc cref="Velocity.Instant" path="/summary"/>
        /// </summary>
        public virtual void AddInstantVelocity(Vector3 _velocity) {
            Velocity.Instant += _velocity;
        }

        // -------------------------------------------
        // Coefficient
        // -------------------------------------------

        /// <summary>
        /// Applies a coefficient to this object velocity.
        /// <param name="_coef">Coefficient to apply.</param>
        /// </summary>
        public void PushVelocityCoef(float _coef) {
            if (_coef == 0f) {
                this.LogWarningMessage("Trying to add a zero coefficient value (This is not allowed)");
                return;
            }

            velocityCoefBuffer.Add(_coef);
            velocityCoef *= _coef;
        }

        /// <summary>
        /// Removes a coefficient from this object velocity.
        /// </summary>
        /// <param name="_coef">Coefficient to remove.</param>
        public void PopVelocityCoef(float _coef) {
            if ((_coef == 0f) || !velocityCoefBuffer.Remove(_coef)) {
                this.LogWarningMessage($"Trying to remove an invalid coefficient value ({_coef})");
                return;
            }

            velocityCoef /= _coef;
        }

        /// <summary>
        /// Get the applied velocity coef at a given index.
        /// <para/> Use <see cref="VelocityCoefCount"/> to get the amount of currently applied coefficients.
        /// </summary>
        /// <param name="_index">Index to get the coef at.</param>
        /// <returns>The velocity coef at the given index.</returns>
        public float GetVelocityCoefAt(int _index) {
            return velocityCoefBuffer[_index];
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Makes this object move in a given direction.
        /// </summary>
        /// <param name="_direction">Direction in which to move this object.</param>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        public virtual bool Move(Vector3 _direction) {
            if (velocityController.Move(_direction)) {
                return true;
            }

            AddMovementVelocity(_direction);
            return false;
        }

        /// <summary>
        /// Completely resets this object velocity back to zero.
        /// <br/> Does not reset its coefficient.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        public virtual bool ResetVelocity() {
            if (velocityController.OnResetVelocity()) {
                return true;
            }

            Velocity.Reset();
            return false;
        }

        /// <summary>
        /// Resets this object velocity coefficient back to 1, and clear its buffer.
        /// </summary>
        public void ResetVelocityCoef() {
            velocityCoefBuffer.Clear();
            velocityCoef = 1f;
        }
        #endregion

        #region Update
        /// <summary>
        /// Pre-update callback.
        /// </summary>
        protected virtual void OnPreUpdate() {
            updateController.OnPreUpdate();
        }

        void IMovableUpdate.Update() {
            // Pre-update callback.
            OnPreUpdate();

            // Position refresh.
            if (shouldBeRefreshed || (transform.position != lastPosition)) {
                RefreshPosition();
            }

            // Gravity.
            if (useGravity && !isGrounded) {
                ApplyGravity();
            }

            // Velocity.
            OnPreComputeVelocity();
            ComputeVelocity(out FrameVelocity _velocity);
            OnPostComputeVelocity(ref _velocity);

            // Collisions.
            CollisionData _data = CollisionType.PerformCollisions(this, Velocity, _velocity, ignoredColliders);

            if (!_data.AppliedVelocity.IsNull()) {
                SetPosition(rigidbody.position);
            }

            // Callbacks and refresh.
            OnAppliedVelocity(_velocity, _data);

            if (shouldBeRefreshed || refreshContinuously) {
                RefreshPosition();
            }

            OnRefreshedObject(_velocity, _data);

            // Update previous velocity.
            lastFrameVelocity = _velocity;

            #if DEVELOPMENT
            // Debug.
            if (debugVelocity) {
                this.LogWarning($"{name.Bold()} Velocity {UnicodeEmoji.RightTriangle.Get()} " +
                                $"M{_velocity.Movement} | F{_velocity.Force} | I{_velocity.Instant} | Final{_data.AppliedVelocity}");
            }
            #endif

            // Post-update callback.
            OnPostUpdate();
        }

        /// <summary>
        /// Post-update callback.
        /// </summary>
        protected virtual void OnPostUpdate() {
            updateController.OnPostUpdate();
        }
        #endregion

        #region Gravity
        /// <summary>
        /// Applies the gravity on this object.
        /// <para/>
        /// Override this to use a specific gravity.
        /// <br/> Use <see cref="AddGravity(float, float)"/> for a quick implementation.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool ApplyGravity() {
            if (velocityController.OnApplyGravity()) {
                return true;
            }

            AddGravity();
            return false;
        }

        /// <summary>
        /// Adds gravity as a force velocity on this object.
        /// <br/> Uses the game global gravity (see <see cref="Physics3DSettings.Gravity"/>).
        /// </summary>
        /// <param name="_gravityCoef">Coefficient applied to the gravity.</param>
        /// <param name="_maxGravityCoef">Coefficient applied to the maximum allowed gravity value.</param>
        public void AddGravity(float _gravityCoef = 1f, float _maxGravityCoef = 1f) {
            float _maxGravity = Physics3DSettings.I.MaxGravity * _maxGravityCoef;

            Quaternion _rotation = Quaternion.FromToRotation(Vector3.down, gravitySense);
            float _gravity = GetRelativeVector(Velocity.Force, _rotation).y;

            if (_gravity > _maxGravity) {
                _gravity = Mathf.Max(Physics3DSettings.I.Gravity * DeltaTime * _gravityCoef * gravityFactor, _maxGravity - _gravity);
                AddForceVelocity(gravitySense * -_gravity);
            }
        }
        #endregion

        #region Computation
        /// <summary>
        /// Called before computing this object frame velocity.
        /// <br/> Use this to perform additional operations, like incrementing the object speed.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnPreComputeVelocity() {
            return computationController.OnPreComputeVelocity();
        }

        /// <summary>
        /// Computes this object velocity just before its collision calculs.
        /// </summary>
        /// <param name="_velocity">Velocity to apply this frame.</param>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool ComputeVelocity(out FrameVelocity _velocity) {
            _velocity = new FrameVelocity();

            if (computationController.OnComputeVelocity(Velocity, ref _velocity)) {
                return true;
            }

            // Get the movement and force velocity relative to this object local space.
            // Prefere caching the transform rotation value for optimization.
            Quaternion _rotation = transform.rotation;
            Vector3 _movement = GetRelativeVector(Velocity.Movement, _rotation);
            Vector3 _force = GetRelativeVector(Velocity.Force, _rotation); 

            float _delta = DeltaTime;

            // Add instant movement.
            if ((_delta != 0f) && (speed != 0f)) {
                Vector3 _instantMovement = GetRelativeVector(Velocity.InstantMovement, _rotation);
                _movement += (_instantMovement.Flat() / (_delta * speed)).SetY(_instantMovement.y);
            }

            // If movement and force have opposite vertical velocity, accordingly reduce them.
            if (Mathm.HaveDifferentSignAndNotNull(_movement.y, _force.y)) {
                float _absMovement = Mathf.Abs(_movement.y);

                _movement.y = Mathf.MoveTowards(_movement.y, 0f, Mathf.Abs(_force.y));
                _force.y = Mathf.MoveTowards(_force.y, 0f, _absMovement);
            }

            // Compute movement and force flat velocity.
            Vector3 _flatMovement = _movement.Flat() * speed;
            Vector3 _flatForce = _force.Flat();

            _movement = Vector3.MoveTowards(_flatMovement, _flatMovement.PerpendicularSurface(_flatForce), _flatForce.magnitude * _delta).SetY(_movement.y);
            _force = Vector3.MoveTowards(_flatForce, _flatForce.PerpendicularSurface(_flatMovement), _flatMovement.magnitude * _delta).SetY(_force.y);

            // When movement is added to the opposite force direction, the resulting velocity is the addition of both.
            // But when this opposite movement is stopped, we need to resume the velocity where it previously was.
            if (equalizeVelocity) {
                Vector3 _previousMovement = GetRelativeVector(lastFrameVelocity.Movement, lastFrameVelocity.Rotation).SetY(0f);
                Vector3 _previousForce = GetRelativeVector(lastFrameVelocity.Force, lastFrameVelocity.Rotation).SetY(0f);

                if (_flatMovement.IsNull() && !_previousMovement.IsNull() && !_previousForce.IsNull()) {
                    _force = (_previousMovement + _previousForce) + (_force - _previousForce);
                }
            }

            // Get this frame velocity.
            _delta *= velocityCoef;
            _movement = GetWorldVector(_movement, _rotation);

            _velocity = new FrameVelocity() {
                Movement = _movement * _delta,
                Force = GetWorldVector(_force, _rotation) * _delta,
                Instant = Velocity.Instant,

                Rotation = _rotation,
            };

            // Reduce flat force velocity for the next frame.
            if (!_force.Flat().IsNull()) {
                float forceDeceleration = isGrounded
                                        ? Physics3DSettings.I.GroundForceDeceleration
                                        : Physics3DSettings.I.AirForceDeceleration;

                _force = Vector3.MoveTowards(_force, new Vector3(0f, _force.y, 0f), forceDeceleration * DeltaTime);
            }

            // Update velocity.
            Velocity.Force = GetWorldVector(_force, _rotation);

            return false;
        }

        /// <summary>
        /// Called after computing this object frame velocity.
        /// <br/> Use this to perform additional operations.
        /// </summary>
        /// <param name="_velocity">Velocity to apply this frame.</param>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnPostComputeVelocity(ref FrameVelocity _velocity) {
            return computationController.OnPostComputeVelocity(ref _velocity);
        }
        #endregion

        #region Collision
        private const float DynamicGravityDetectionDistance = 15f;

        // -----------------------

        /// <summary>
        /// Set this object ground state, from collision results.
        /// </summary>
        /// <param name="_isGrounded">Is the object grounded at the end of the collisions.</param>
        /// <param name="_hit">Collision ground hit (default is not grounded).</param>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        internal protected virtual bool SetGroundState(bool _isGrounded, RaycastHit _hit) {
            if (collisionController.OnSetGroundState(ref _isGrounded, _hit)) {
                return true;
            }

            // Changed ground state callback.
            if (isGrounded != _isGrounded) {
                OnGrounded(_isGrounded);
            }

            bool _isDynamicGravity = gravityMode == GravityMode.Dynamic;

            // Only update normal when grounded (hit is set to default when not).
            if (isGrounded) {
                groundNormal = _hit.normal;

                if (_isDynamicGravity) {
                    gravitySense = -_hit.normal;
                }
            }
            else if (_isDynamicGravity && collider.Cast(gravitySense, out _hit, DynamicGravityDetectionDistance, QueryTriggerInteraction.Ignore, ignoredColliders)
                  && Physics3DUtility.IsGroundSurface(_hit, -gravitySense)) {

                // When using dynamic gravity, detect nearest ground and use it as reference surface.
                groundNormal = _hit.normal;
                gravitySense = -_hit.normal;
            }

            return false;
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        /// <summary>
        /// Called after velocity is applied on this object, but before extracting the object from overlapping collider(s).
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnAppliedVelocity(FrameVelocity _velocity, CollisionData _data) {
            if (collisionController.OnAppliedVelocity(ref _velocity, _data)) {
                return true;
            }

            // Rotates according to the ground normal.
            Vector3 _up = isGrounded ? GroundNormal : -gravitySense;

            Quaternion _from = transform.rotation;
            Quaternion _to = Quaternion.LookRotation(transform.forward, Vector3.Lerp(Vector3.up, _up, groundOrientationFactor));

            if (_from != _to) {
                _to = Quaternion.RotateTowards(_from, _to, groundOrientationSpeed * DeltaTime * 90f);
                SetRotation(_to);
            }

            return false;
        }

        /// <summary>
        /// Called at the end of the update, after all velocity calculs and overlap operations have been performed.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnRefreshedObject(FrameVelocity _velocity, CollisionData _data) {
            return collisionController.OnRefreshedObject(ref _velocity, _data);
        }

        /// <summary>
        /// Called when this object extracts from a collider.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnExtractFromCollider(Collider _collider, Vector3 _direction, float _distance) {
            if (collisionController.OnExtractFromCollider(_collider, _direction, _distance)) {
                return true;
            }

            Vector3 _position = rigidbody.position + (_direction * _distance);
            SetPosition(_position);

            return false;
        }

        /// <summary>
        /// Called when this object ground state is changed.
        /// </summary>
        /// <returns><inheritdoc cref="Doc" path="/returns"/></returns>
        protected virtual bool OnGrounded(bool _isGrounded) {
            if (collisionController.OnGrounded(_isGrounded)) {
                return true;
            }

            isGrounded = _isGrounded;

            // Dampens force velocity when hitting ground.
            if (_isGrounded && !Velocity.Force.IsNull()) {
                Quaternion _rotation = transform.rotation;

                Vector3 _force = GetRelativeVector(Velocity.Force, _rotation).SetY(0f);
                _force *= Physics3DSettings.I.OnGroundedForceMultiplier;

                Velocity.Force = GetWorldVector(_force, _rotation);
            }

            return false;
        }
        #endregion

        #region Trigger
        private static readonly List<ITrigger> getTriggerComponentBuffer    = new List<ITrigger>();
        private static readonly List<ITrigger> triggerBuffer                = new List<ITrigger>();

        // All triggers currently overlapping with this object.
        protected readonly EnhancedCollection<ITrigger> overlappingTriggers = new EnhancedCollection<ITrigger>();

        /// <inheritdoc cref="ITriggerActor.Behaviour"/>
        EnhancedBehaviour ITriggerActor.Behaviour {
            get { return this; }
        }

        // -----------------------

        /// <summary>
        /// Refreshes this object trigger interaction.
        /// </summary>
        protected void RefreshTriggers() {

            // Overlapping triggers.
            int _amount = TriggerOverlap();
            triggerBuffer.Clear();

            for (int i = 0; i < _amount; i++) {
                Collider _overlap = GetOverlapTriggerAt(i);

                if (!_overlap.isTrigger) {
                    continue;
                }

                getTriggerComponentBuffer.Clear();

                // If there is a LevelTrigger, ignore any other trigger.
                if (_overlap.TryGetComponent(out LevelTrigger _levelTrigger) && _levelTrigger.isActiveAndEnabled) {

                    getTriggerComponentBuffer.Add(_levelTrigger);

                } else {

                    _overlap.GetComponents(getTriggerComponentBuffer);
                }

                // Activation.
                for (int j = 0; j < getTriggerComponentBuffer.Count; j++) {

                    ITrigger _trigger = getTriggerComponentBuffer[j];
                    if ((_trigger is Behaviour _behaviour) && !_behaviour.isActiveAndEnabled) {
                        continue;
                    }

                    triggerBuffer.Add(_trigger);

                    // Trigger enter.
                    if (HasEnteredTrigger(_trigger)) {

                        overlappingTriggers.Add(_trigger);
                        OnEnterTrigger(_trigger);
                    }
                }
            }

            // Exits from no more detected triggers.
            for (int i = overlappingTriggers.Count; i-- > 0;) {
                ITrigger _trigger = overlappingTriggers[i];

                if (HasExitedTrigger(_trigger)) {

                    overlappingTriggers.RemoveAt(i);
                    OnExitTrigger(_trigger);
                }
            }

            // ----- Local Methods ----- \\

            bool HasEnteredTrigger(ITrigger _trigger) {

                for (int i = 0; i < overlappingTriggers.Count; i++) {
                    ITrigger _other = overlappingTriggers[i];

                    if (_trigger == _other) {
                        return false;
                    }
                }

                return true;
            }

            bool HasExitedTrigger(ITrigger _trigger) {

                for (int i = 0; i < triggerBuffer.Count; i++) {
                    ITrigger _other = triggerBuffer[i];

                    if (_trigger == _other) {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Exits from all overlapping triggers.
        /// </summary>
        protected void ExitTriggers() {

            for (int i = 0; i < overlappingTriggers.Count; i++) {

                ITrigger _trigger = overlappingTriggers[i];
                OnExitTrigger(_trigger);
            }

            overlappingTriggers.Clear();
        }

        /// <summary>
        /// Called when this object enters in a trigger.
        /// </summary>
        /// <param name="_trigger">Entering <see cref="ITrigger"/>.</param>
        protected virtual void OnEnterTrigger(ITrigger _trigger) {

            _trigger.OnEnterTrigger(this);
            triggerController.OnEnterTrigger(_trigger);
        }

        /// <summary>
        /// Called when this object exits from a trigger.
        /// </summary>
        /// <param name="_trigger">Exiting <see cref="ITrigger"/>.</param>
        protected virtual void OnExitTrigger(ITrigger _trigger) {

            _trigger.OnExitTrigger(this);
            triggerController.OnExitTrigger(_trigger);
        }

        // -------------------------------------------
        // Triggers
        // -------------------------------------------

        /// <inheritdoc cref="ITriggerActor.ExitTrigger(ITrigger)"/>
        void ITriggerActor.ExitTrigger(ITrigger _trigger) {

            // Remove from list.
            int _index = overlappingTriggers.IndexOf(_trigger);
            if (_index != -1) {
                overlappingTriggers.RemoveAt(_index);
            }

            OnExitTrigger(_trigger);
        }
        #endregion

        #region Refresh
        /// <summary>
        /// Refresh this object position, and extract it from any overlapping collider.
        /// </summary>
        public void RefreshPosition() {

            // Solid colliders.
            int _amount = ColliderOverlap();

            for (int i = 0; i < _amount; i++) {
                Collider _overlap = GetOverlapColliderAt(i);

                if (Physics.ComputePenetration(collider.Collider, rigidbody.position, rigidbody.rotation,
                                               _overlap, _overlap.transform.position, _overlap.transform.rotation,
                                               out Vector3 _direction, out float _distance)) {
                    // Collider extraction.
                    OnExtractFromCollider(_overlap, _direction, _distance);
                }
            }

            RefreshTriggers();

            shouldBeRefreshed = false;
            lastPosition = transform.position;
        }

        // -------------------------------------------
        // Overlap
        // -------------------------------------------

        /// <summary>
        /// Performs an overlap operation using this object physics collider.
        /// </summary>
        /// <returns>Total count of overlapping colliders.</returns>
        public int ColliderOverlap() {
            return collider.Overlap(ignoredColliders, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Performs an overlap operation using this object trigger.
        /// </summary>
        /// <returns>Total count of overlapping triggers.</returns>
        public int TriggerOverlap() {
            return trigger.Overlap(ignoredColliders, QueryTriggerInteraction.Collide);
        }

        /// <summary>
        /// Get the overlapping collider at a given index.
        /// <para/>
        /// Use <see cref="ColliderOverlap"/> to get the count of overlapping objects.
        /// </summary>
        /// <param name="_index">Index to get the collider at.</param>
        /// <returns>Overlapping collider at the given index.</returns>
        public Collider GetOverlapColliderAt(int _index) {
            return PhysicsCollider3D.GetOverlapCollider(_index);
        }

        /// <summary>
        /// Get the overlapping trigger at a given index.
        /// <para/>
        /// Use <see cref="TriggerOverlap"/> to get the count of overlapping objects.
        /// </summary>
        /// <param name="_index">Index to get the trigger at.</param>
        /// <returns>Overlapping trigger at the given index.</returns>
        public Collider GetOverlapTriggerAt(int _index) {
            return PhysicsCollider3D.GetOverlapCollider(_index);
        }
        #endregion

        #region Transform
        /// <summary>
        /// Sets this object position.
        /// <br/> Use this instead of setting <see cref="Transform.position"/>.
        /// </summary>
        public virtual void SetPosition(Vector3 _position) {
            rigidbody.position = _position;
            transform.position = _position;

            shouldBeRefreshed = true;
        }

        /// <summary>
        /// Sets this object rotation.
        /// <br/> Use this instead of setting <see cref="Transform.rotation"/>.
        /// </summary>
        public virtual void SetRotation(Quaternion _rotation) {
            rigidbody.rotation = _rotation;
            transform.rotation = _rotation;

            shouldBeRefreshed = true;
        }

        /// <summary>
        /// Sets this object position and rotation.
        /// <br/> Use this instead of setting <see cref="Transform.position"/> and <see cref="Transform.rotation"/>.
        /// </summary>
        public void SetPositionAndRotation(Vector3 _position, Quaternion _rotation) {
            SetPosition(_position);
            SetRotation(_rotation);
        }

        /// <inheritdoc cref="SetPositionAndRotation(Vector3, Quaternion)"/>
        public void SetPositionAndRotation(Transform _transform, bool _useLocal = false) {
            Vector3 _position;
            Quaternion _rotation;

            if (_useLocal) {
                _position = _transform.localPosition;
                _rotation = _transform.localRotation;
            } else {
                _position = _transform.position;
                _rotation = _transform.rotation;
            }

            SetPositionAndRotation(_position, _rotation);
        }

        // -----------------------

        /// <summary>
        /// Adds an offset to this object position.
        /// </summary>
        /// <param name="_offset">Transform position offset.</param>
        public void OffsetPosition(Vector3 _offset) {
            SetPosition(transform.position + _offset);
        }

        /// <summary>
        /// Adds an offset to this object rotation.
        /// </summary>
        /// <param name="_offset">Transform rotation offset.</param>
        public void OffsetRotation(Quaternion _offset) {
            SetRotation(transform.rotation * _offset);
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        [Button(ActivationMode.Play, SuperColor.Raspberry, IsDrawnOnTop = false)]
        #pragma warning disable IDE0051
        private void SetTransformValues(Transform transform, bool usePosition = true, bool useRotation = true) {
            if (usePosition) {
                SetPosition(transform.position);
            }

            if (useRotation) {
                SetRotation(transform.rotation);
            }
        }
        #endregion        

        #region Utility
        /// <summary>
        /// Enables/Disables this object colliders.
        /// </summary>
        /// <param name="_enabled">Whether to enable or disable colliders.</param>
        public void EnableColliders(bool _enabled) {
            collider.Collider.enabled = _enabled;
            trigger.Collider.enabled = _enabled;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// Documentation only method.
        /// </summary>
        /// <returns>True to override this behaviour from the controller, false to call the base definition.</returns>
        protected bool Doc() { return false; }
        #endregion
    }
}
