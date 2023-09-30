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
using UnityEngine.AI;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// <see cref="NavigationPath3D"/>-related wrapper for a single path operation.
    /// </summary>
    public struct PathHandler : IHandler<NavigationPath3D> {
        #region Global Members
        private Handler<NavigationPath3D> handler;

        // -----------------------

        public int ID {
            get { return handler.ID; }
        }

        public bool IsValid {
            get { return GetHandle(out _); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="PathHandler(NavigationPath3D, int)"/>
        public PathHandler(NavigationPath3D _path) {
            handler = new Handler<NavigationPath3D>(_path);
        }

        /// <param name="_path"><see cref="NavigationPath3D"/> used for navigation.</param>
        /// <param name="_id">ID of the associated navigation operation.</param>
        /// <inheritdoc cref="PathHandler"/>
        public PathHandler(NavigationPath3D _path, int _id) {
            handler = new Handler<NavigationPath3D>(_path, _id);
        }
        #endregion

        #region Navigation
        /// <inheritdoc cref="NavigationPath3D.UpdatePath"/>
        public bool UpdatePath() {
            if (!GetHandle(out NavigationPath3D _path)) {
                return false;
            }

            return _path.UpdatePath();
        }

        /// <inheritdoc cref="NavigationPath3D.GetNextPosition(out Vector3)"/>
        public bool GetNextPosition(out Vector3 _position) {
            if (!GetHandle(out NavigationPath3D _path)) {

                _position = Vector3.zero;
                return false;
            }

            return _path.GetNextPosition(out _position);
        }

        /// <inheritdoc cref="NavigationPath3D.GetNextDistance(out Vector3)"/>
        public bool GetNextDistance(out Vector3 _distance) {
            if (!GetHandle(out NavigationPath3D _path)) {

                _distance = Vector3.zero;
                return false;
            }

            return _path.GetNextDistance(out _distance);
        }

        /// <inheritdoc cref="NavigationPath3D.GetNextDirection(out Vector3)"/>
        public bool GetNextDirection(out Vector3 _direction) {
            if (!GetHandle(out NavigationPath3D _path)) {

                _direction = Vector3.zero;
                return false;
            }

            return _path.GetNextDirection(out _direction);
        }
        #endregion

        #region Utility
        public bool GetHandle(out NavigationPath3D _path) {
            return handler.GetHandle(out _path) && _path.IsActive;
        }

        /// <summary>
        /// Resumes this handle associated <see cref="NavigationPath3D"/>.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Resume()"/>
        public bool Resume() {
            if (!GetHandle(out NavigationPath3D _path)) {
                return false;
            }

            return _path.Resume();
        }

        /// <summary>
        /// Pauses this handle associated <see cref="NavigationPath3D"/>.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Pause"/>
        public bool Pause() {
            if (!GetHandle(out NavigationPath3D _path)) {
                return false;
            }

            return _path.Pause();
        }

        /// <summary>
        /// Stops this handle associated <see cref="NavigationPath3D"/>.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Cancel"/>
        public bool Cancel() {
            if (!GetHandle(out NavigationPath3D _player)) {
                return false;
            }

            return _player.Cancel();
        }

        /// <summary>
        /// Stops this handle associated <see cref="NavigationPath3D"/>.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Complete"/>
        public bool Complete() {
            if (!GetHandle(out NavigationPath3D _path)) {
                return false;
            }

            return _path.Complete();
        }
        #endregion
    }

    /// <summary>
    /// <see cref="CreatureMovable3D"/>-related path wrapper class.
    /// </summary>
    [Serializable]
    public class NavigationPath3D : IHandle, IPoolableObject {
        #region State
        /// <summary>
        /// References all available states for an <see cref="NavigationPath3D"/>.
        /// </summary>
        public enum State {
            Inactive    = 0,
            Active      = 1,
            Paused      = 2,
        }
        #endregion

        #region Global Members
        private int id = 0;
        private State state = State.Inactive;
        private CreatureMovable3D movable = null;

        private readonly List<Vector3> path = new List<Vector3>();
        private int index = -1;

        private bool useFinalRotation = false;
        private Quaternion finalRotation = Quaternion.identity;

        /// <summary>
        /// Called when this path is completed.
        /// <para/>
        /// Parameters are: 
        /// <br/> • A boolean indicating whether the path was fully completed, or prematurely stopped.
        /// <br/> • The associated <see cref="CreatureMovable3D"/>.
        /// </summary>
        public Action<bool, CreatureMovable3D> OnComplete = null;

        // -----------------------

        /// <inheritdoc cref="IHandle.ID"/>
        public int ID {
            get { return id; }
        }

        /// <summary>
        /// Current state of this path.
        /// </summary>
        public State Status {
            get { return state; }
        }

        /// <summary>
        /// Indicate whether this path is currently active or not.
        /// </summary>
        public bool IsActive {
            get { return (index != -1) && (state != State.Inactive); }
        }

        /// <summary>
        /// Index of the current path destination position.
        /// </summary>
        public int Index {
            get { return index; }
        }
        #endregion

        #region Path
        /// <summary>
        /// Minimum distance from the current destination to be considered as reached, on the X and Z axises.
        /// </summary>
        public const float MinFlatDestinationDistance = .01f;

        /// <summary>
        /// Minimum distance from the current destination to be considered as reached, on the Y axis.
        /// </summary>
        public const float MinVerticalDestinationDistance = .25f;

        /// <summary>
        /// Maximum distance used to sample the nav mesh data for navigation.
        /// </summary>
        public const float MaxNavMeshDistance = 9f;

        private static readonly Vector3[] pathBuffer    = new Vector3[16];
        private static NavMeshPath navMeshPath          = null; // Instantiation is not allowed to be called on initialization.
        private static int lastID = 0;

        // -----------------------

        /// <summary>
        /// Set this navigation path destination position.
        /// </summary>
        /// <param name="_movable">Object to set this navigation path for.</param>
        /// <param name="_destination">Destination position of this path.</param>
        /// <param name="_onComplete"><inheritdoc cref="OnComplete" path="/summary"/></param>
        /// <returns><see cref="PathHandler"/> of this navigation operation.</returns>
        internal PathHandler Setup(CreatureMovable3D _movable, Vector3 _destination, Action<bool, CreatureMovable3D> _onComplete = null) {
            // Cancel previous operation.
            Cancel();

            navMeshPath.ClearCorners();
            path.Clear();

            // Calculate path.
            if (NavMesh.SamplePosition(_destination, out NavMeshHit _hit, MaxNavMeshDistance, NavMesh.AllAreas)
             && NavMesh.CalculatePath(_movable.Transform.position, _hit.position, NavMesh.AllAreas, navMeshPath)) {

                int _pathCount = navMeshPath.GetCornersNonAlloc(pathBuffer);
                for (int i = 0; i < _pathCount; i++) {
                    path.Add(pathBuffer[i]);
                }
            } else {

                path.Add(_destination);
                this.LogWarningMessage("Path calcul could not be performed");
            }

            index = 0;
            return CreateHandler(_movable, _onComplete);
        }

        /// <summary>
        /// Set all this navigation path positions and destination.
        /// </summary>
        /// <param name="_path">All positions to initialize this path with.</param>
        /// <inheritdoc cref="Setup(CreatureMovable3D, Vector3, Action{bool, CreatureMovable3D})"/>
        internal PathHandler Setup(CreatureMovable3D _movable, Vector3[] _path, Action<bool, CreatureMovable3D> _onComplete = null) {

            // Cancel previous operation.
            Cancel();

            path.Clear();
            path.AddRange(_path);

            index = (_path.Length != 0) ? 0 : -1;
            return CreateHandler(_movable, _onComplete);
        }

        /// <param name="_finalRotation">Final rotation of the object.</param>
        /// <inheritdoc cref="Setup(CreatureMovable3D, Vector3, Action{bool, CreatureMovable3D})"/>
        internal PathHandler Setup(CreatureMovable3D _movable, Vector3 _destination, Quaternion _finalRotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            PathHandler _handler = Setup(_movable, _destination, _onComplete);
            SetFinalRotation(_finalRotation);

            return _handler;
        }

        /// <param name="_finalRotation">Final rotation of the object.</param>
        /// <inheritdoc cref="Setup(CreatureMovable3D, Vector3[], Action{bool, CreatureMovable3D})"/>
        internal PathHandler Setup(CreatureMovable3D _movable, Vector3[] _path, Quaternion _finalRotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            PathHandler _handler = Setup(_movable, _path, _onComplete);
            SetFinalRotation(_finalRotation);

            return _handler;
        }

        /// <param name="_destination">Destination position and rotation of this path.</param>
        /// <param name="_useRotation">Whether to rotate the object in the transform direction on completion or not.</param>
        /// <inheritdoc cref="Setup(CreatureMovable3D, Vector3, Action{bool, CreatureMovable3D})"/>
        internal PathHandler Setup(CreatureMovable3D _movable, Transform _destination, bool _useRotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            PathHandler _handler = Setup(_movable, _destination.position, _onComplete);

            if (_useRotation) {
                SetFinalRotation(_destination.rotation);
            }

            return _handler;
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        /// <summary>
        /// Pauses this path current navigation.
        /// </summary>
        public bool Pause() {

            // Ignore if not active.
            if (state != State.Active) {
                return false;
            }

            SetState(State.Paused);
            return true;
        }

        /// <summary>
        /// Resumes this path current navigation.
        /// </summary>
        public bool Resume() {

            // Ignore if not paused.
            if (state != State.Paused) {
                return false;
            }

            SetState(State.Active);
            return true;
        }

        /// <summary>
        /// Cancels this path current navigation.
        /// </summary>
        public bool Cancel() {
            return Stop(false);
        }

        /// <summary>
        /// Completes this path current navigation.
        /// </summary>
        public bool Complete() {

            // Ignore if already inactive.
            if (state == State.Inactive) {
                return false;
            }

            // Teleport to destination.
            if (path.SafeLast(out Vector3 _position)) {

                if (useFinalRotation) {
                    movable.SetPositionAndRotation(_position, finalRotation);
                } else {
                    movable.SetPosition(_position);
                }
            }

            Stop(true);
            return true;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private PathHandler CreateHandler(CreatureMovable3D _movable, Action<bool, CreatureMovable3D> _onComplete) {
            OnComplete = _onComplete;
            movable = _movable;

            SetState(State.Active);

            id = ++lastID;
            return new PathHandler(this, id);
        }

        private void SetFinalRotation(Quaternion _rotation) {
            useFinalRotation = true;
            finalRotation = _rotation;
        }

        private bool Stop(bool _isCompleted) {

            // Ignore if already inactive.
            if (state == State.Inactive) {
                return false;
            }

            // State.
            SetState(State.Inactive);
            id = 0;

            index = -1;
            useFinalRotation = false;
            movable.OnCompleteNavigation(_isCompleted);

            // Callback.
            OnComplete?.Invoke(_isCompleted, movable);

            OnComplete = null;
            movable = null;

            NavigationPath3DManager.Release(this);
            return true;
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Update this path.
        /// </summary>
        /// <returns><inheritdoc cref="GetNextDistance(out Vector3)" path="/returns"/></returns>
        public bool UpdatePath() {
            if (!GetNextDistance(out Vector3 _distance)) {
                return false;
            }

            _distance = _distance.RotateInverse(movable.Transform.rotation);

            if ((_distance.Flat().magnitude <= MinFlatDestinationDistance) && (Mathf.Abs(_distance.y) <= MinVerticalDestinationDistance)) {

                // Path end.
                if (index == (path.Count - 1)) {

                    // Wait for completion.
                    if (!movable.DoCompleteNavigation()._completed) {
                        return true;
                    }

                    // Final rotation.
                    if (useFinalRotation) {
                        movable.TurnTo(finalRotation.ToDirection());
                    }

                    Stop(true);
                    return false;
                }

                index++;
            }

            return true;
        }

        /// <summary>
        /// Get the next target position on this path.
        /// </summary>
        /// <param name="_position">Next destination position on the path.</param>
        /// <returns>True if the path is active and a new destination was found, false otherwise.</returns>
        public bool GetNextPosition(out Vector3 _position) {
            if (!IsActive) {
                _position = Vector3.zero;
                return false;
            }

            _position = path[index];
            return true;
        }

        /// <summary>
        /// Get the distance between an object and the next desination position.
        /// </summary>
        /// <param name="_distance">Distance between the object and its next destination position, not normalized.</param>
        /// <returns>True if the path is active and a new destination was found, false otherwise.</returns>
        public bool GetNextDistance(out Vector3 _distance) {
            if (!IsActive) {
                _distance = Vector3.zero;
                return false;
            }

            // Paused state.
            if (state == State.Paused) {
                _distance = Vector3.zero;
                return true;
            }

            Transform _transform = movable.Transform;
            _distance = path[index] - _transform.position;

            return true;
        }

        /// <summary>
        /// Get the direction to the next destination position.
        /// <para/>
        /// Similar to <see cref="GetNextDistance(out Vector3)"/>,
        /// but only on the object X and Z axises.
        /// </summary>
        /// <param name="_direction">Direction to the next destination position, not normalized.</param>
        /// <returns><inheritdoc cref="GetNextDistance(out Vector3)" path="/returns"/></returns>
        public bool GetNextDirection(out Vector3 _direction) {
            if (!GetNextDistance(out _direction)) {
                return false;
            }

            // Removes the vertical component of the direction.
            Quaternion _rotation = movable.Transform.rotation;
            _direction = _direction.RotateInverse(_rotation).SetY(0f).Rotate(_rotation);

            return true;
        }
        #endregion

        #region Pool
        void IPoolableObject.OnCreated() {

            // Create nav mesh helper.
            if (navMeshPath == null) {
                navMeshPath = new NavMeshPath();
            }
        }

        void IPoolableObject.OnRemovedFromPool() { }

        void IPoolableObject.OnSentToPool() {

            // Make sure the navigation is not active.
            Cancel();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Sets the state of this object.
        /// </summary>
        /// <param name="_state">New state of this object.</param>
        private void SetState(State _state) {
            state = _state;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="NavigationPath3D"/> pool managing class.
    /// </summary>
    internal class NavigationPath3DManager : IObjectPoolManager<NavigationPath3D> {
        #region Pool
        private static readonly ObjectPool<NavigationPath3D> pool = new ObjectPool<NavigationPath3D>(1);
        public static readonly NavigationPath3DManager Instance = new NavigationPath3DManager();

        /// <inheritdoc cref="NavigationPath3DManager"/>
        private NavigationPath3DManager() {

            // Pool initialization.
            pool.Initialize(this);
        }

        // -----------------------

        /// <summary>
        /// Get a <see cref="NavigationPath3D"/> instance from the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.Get"/>
        public static NavigationPath3D Get() {
            return pool.Get();
        }

        /// <summary>
        /// Releases a specific <see cref="NavigationPath3D"/> instance and sent it back to the pool.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.Release(T)"/>
        public static bool Release(NavigationPath3D _call) {
            return pool.Release(_call);
        }

        /// <summary>
        /// Clears the <see cref="NavigationPath3D"/> pool content.
        /// </summary>
        /// <inheritdoc cref="ObjectPool{T}.Clear(int)"/>
        public static void ClearPool(int _capacity = 1) {
            pool.Clear(_capacity);
        }

        // -------------------------------------------
        // Manager
        // -------------------------------------------

        /// <inheritdoc cref="IObjectPoolManager{NavigationPath3D}.CreateInstance"/>
        NavigationPath3D IObjectPoolManager<NavigationPath3D>.CreateInstance() {
            return new NavigationPath3D();
        }

        /// <inheritdoc cref="IObjectPoolManager{NavigationPath3D}.DestroyInstance(NavigationPath3D)"/>
        void IObjectPoolManager<NavigationPath3D>.DestroyInstance(NavigationPath3D _call) {
            // Cannot destroy the instance, so simply ignore the object and wait for the garbage collector to pick it up.
        }
        #endregion
    }

    /// <summary>
    /// Advanced <see cref="Movable3D"/> with the addition of various creature-like behaviours.
    /// </summary>
    [AddComponentMenu(FrameworkUtility.MenuPath + "Physics 3D/Creature Movable 3D"), DisallowMultipleComponent]
    public class CreatureMovable3D : Movable3D {
        #region Rotation Mode
        /// <summary>
        /// Determines how the creature turn when following a path.
        /// </summary>
        public enum PathRotationMode {
            /// <summary>
            /// Don't turn the creature.
            /// </summary>
            None                = 0,

            /// <summary>
            /// Turn in direction of the new destination before moving.
            /// </summary>
            TurnBeforeMovement  = 1,

            /// <summary>
            /// Turn while moving to the next destination.
            /// </summary>
            TurnDuringMovement  = 2,
        }
        #endregion

        #region Global Members
        [Space(5f), PropertyOrder(1)]

        [SerializeField, Enhanced, Required] protected CreatureMovable3DAttributes attributes = null;

        [PropertyOrder(3)]

        [SerializeField, Enhanced, ReadOnly] protected Vector3 forward = Vector3.zero;

        // -----------------------

        public override CollisionSystem3DType CollisionType {
            get { return CollisionSystem3DType.Creature; }
        }

        public override float ClimbHeight {
            get { return attributes.ClimbHeight; }
        }

        public override float SnapHeight {
            get { return attributes.SnapHeight; }
        }

        public override bool IsSpeedEditable {
            get { return false; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            // Attributes.
            attributes.Register(this);
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            // Attributes.
            attributes.Unregister(this);
        }
        #endregion

        #region Controller
        private ICreatureMovable3DRotationController rotationController = DefaultMovable3DController.Instance;
        private ICreatureMovable3DSpeedController speedController       = DefaultMovable3DController.Instance;
        private ICreatureMovable3DNavigationController pathController         = DefaultMovable3DController.Instance;

        // -----------------------

        public override void RegisterController<T>(T _object) {
            base.RegisterController<T>(_object);

            if (_object is ICreatureMovable3DRotationController _rotation) {
                rotationController = _rotation;
            }

            if (_object is ICreatureMovable3DSpeedController _speed) {
                speedController = _speed;
            }

            if (_object is ICreatureMovable3DNavigationController _path) {
                pathController = _path;
            }
        }

        public override void UnregisterController<T>(T _object) {
            base.UnregisterController<T>(_object);

            if ((_object is ICreatureMovable3DRotationController _rotation) && (rotationController == _rotation)) {
                rotationController = DefaultMovable3DController.Instance;
            }

            if ((_object is ICreatureMovable3DSpeedController _speed) && (speedController == _speed)) {
                speedController = DefaultMovable3DController.Instance;
            }

            if ((_object is ICreatureMovable3DNavigationController _path) && (pathController == _path)) {
                pathController = DefaultMovable3DController.Instance;
            }
        }
        #endregion

        #region Velocity
        public override bool ResetVelocity() {
            if (base.ResetVelocity()) {
                return true;
            }

            ResetSpeed();
            return false;
        }

        // -------------------------------------------
        // Speed
        // -------------------------------------------

        /// <summary>
        /// Updates this object speed, for this frame (increase or decrease).
        /// </summary>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        protected virtual bool UpdateSpeed() {
            if (speedController.OnUpdateSpeed()) {
                return true;
            }

            // Update the speed depending on this frame movement.
            Vector3 _movement = GetRelativeVector(Velocity.Movement + Velocity.InstantMovement).Flat();

            if (_movement.IsNull()) {
                DecreaseSpeed();
            } else {
                IncreaseSpeed();
            }

            return false;
        }

        /// <summary>
        /// Increases this object speed.
        /// </summary>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        public virtual bool IncreaseSpeed() {
            if (speedController.OnIncreaseSpeed()) {
                return true;
            }

            float _increase = DeltaTime;
            if (!IsGrounded) {
                _increase *= attributes.AirAccelCoef;
            }

            speed = attributes.MoveSpeed.EvaluateContinue(InstanceID, _increase);
            return false;
        }

        /// <summary>
        /// Decreases this object speed.
        /// </summary>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        public virtual bool DecreaseSpeed() {
            if (speedController.OnDecreaseSpeed()) {
                return true;
            }

            speed = attributes.MoveSpeed.Decrease(InstanceID, DeltaTime);
            return false;
        }

        /// <summary>
        /// Resets this object speed.
        /// </summary>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        public virtual bool ResetSpeed() {
            if (speedController.OnResetSpeed()) {
                return true;
            }

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

        #region Navigation
        private const float PathDelay = .01f;

        private Vector3 lastPathMovement = Vector3.zero;
        private PathHandler path = default;

        // -----------------------

        /// <inheritdoc cref="NavigateTo(Vector3, Quaternion, Action{bool, CreatureMovable3D})"/>
        public PathHandler NavigateTo(Vector3 _destination, Action<bool, CreatureMovable3D> _onComplete = null) {
            return SetNavigationPath(NavigationPath3DManager.Get().Setup(this, _destination, _onComplete));
        }

        /// <inheritdoc cref="NavigateTo(Vector3[], Quaternion, Action{bool, CreatureMovable3D})"/>
        public PathHandler NavigateTo(Vector3[] _path, Action<bool, CreatureMovable3D> _onComplete = null) {
            return SetNavigationPath(NavigationPath3DManager.Get().Setup(this, _path, _onComplete));
        }

        /// <summary>
        /// Set this object path to reach a specific destination position.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Setup(CreatureMovable3D, Vector3, Quaternion, Action{bool, CreatureMovable3D})"/>
        public PathHandler NavigateTo(Vector3 _destination, Quaternion _rotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            return SetNavigationPath(NavigationPath3DManager.Get().Setup(this, _destination, _rotation, _onComplete));
        }

        /// <summary>
        /// Set this object path positions.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Setup(CreatureMovable3D, Vector3[], Quaternion, Action{bool, CreatureMovable3D})"/>
        public PathHandler NavigateTo(Vector3[] _path, Quaternion _rotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            return SetNavigationPath(NavigationPath3DManager.Get().Setup(this, _path, _rotation, _onComplete));
        }

        /// <summary>
        /// Set this object path destination position and rotation.
        /// </summary>
        /// <inheritdoc cref="NavigationPath3D.Setup(CreatureMovable3D, Transform, bool, Action{bool, CreatureMovable3D})"/>
        [Button(ActivationMode.Play, SuperColor.HarvestGold)]
        public PathHandler NavigateTo(Transform _transform, bool _useRotation, Action<bool, CreatureMovable3D> _onComplete = null) {
            return SetNavigationPath(NavigationPath3DManager.Get().Setup(this, _transform, _useRotation, _onComplete));
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get this object current navigation path.
        /// </summary>
        /// <param name="_path">Current path of the object.</param>
        /// <returns>True if this object path is active, false otherwise.</returns>
        public bool GetNavigationPath(out PathHandler _path) {
            _path = path;
            return _path.IsValid;
        }

        /// <summary>
        /// Completes this object current navigation path.
        /// </summary>
        /// <returns>True if the navigation could be successfully completed, false otherwise.</returns>
        public bool CompleteNavigation() {
            return path.Complete();
        }

        /// <summary>
        /// Cancels this object current navigation path.
        /// </summary>
        /// <returns>True if the navigation could be successfully canceled, false otherwise.</returns>
        public bool CancelNavigation() {
            return path.Cancel();
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        /// <summary>
        /// Called when this object navigation path is set.
        /// </summary>
        /// <param name="_path">This object path.</param>
        internal protected virtual PathHandler SetNavigationPath(PathHandler _path) {

            // Use a delay in case the current path is being completed during the same frame.
            if (path.IsValid) {
                Delayer.Call(PathDelay, SetPath, true);
            } else {
                SetPath();
            }

            return _path;

            // ----- Local Methods ----- \\

            void SetPath() {

                CancelNavigation();

                path = _path;
                pathController.OnNavigateTo(_path);
            }
        }

        /// <summary>
        /// Determines if the current navigation path should be completed or not.
        /// <para/>
        /// By default, waits for the object movement to be null (useful when using root motion).
        /// </summary>
        /// <returns>Override: <inheritdoc cref="Movable3D.Doc" path="/returns"/>.
        /// <para/>Completed: True if the path should be completed, false otherwise.</returns>
        internal protected virtual (bool _override, bool _completed) DoCompleteNavigation() {
            var _result = pathController.CompletePath();

            if (_result._override) {
                return _result;
            }

            return (false, lastPathMovement.IsNull());
        }

        /// <summary>
        /// Called when this object navigation path is complete.
        /// </summary>
        /// <param name="_success">True if the navigation path was successfully completed, false if canceled.</param>
        internal protected virtual void OnCompleteNavigation(bool _success) {
            pathController.OnCompleteNavigation(_success);
        }
        #endregion

        #region Orientation
        private const float MinRotationAngle = PathRotationTurnAngle - .2f;

        private Action onTurnComplete = null;

        // -----------------------

        /// <summary>
        /// Turns this object on its Y axis, at a specific angle.
        /// </summary>
        /// <param name="_angleIncrement">Local rotation angle increment.</param>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        public virtual bool Turn(float _angleIncrement, bool _withController = true) {

            if (_withController && rotationController.OnTurn(ref _angleIncrement)) {
                return true;
            }

            OffsetRotation(Quaternion.Euler(transform.up * _angleIncrement));
            return false;
        }

        /// <summary>
        /// Turns this object on its Y axis, using a given direction.
        /// </summary>
        /// <param name="_direction">Direction in which to turn the object (1 for right, -1 for left).</param>
        public void TurnTo(float _direction) {
            if (_direction == 0f) {
                ResetTurn();
                return;
            }

            float _angle = GetTurnAngle(_direction);
            Turn(_angle);
        }

        /// <summary>
        /// Turns this object on its Y axis, to a specific forward.
        /// </summary>
        /// <param name="_forward">New forward target.</param>
        /// <param name="_onTurnComplete">Delegate to call once the rotation is complete.</param>
        /// <returns><inheritdoc cref="Movable3D.Doc" path="/returns"/></returns>
        public bool TurnTo(Vector3 _forward, Action _onTurnComplete = null) {

            // Invalid operation.
            if (_forward.IsNull()) {

                StopTurnTo(true);
                _onTurnComplete?.Invoke();

                return false;
            }

            StopTurnTo(false);

            // Controller
            if (rotationController.OnTurnTo(_forward, _onTurnComplete)) {
                return true;
            }

            forward = _forward;
            onTurnComplete = _onTurnComplete;

            return false;
        }

        /// <summary>
        /// Stops the current turn to operation.
        /// </summary>
        /// <param name="_reset">If true, resets all associated parameters.</param>
        public void StopTurnTo(bool _reset = true) {
            // Controller.
            rotationController.OnCompleteTurnTo(_reset);

            forward = Vector3.zero;

            // Callback.
            onTurnComplete?.Invoke();

            // Reset.
            if (_reset) {
                ResetTurn();
                onTurnComplete = null;
            }
        }

        /// <summary>
        /// Resets this object current turn speed.
        /// </summary>
        public void ResetTurn() {
            attributes.TurnSpeed.Reset(InstanceID);
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Updates this creature rotation.
        /// </summary>
        private void UpdateRotation() {
            if (forward.IsNull()) {
                return;
            }

            float _angle = GetForwardAngle(forward);

            // Rotation achieved.
            if (Mathf.Abs(_angle) < MinRotationAngle) {

                StopTurnTo();
                return;
            }

            // Rotate.
            float _forwardAngle = _angle;
            _angle = Mathf.MoveTowards(0f, _angle, GetTurnAngle(1f));


            // Do not increment if exact angle.
            bool _withController = true;

            if (Mathf.Approximately(_angle, _forwardAngle)) {
                _withController = false;
            }

            Turn(_angle, _withController);
        }

        private float GetTurnAngle(float _coef) {
            return attributes.TurnSpeed.EvaluateContinue(InstanceID, DeltaTime) * DeltaTime * _coef * 90f;
        }

        private float GetForwardAngle(Vector3 _forward) {

            // Make sure vector up is valid.
            _forward = Vector3.ProjectOnPlane(_forward, Transform.up);

            // Get angle.
            float _angle = Vector3.SignedAngle(transform.forward, _forward, transform.up);

            if (Mathf.Abs(_angle) > 180f) {
                _angle -= 360f * Mathf.Sign(_angle);
            }

            return _angle;
        }
        #endregion

        #region Computation
        private const float PathRotationTurnAngle = 2.5f;

        private const float PathStuckMagnitudeTolerance = .1f;
        private const float PathStuckMaxDuration = 1f;

        private Vector3 pathLastDirection = Vector3.zero;
        private float pathStuckDuration = 0f;

        // -----------------------

        protected override bool OnPreComputeVelocity() {
            if (base.OnPreComputeVelocity()) {
                return true;
            }

            bool _isStuck = false;

            // Follow path.
            if (path.GetNextDirection(out Vector3 _direction)) {

                if (FollowPath(_direction.normalized)) {

                    // When following the path, check if something is preventing the object from moving.
                    float _difference = Mathf.Abs(pathLastDirection.sqrMagnitude - _direction.sqrMagnitude);

                    if (_difference < PathStuckMagnitudeTolerance) {

                        pathStuckDuration += DeltaTime;

                        // If stuck for too long, cancel path.
                        if (pathStuckDuration > PathStuckMaxDuration) {
                            CompleteNavigation();
                        } else {
                            _isStuck = true;
                        }
                    }
                }

                // ----- Local Method ----- \\

                bool FollowPath(Vector3 _movement) {
                    switch (attributes.PathRotationMode) {

                        case PathRotationMode.TurnBeforeMovement:
                            TurnTo(_direction);

                            // Don't move while not facing direction.
                            if (!Mathm.IsInRange(GetForwardAngle(_direction), -PathRotationTurnAngle, PathRotationTurnAngle)) {                                
                                return false;
                            }

                            break;

                        case PathRotationMode.TurnDuringMovement:
                            TurnTo(_direction);
                            break;

                        case PathRotationMode.None:
                        default:
                            break;
                    }

                    Move(_direction);
                    return true;
                }
            }

            // Position update.
            if (!_isStuck) {
                pathLastDirection = _direction;
                pathStuckDuration = 0f;
            }

            UpdateSpeed();
            return false;
        }

        protected override bool OnPostComputeVelocity(ref FrameVelocity _velocity) {
            if (base.OnPostComputeVelocity(ref _velocity)) {
                return true;
            }

            // Clamp path velocity magnitude.
            if (path.GetNextDirection( out Vector3 _direction)) {

                // Cache unclamped movement.
                Vector3 _movement = _velocity.Movement;
                lastPathMovement = _movement;

                // Clamping the vector magnitude does not guarantee that the movement
                // will not be oriented in the wrong direction,
                // so let's clamp its X & Z components independently.
                _movement.x = Mathf.Min(Mathf.Abs(_movement.x), Mathf.Abs(_direction.x)) * Mathf.Sign(_movement.x);
                _movement.z = Mathf.Min(Mathf.Abs(_movement.z), Mathf.Abs(_direction.z)) * Mathf.Sign(_movement.z);

                _velocity.Movement = _movement;
            }

            return false;
        }
        #endregion

        #region Collision
        protected override bool OnAppliedVelocity(FrameVelocity _velocity, CollisionData _data) {
            if (base.OnAppliedVelocity(_velocity, _data)) {
                return true;
            }

            // Path update.
            if (path.IsValid && !path.UpdatePath()) {
                path = default;
            }

            // Forward rotation.
            UpdateRotation();

            return false;
        }
        #endregion
    }
}
