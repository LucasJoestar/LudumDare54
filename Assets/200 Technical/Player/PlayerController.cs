// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.GameStates;
using LudumDare54.GameStates;
using System.Collections.Generic;
using UnityEngine;

namespace LudumDare54
{
    /// <summary>
    /// Player controller (including actions and movements) singleton.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(999)]
    [AddComponentMenu(ProjectUtility.MenuPath + "Player/Player Controller"), SelectionBase, DisallowMultipleComponent]
    public class PlayerController : EnhancedSingleton<PlayerController>, IInputUpdate, IGameStateOverrideCallback {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init | UpdateRegistration.Input;

        #region Global Members
        [Section("Player Controller")]

        [SerializeField, Enhanced, Required] private PlayerControllerAttributes attributes = null;

        [Space(5f)]

        [SerializeField, Required] protected new Rigidbody2D rigidbody  = null;
        [SerializeField, Required] protected new Collider2D collider    = null;
        [SerializeField, Required] protected Collider2D trigger         = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(5f)]

        [SerializeField] private bool isPlayable = true;
        [SerializeField] private bool debugVelocity = false;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private float speed = 0f;
        [SerializeField, Enhanced, ReadOnly] private Vector2 velocity = Vector2.zero;

        [Space(10f)]

        [Tooltip("Last recorded position of this object")]
        [SerializeField, Enhanced, ReadOnly] protected Vector3 lastPosition = new Vector3();

        // -----------------------

        private Transform thisTransform = null;
        private bool shouldBeRefreshed = false;

        // -----------------------

        public override Transform Transform {
            get { return thisTransform; }
        }

        public Rigidbody2D Rigidbody {
            get { return rigidbody; }
        }

        public Collider2D Collider {
            get { return collider; }
        }

        public Collider2D Trigger {
            get { return trigger; }
        }
        #endregion

        #region Enhanced Behaviour
        public override bool AutoRegenerateID {
            get { return false; }
        }

        private GameState gameState = null;

        // -----------------------

        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Game state.
            gameState = GameState.CreateState<GameplayGameState>();

            // Registration.
            GameStateManager.Instance.RegisterOverrideCallback(this);
            attributes.Register(this);
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            thisTransform = transform;
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Game state.
            gameState.RemoveState();
            gameState = null;

            // Unregistration.
            GameStateManager.Instance.UnregisterOverrideCallback(this);
            attributes.Register(this);
        }

        // -----------------------

        protected override void OnNonSingletonInstance() {
            Instance.LogErrorMessage($"Player Controller duplicate! Destroying object \"{name}\"");
            Destroy(gameObject);
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            // Missing references.
            if (!rigidbody) {
                rigidbody = GetComponentInChildren<Rigidbody2D>();
            }

            if (!collider) {
                collider = GetComponentInChildren<Collider2D>();
            }
        }
        #endif
        #endregion

        #region Game State
        void IGameStateOverrideCallback.OnGameStateOverride(in GameStateOverride _state) {

            // Override issue management.
            if (!(_state is LudumDareGameStateOverride _override)) {
                GameStateManager.Instance.LogErrorMessage($"Override type not compatible: {_state.GetType().Name.Bold()}");
                return;
            }

            // Behaviour.
            isPlayable = _override.HasControl;
        }
        #endregion

        #region Input
        void IInputUpdate.Update() {

            // Reset state.
            if (!isPlayable) {

                velocity = Vector2.zero;
                ResetSpeed();

                return;
            }

            // Position refresh.
            if (shouldBeRefreshed || (transform.position != lastPosition)) {
                RefreshPosition();
            }

            // Movement.
            PlayerInputs _inputs = InputSettings.I.PlayerInputs;
            velocity = new Vector2(_inputs.HorizontalAxis.GetAxis(), _inputs.ForwardAxis.GetAxis());

            #if DEVELOPMENT
            // Debug.
            if (debugVelocity) {
                this.LogWarning($"{name.Bold()} Velocity {UnicodeEmoji.RightTriangle.Get()} {velocity.ToStringX(3)}");
            }
            #endif

            UpdateSpeed();
            Move(velocity);
        }
        #endregion

        #region Speed
        /// <summary>
        /// Updates this object speed, for this frame (increase or decrease).
        /// </summary>
        protected virtual bool UpdateSpeed() {

            // Update the speed depending on this frame movement.
            if (velocity.IsNull()) {
                DecreaseSpeed();
            } else {
                IncreaseSpeed();
            }

            return false;
        }

        /// <summary>
        /// Increases this object speed.
        /// </summary>
        public virtual bool IncreaseSpeed() {

            speed = attributes.MoveSpeed.EvaluateContinue(InstanceID, DeltaTime);
            return false;
        }

        /// <summary>
        /// Decreases this object speed.
        /// </summary>
        public virtual bool DecreaseSpeed() {

            speed = attributes.MoveSpeed.Decrease(InstanceID, DeltaTime);
            return false;
        }

        /// <summary>
        /// Resets this object speed.
        /// </summary>
        public virtual bool ResetSpeed() {

            speed = attributes.MoveSpeed.Reset(InstanceID);
            return false;
        }

        // -----------------------

        /// <summary>
        /// Get this object speed ratio.
        /// </summary>
        public float GetSpeedRatio() {
            return attributes.MoveSpeed.GetTimeRatio(InstanceID);
        }

        /// <summary>
        /// Set this object speed ratio.
        /// </summary>
        public virtual void SetSpeedRatio(float _ratio) {
            speed = attributes.MoveSpeed.EvaluatePercent(InstanceID, _ratio);
        }
        #endregion

        #region Velocity
        public void Move(Vector2 _direction) {

            ApplyVelocity(_direction.normalized * speed * DeltaTime);
        }

        public void ApplyVelocity(Vector2 _velocity) {

            // Move.
            Vector2 _appliedVelocity = PerformCollisions(_velocity);
            if (!_appliedVelocity.IsNull()) {
                SetPosition(rigidbody.position);
            }

            // Refresh.
            RefreshPosition();
        }
        #endregion

        #region Collision Calculs
        private const int   CollisionSystemRecursivityCount = 3;

        private static readonly List<RaycastHit2D> hitBuffer = new List<RaycastHit2D>();
        private static readonly RaycastHit2D[] castBuffer    = new RaycastHit2D[16];

        // -----------------------

        /// <summary>
        /// Move rigidbody according to a complex collision system.
        /// When hitting something, continue movement all along hit surface.
        /// Perform movement according to ground surface angle.
        /// </summary>
        private Vector2 PerformCollisions(Vector2 _velocity) {

            Rigidbody2D _rigidbody = Rigidbody;
            Vector2 _position = _rigidbody.position;

            // Calculate the remaining velocity collisions, using recursivity.
            PerformCollisionsRecursively(_rigidbody, collider, _velocity, CollisionSystemRecursivityCount);

            velocity = Vector2.zero;
            return _rigidbody.position - _position;
        }

        /// <summary>
        /// Calculates complex collisions recursively.
        /// </summary>
        private void PerformCollisionsRecursively(Rigidbody2D _rigidbody, Collider2D _collider, Vector2 _velocity, int _recursivity = 0) {

            // Velocity cast.
            int _amount = CastCollider(_velocity, castBuffer, out float _distance);

            if (_amount == 0) {
                MoveObject(_rigidbody, _velocity);
                return;
            }

            RaycastHit2D _hit = castBuffer[0];

            // Zero distance means that the object is stuck into something.
            if (_distance == 0f) {
                ComputeCast(ref _velocity, _hit);
                return;
            }

            // Move the object and get the remaining velocity, after displacement.
            _velocity = MoveObject(_rigidbody, _velocity, _distance);
            ComputeCast(ref _velocity, castBuffer, _amount);

            // Recursivity limit.
            if (--_recursivity == 0) {
                return;
            }

            // Compute main collision.
            if (!_velocity.IsNull()) {
                PerformCollisionsRecursively(_rigidbody, _collider, _velocity, _recursivity);
            } else {
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        protected virtual Vector3 MoveObject(Rigidbody2D _rigidbody, Vector2 _velocity, float _distance) {

            // To not stuck the object into another collider, be sure the compute contact offset.
            if ((_distance -= Physics2D.defaultContactOffset) > 0f) {

                Vector2 _move = _velocity.normalized * _distance;

                MoveObject(_rigidbody, _move);
                _velocity -= _move;
            }

            return _velocity;
        }

        protected virtual void MoveObject(Rigidbody2D _rigidbody, Vector2 _velocity) {
            _rigidbody.position += _velocity;
        }

        /// <summary>
        /// Inserts a RaycastHit information into the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected void ComputeCast(ref Vector2 _velocity, RaycastHit2D _hit) {
            hitBuffer.Add(_hit);
            _velocity = _velocity.PerpendicularSurface(_hit.normal);
        }

        /// <summary>
        /// Inserts an array of RaycastHit informations into the <see cref="castBuffer"/> buffer.
        /// </summary>
        protected void ComputeCast(ref Vector2 _velocity, RaycastHit2D[] _hits, int _amount) {
            for (int i = 0; i < _amount; i++) {
                ComputeCast(ref _velocity, _hits[i]);
            }
        }
        #endregion

        #region Cast
        private const float castMaxDistanceDetection = .001f;
        private static readonly RaycastHit2D[] singleCastBuffer = new RaycastHit2D[1];

        // -----------------------

        /// <summary>
        /// Performs a cast in a specific direction.
        /// </summary>
        public bool CastCollider(Vector2 _velocity, out RaycastHit2D _hit) {

            bool _result = collider.Cast(_velocity, contactFilter, singleCastBuffer, _velocity.magnitude) > 0;
            _hit = singleCastBuffer[0];

            return _result;
        }

        /// <summary>
        /// Performs a cast in a specific direction.
        /// </summary>
        public int CastCollider(Vector2 _velocity, RaycastHit2D[] _hitBuffer, out float _distance) {

            _distance = _velocity.magnitude;

            int _amount = collider.Cast(_velocity, contactFilter, _hitBuffer, _distance);
            if (_amount != 0) {

                // Hits are already sorted by distance, so simply get closest one.
                _distance = Mathf.Max(0f, _hitBuffer[0].distance - Physics2D.defaultContactOffset);

                // Retains only closest hits by ignoring those too far.
                for (int i = 1; i < _amount; i++) {
                    if ((_hitBuffer[i].distance + castMaxDistanceDetection) > _hitBuffer[0].distance)
                        return i;
                }
            }

            return _amount;
        }
        #endregion

        #region Overlap
        private static readonly List<Collider2D> collisionBuffer = new List<Collider2D>();
        private static ContactFilter2D contactFilter = new ContactFilter2D();

        // -----------------------

        /// <summary>
        /// Refresh this object position, and extract it from any overlapping collider.
        /// </summary>
        public void RefreshPosition() {

            // Extract from overlapping colliders.
            int _count = ColliderOverlap();
            ColliderDistance2D _distance;

            for (int i = 0; i < _count; i++) {
                _distance = collider.Distance(GetOverlapAt(i));

                if (_distance.isOverlapped) {
                    rigidbody.position += _distance.normal * _distance.distance;
                }
            }

            // Refresh.
            SetPosition(rigidbody.position);
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

            contactFilter.layerMask     = attributes.ColliderMask;
            contactFilter.useLayerMask  = true;
            contactFilter.useTriggers   = false;

            return collider.OverlapCollider(contactFilter, collisionBuffer);
        }

        /// <summary>
        /// Performs an overlap operation using this object trigger.
        /// </summary>
        /// <returns>Total count of overlapping triggers.</returns>
        public int TriggerOverlap() {

            contactFilter.layerMask     = attributes.TriggerMask;
            contactFilter.useLayerMask  = true;
            contactFilter.useTriggers   = true;

            return trigger.OverlapCollider(contactFilter, collisionBuffer);
        }

        /// <summary>
        /// Get the overlapping collider at a given index.
        /// <para/>
        /// Use <see cref="ColliderOverlap"/> to get the count of overlapping objects.
        /// </summary>
        /// <param name="_index">Index to get the collider at.</param>
        /// <returns>Overlapping collider at the given index.</returns>
        public Collider2D GetOverlapAt(int _index) {
            return collisionBuffer[_index];
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
            rigidbody.SetRotation(_rotation);
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
        /// Teleports the player to a given position.
        /// </summary>
        /// <param name="_position">Where to teleport the player (position and orientation.</param>
        /// <param name="_resetState">If true, resets the current player state.</param>
        /// <param name="_refreshPosition">If true, refreshes the current player position.</param>
        public void TeleportTo(Transform _position, bool _resetState = true, bool _refreshPosition = false) {

            if (_resetState) {
                ResetState();
            }

            SetPositionAndRotation(_position, false);

            if (_refreshPosition) {
                //RefreshPosition();
            }
        }

        /// <summary>
        /// Resets the current player state.
        /// </summary>
        public void ResetState() {
            ResetSpeed();
        }
        #endregion

        #region Logger
        public override Color GetLogMessageColor(LogType _type) {
            return SuperColor.Lavender.Get();
        }
        #endregion
    }
}
