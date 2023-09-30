// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedFramework.Core;
using System;
using UnityEngine;

namespace EnhancedFramework.Physics3D {
    #region Controllers
    // -------------------------------------------
    // Movable
    // -------------------------------------------

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> colliders.
    /// </summary>
    public interface IMovable3DColliderController {
        /// <inheritdoc cref="Movable3D.GetColliderMask"/>
        /// <returns>-1 to use the movable default collision mask implementation, otherwise the collision mask to be used.</returns>
        int GetColliderMask(Collider _collider);

        /// <inheritdoc cref="Movable3D.GetTriggerMask"/>
        /// <returns><inheritdoc cref="GetColliderMask(Collider)" path="/returns"/></returns>
        int GetTriggerMask(Collider _trigger);
    }

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> velocity.
    /// </summary>
    public interface IMovable3DVelocityController {
        /// <inheritdoc cref="Movable3D.Move(Vector3)"/>
        bool Move(Vector3 _direction);

        /// <inheritdoc cref="Movable3D.ResetVelocity"/>
        bool OnResetVelocity();

        /// <inheritdoc cref="Movable3D.ApplyGravity"/>
        bool OnApplyGravity();
    }

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> update.
    /// </summary>
    public interface IMovable3DUpdateController {
        /// <inheritdoc cref="Movable3D.OnPreUpdate"/>
        void OnPreUpdate();

        /// <inheritdoc cref="Movable3D.OnPostUpdate"/>
        void OnPostUpdate();
    }

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> computations.
    /// </summary>
    public interface IMovable3DComputationController {
        /// <inheritdoc cref="Movable3D.OnPreComputeVelocity"/>
        bool OnPreComputeVelocity();

        /// <param name="_velocity">Actual velocity of the object</param>
        /// <param name="_frameVelocity"><inheritdoc cref="Movable3D.ComputeVelocity()" path="/returns"/></param>
        /// <inheritdoc cref="Movable3D.ComputeVelocity"/>
        bool OnComputeVelocity(Velocity _velocity, ref FrameVelocity _frameVelocity);

        /// <inheritdoc cref="Movable3D.OnPostComputeVelocity"/>
        bool OnPostComputeVelocity(ref FrameVelocity _frameVelocity);
    }

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> collisions.
    /// </summary>
    public interface IMovable3DCollisionController {
        /// <inheritdoc cref="Movable3D.CollisionType"/>
        CollisionSystem3DType CollisionType { get; }

        /// <inheritdoc cref="Movable3D.SetGroundState(bool, RaycastHit)"/>
        bool OnSetGroundState(ref bool _isGrounded, RaycastHit _hit);

        /// <inheritdoc cref="Movable3D.OnAppliedVelocity(FrameVelocity, CollisionData)"/>
        bool OnAppliedVelocity(ref FrameVelocity _velocity, CollisionData _data);

        /// <inheritdoc cref="Movable3D.OnRefreshedObject(FrameVelocity, CollisionData)"/>
        bool OnRefreshedObject(ref FrameVelocity _velocity, CollisionData _data);

        /// <inheritdoc cref="Movable3D.OnGrounded(bool)"/>
        bool OnGrounded(bool _isGrounded);

        /// <inheritdoc cref="Movable3D.OnExtractFromCollider(Collider, Vector3, float)"/>
        bool OnExtractFromCollider(Collider _collider, Vector3 _direction, float _distance);
    }

    /// <summary>
    /// Controller for a <see cref="Movable3D"/> trigger.
    /// </summary>
    public interface IMovable3DTriggerController {
        /// <inheritdoc cref="Movable3D.OnEnterTrigger(ITrigger)"/>
        void OnEnterTrigger(ITrigger _trigger);

        /// <inheritdoc cref="Movable3D.OnExitTrigger(ITrigger)"/>
        void OnExitTrigger(ITrigger _trigger);
    }

    // -------------------------------------------
    // Creature
    // -------------------------------------------

    /// <summary>
    /// Controller for a <see cref="CreatureMovable3D"/> speed.
    /// </summary>
    public interface ICreatureMovable3DSpeedController {
        /// <inheritdoc cref="CreatureMovable3D.UpdateSpeed"/>
        bool OnUpdateSpeed();

        /// <inheritdoc cref="CreatureMovable3D.IncreaseSpeed"/>
        bool OnIncreaseSpeed();

        /// <inheritdoc cref="CreatureMovable3D.DecreaseSpeed()"/>
        bool OnDecreaseSpeed();

        /// <inheritdoc cref="CreatureMovable3D.ResetSpeed"/>
        bool OnResetSpeed();
    }

    /// <summary>
    /// Controller for a <see cref="CreatureMovable3D"/> rotation.
    /// </summary>
    public interface ICreatureMovable3DRotationController {
        /// <inheritdoc cref="CreatureMovable3D.Turn(float)"/>
        bool OnTurn(ref float _angleIncrement);

        /// <inheritdoc cref="CreatureMovable3D.TurnTo(Vector3, Action)"/>
        bool OnTurnTo(Vector3 _forward, Action _onComplete);

        /// <inheritdoc cref="CreatureMovable3D.StopTurnTo(bool)"/>
        void OnCompleteTurnTo(bool _reset);
    }

    /// Controller for a <see cref="CreatureMovable3D"/> navigation path callbacks.
    /// </summary>
    public interface ICreatureMovable3DNavigationController {
        /// <inheritdoc cref="CreatureMovable3D.DoCompleteNavigation()"/>
        (bool _override, bool _completed) CompletePath();

        /// <inheritdoc cref="CreatureMovable3D.SetNavigationPath(PathHandler)"/>
        void OnNavigateTo(PathHandler _path);

        /// <inheritdoc cref="CreatureMovable3D.OnCompleteNavigation(bool)"/>
        void OnCompleteNavigation(bool _success);
    }
    #endregion

    /// <summary>
    /// Default controller used when no other controller is specified.
    /// </summary>
    internal class DefaultMovable3DController : IMovable3DColliderController, IMovable3DVelocityController, IMovable3DUpdateController,
                                                IMovable3DComputationController, IMovable3DCollisionController, IMovable3DTriggerController,
                                                ICreatureMovable3DSpeedController, ICreatureMovable3DRotationController, ICreatureMovable3DNavigationController {
        #region Instance
        /// <summary>
        /// Static instance of this class.
        /// </summary>
        public static DefaultMovable3DController Instance = new DefaultMovable3DController();
        #endregion

        // ----- Movable ----- \\

        #region Velocity
        public bool Move(Vector3 _direction) {
            return false;
        }

        public bool OnApplyGravity() {
            return false;
        }

        public bool OnResetVelocity() {
            return false;
        }
        #endregion

        #region Update
        public void OnPreUpdate() { }

        public void OnPostUpdate() { }
        #endregion

        #region Computation
        public bool OnPreComputeVelocity() {
            return false;
        }

        public bool OnComputeVelocity(Velocity _velocity, ref FrameVelocity _frameVelocity) {
            return false;
        }

        public bool OnPostComputeVelocity(ref FrameVelocity _frameVelocity) {
            return false;
        }
        #endregion

        #region Collision
        public CollisionSystem3DType CollisionType {
            get { return CollisionSystem3DType.Simple; }
        }

        // -----------------------

        public bool OnAppliedVelocity(ref FrameVelocity _velocity, CollisionData _data) {
            return false;
        }

        public bool OnRefreshedObject(ref FrameVelocity _velocity, CollisionData _data) {
            return false;
        }

        public bool OnGrounded(bool _isGrounded) {
            return false;
        }

        public bool OnSetGroundState(ref bool _isGrounded, RaycastHit _hit) {
            return false;
        }

        public bool OnExtractFromCollider(Collider _collider, Vector3 _direction, float _distance) {
            return false;
        }
        #endregion

        #region Collider
        public int GetColliderMask(Collider _collider) {
            return -1;
        }

        public int GetTriggerMask(Collider _collider) {
            return -1;
        }
        #endregion

        #region Trigger
        public void OnEnterTrigger(ITrigger _trigger) { }

        public void OnExitTrigger(ITrigger _trigger) { }
        #endregion

        // ----- Creature ----- \\

        #region Speed
        public bool OnUpdateSpeed() {
            return false;
        }

        public bool OnIncreaseSpeed() {
            return false;
        }

        public bool OnDecreaseSpeed() {
            return false;
        }

        public bool OnResetSpeed() {
            return false;
        }
        #endregion

        #region Rotation
        public bool OnTurn(ref float _angleIncrement) {
            return false;
        }

        public bool OnTurnTo(Vector3 _forward, Action _onComplete) {
            return false;
        }

        public void OnCompleteTurnTo(bool _reset) { }
        #endregion

        #region Navigation
        public (bool _override, bool _completed) CompletePath() {
            return (false, false);
        }

        public void OnNavigateTo(PathHandler _path) { }

        public void OnCompleteNavigation(bool _success) { }
        #endregion
    }
}
