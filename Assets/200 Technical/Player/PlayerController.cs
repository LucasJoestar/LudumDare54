// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
//  • Look at
//  • Fire (complex) / Preview
//  • Fire (simple)
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.GameStates;
using LudumDare54.GameStates;
using LudumDare54.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace LudumDare54
{
    /// <summary>
    /// Base interface for any <see cref="PlayerController"/> trigger.
    /// </summary>
    public interface IPlayerTrigger {
        #region Content
        void OnPlayerTriggerEnter(PlayerController player);
        void OnPlayerTriggerExit(PlayerController player);
        #endregion
    }

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
        [SerializeField, Enhanced, Required] private Transform graph = null;
        [SerializeField, Enhanced, Required] private Transform spawn = null;

        [Space(5f)]

        [SerializeField, Required] protected new Rigidbody2D rigidbody  = null;
        [SerializeField, Required] protected new Collider2D collider    = null;
        [SerializeField, Required] protected Collider2D trigger         = null;

        [Space(5f)]

        [SerializeField, Required] protected Animator animator = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(5f)]

        [SerializeField] private bool isPlayable    = true;
        [SerializeField] private bool debugVelocity = false;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private bool isFacingRight = true;
        [SerializeField, Enhanced, ReadOnly] private bool isGrounded    = true;
        [SerializeField, Enhanced, ReadOnly] private bool isLoading     = false;
        [SerializeField, Enhanced, ReadOnly] private bool removeControl = false;

        [Space(5f)]

        [SerializeField, Enhanced, ReadOnly] private float speed = 0f;
        [SerializeField, Enhanced, ReadOnly] private Vector2 velocity = Vector2.zero;

        [Space(10f)]

        [Tooltip("Last recorded position of this object")]
        [SerializeField, Enhanced, ReadOnly] protected Vector3 lastPosition = new Vector3();

        // -----------------------

        private Transform thisTransform = null;
        private bool shouldBeRefreshed  = false;

        private Action fallDelegate = null;
        private bool initFacingRight = true;

        // -----------------------

        public override Transform Transform {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    return transform;
                #endif

                return thisTransform;
            }
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

        public bool HasControl {
            get { return isPlayable && isGrounded && !isLoading && !removeControl; }
        }
        #endregion

        #region Enhanced Behaviour
        public override bool AutoRegenerateID {
            get { return false; }
        }

        private GameState gameState = null;
        private bool isInit = false;

        // -----------------------

        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Game state.
            if (isInit) {
                gameState = GameState.CreateState<GameplayGameState>();
            }

            // Registration.
            GameStateManager.Instance.RegisterOverrideCallback(this);
            attributes.Register(this);
        }

        protected override void OnInit() {
            base.OnInit();

            // Initialization.
            thisTransform = transform;

            // Game state.
            gameState = GameState.CreateState<GameplayGameState>();
            fallDelegate = Fall;

            isInit = true;
            initFacingRight = isFacingRight;
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Game state.
            if (isInit) {
                gameState.RemoveState();
                gameState = null;
            }

            // Unregistration.
            GameStateManager.Instance.UnregisterOverrideCallback(this);
            attributes.Unregister(this);
        }

        // -----------------------

        protected override void OnNonSingletonInstance() {
            Instance.LogErrorMessage($"Player Controller duplicate! Destroying object \"{name}\"");
            Destroy(gameObject);
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        private void OnTriggerEnter2D(Collider2D _collision) {

            if (_collision.TryGetComponent(out IPlayerTrigger _trigger)) {
                _trigger.OnPlayerTriggerEnter(this);
            }
        }

        private void OnTriggerExit2D(Collider2D _collision) {

            if (_collision.TryGetComponent(out IPlayerTrigger _trigger)) {
                _trigger.OnPlayerTriggerExit(this);
            }
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

        // ----- Input ----- \\

        #region Input
        void IInputUpdate.Update() {

            if (isLoading)
                return;

            PlayerInputs _inputs = InputSettings.I.PlayerInputs;

            // Reload.
            if (_inputs.ResetInput.Performed()) {

                ReloadLevel();
                return;
            }

            // Reset state.
            if (!HasControl) {

                velocity = Vector2.zero;
                ResetSpeed();

                return;
            }

            // Position refresh.
            if (shouldBeRefreshed || (transform.position != lastPosition)) {
                RefreshPosition();
            }

            // Movement.
            velocity = new Vector2(_inputs.HorizontalAxis.GetAxis(), _inputs.ForwardAxis.GetAxis());

            #if DEVELOPMENT
            // Debug.
            if (debugVelocity) {
                this.LogWarning($"{name.Bold()} Velocity {UnicodeEmoji.RightTriangle.Get()} {velocity.ToStringX(3)}");
            }
            #endif

            UpdateSpeed();
            Move(velocity);
            UpdateGround();
        }
        #endregion

        // ----- Special ----- \\

        #region Animation
        private const int MovementLayer = 0;
        private const int SpecialLayer  = 1;
        private const int OverrideLayer = 2;

        private static readonly int idle_Hash = Animator.StringToHash("Idle");
        private static readonly int move_Hash = Animator.StringToHash("Moving");
        private static readonly int fire_Hash = Animator.StringToHash("Fire");
        private static readonly int fall_Hash = Animator.StringToHash("Fall");

        // -------------------------------------------
        // States
        // -------------------------------------------

        public void PlayIdle() {

            for (int i = 0; i < animator.layerCount; i++) {
                animator.Play(idle_Hash, i, 0f);
            }
        }

        public void PlayFire() {
            animator.Play(fire_Hash, SpecialLayer, 0f);
        }

        public void PlayFall() {
            animator.Play(fall_Hash, OverrideLayer, 0f);
        }

        // -------------------------------------------
        // Parameters
        // -------------------------------------------

        public void PlayMove(bool isMoving) {
            animator.SetBool(move_Hash, isMoving);
        }
        #endregion

        #region Flip
        [Button(SuperColor.HarvestGold)]
        public void Flip(bool _isFacingRight) {

            if (_isFacingRight == isFacingRight)
                return;

            Transform.Rotate(0f, 180f * _isFacingRight.Signf(), 0f);
            isFacingRight = _isFacingRight;
        }
        #endregion

        #region Ground
        private Sequence fallSequence  = null;
        private DelayHandler fallDelay = default;

        // -----------------------

        private void UpdateGround() {

            // Ignore.
            if (!isGrounded) {
                return;
            }

            bool _isGrounded = ColliderOverlap(VariousSettings.I.GroundMask) != 0;
            SetGrounded(_isGrounded);
        }

        private void SetGrounded(bool _isGrounded) {

            if (_isGrounded == isGrounded)
                return;

            if (_isGrounded) {

                CancelFall();

            } else if (!fallDelay.IsValid) {

                fallDelay = Delayer.Call(attributes.FallDelay, fallDelegate, false);
            }
        }

        // -------------------------------------------
        // Fall
        // -------------------------------------------

        private void Fall() {

            fallDelay.Cancel();

            if (!isGrounded)
                return;

            // State.
            isGrounded = false;
            PlayFall();

            // Animation.
            fallSequence = DOTween.Sequence();

            float duration = attributes.FallDuration;

            Tween _movement = Transform.DOBlendableMoveBy(new Vector3(0f, attributes.FallMovementOffset, 0f), attributes.FallMovementDuration).SetEase(attributes.FallMovementEase);
            Tween _scale    = Transform.DOScale(0f, duration).SetEase(attributes.FallScaleEase);
            Tween _rotation = graph.DOBlendableRotateBy(new Vector3(0f, 0f, attributes.FallRotation), duration, RotateMode.LocalAxisAdd).SetEase(attributes.FallRotationEase);

            fallSequence.Append(_movement);
            fallSequence.Join(_rotation);
            fallSequence.Join(_scale);

            fallSequence.AppendInterval(attributes.FallRespawnDelay)
                        .SetAutoKill(true).SetRecyclable(true).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnKill() {

                if (!Application.isPlaying)
                    return;

                fallSequence = null;
                isGrounded   = true;

                Transform.ResetLocal();
                graph.ResetLocal();

                isFacingRight = true;
                Flip(initFacingRight);

                Respawn();
            }
        }

        private void CancelFall() {

            fallDelay.Cancel();

            if (!isGrounded)
                return;

            if (fallSequence.IsActive()) {
                fallSequence.Complete(true);
            }
        }
        #endregion

        #region Control
        public void RemoveControl(bool _removeControl) {
            removeControl = _removeControl;
        }
        #endregion

        #region Reload
        private void ReloadLevel() {

            EnhancedSceneManager _sceneManager = EnhancedSceneManager.Instance;

            if (_sceneManager.LoadingState != LoadingState.Inactive)
                return;

            LudumDareLoadSceneSettings _settings = new LudumDareLoadSceneSettings(LoadingMode.Black, LoadingChronos.FreezeOnFaded);
            int _sceneCount = _sceneManager.LoadedBundleCount;

            for (int i = 0; i < _sceneCount; i++) {

                LoadSceneMode _mode = (i == 0) ? LoadSceneMode.Single : LoadSceneMode.Additive;
                _sceneManager.LoadSceneBundle(_sceneManager.GetLoadedBundleAt(i), _mode, _settings);
            }

            isLoading = _sceneCount != 0;
        }
        #endregion

        // ----- Velocity ----- \\

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
            PlayMove(false);

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

            if (!Mathm.ApproximatelyZero(_direction.x)) {
                Flip(_direction.x > 0f);
            }

            PlayMove(!_direction.IsNull());
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

        // ----- Physics ----- \\

        #region Collision
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

            contactFilter.layerMask     = attributes.ColliderMask;
            contactFilter.useLayerMask  = true;
            contactFilter.useTriggers   = false;

            bool _result = collider.Cast(_velocity, contactFilter, singleCastBuffer, _velocity.magnitude) > 0;
            _hit = singleCastBuffer[0];

            return _result;
        }

        /// <summary>
        /// Performs a cast in a specific direction.
        /// </summary>
        public int CastCollider(Vector2 _velocity, RaycastHit2D[] _hitBuffer, out float _distance) {

            contactFilter.layerMask     = attributes.ColliderMask;
            contactFilter.useLayerMask  = true;
            contactFilter.useTriggers   = false;

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


        public int ColliderOverlap() {
            return ColliderOverlap(attributes.ColliderMask);
        }

        public int TriggerOverlap() {
            return TriggerOverlap(attributes.TriggerMask);
        }

        /// <summary>
        /// Performs an overlap operation using this object physics collider.
        /// </summary>
        /// <returns>Total count of overlapping colliders.</returns>
        public int ColliderOverlap(int layer) {

            contactFilter.layerMask     = layer;
            contactFilter.useLayerMask  = true;
            contactFilter.useTriggers   = false;

            return collider.OverlapCollider(contactFilter, collisionBuffer);
        }

        /// <summary>
        /// Performs an overlap operation using this object trigger.
        /// </summary>
        /// <returns>Total count of overlapping triggers.</returns>
        public int TriggerOverlap(int layer) {

            contactFilter.layerMask     = layer;
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

        // ----- Utility ----- \\

        #region Utility
        /// <summary>
        /// Makes the player respawn.
        /// </summary>
        public void Respawn() {
            TeleportTo(spawn, true, true);
        }

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
                RefreshPosition();
            }
        }

        /// <summary>
        /// Resets the current player state.
        /// </summary>
        public void ResetState() {

            ResetSpeed();
            CancelFall();
            PlayIdle();

            isGrounded = true;
        }
        #endregion

        #region Logger
        public override Color GetLogMessageColor(LogType _type) {
            return SuperColor.Lavender.Get();
        }
        #endregion
    }
}
