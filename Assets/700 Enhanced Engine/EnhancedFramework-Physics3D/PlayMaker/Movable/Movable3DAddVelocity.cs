// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Physics3D.PlayMaker {
    /// <summary>
    /// Base <see cref="FsmStateAction"/> used to add a velocity to a <see cref="Movable3D"/>.
    /// </summary>
    public abstract class BaseMovable3DAddVelocity : BaseMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Velocity - Instant - Every Frame
        // -------------------------------------------

        [Tooltip("Velocity to add to the object.")]
        [RequiredField]
        public FsmVector3 Velocity;

        [Tooltip("If true, adds velocity for this frame only. Adds persistent velocity otherwise.")]
        [RequiredField]
        public FsmBool InstantVelocity;

        [Tooltip("Repeat every frame.")]
        public bool EveryFrame;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Velocity = null;
            InstantVelocity = false;
            EveryFrame = false;
        }

        public override void OnEnter() {
            base.OnEnter();

            AddVelocity();

            if (!EveryFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            base.OnUpdate();

            AddVelocity();
        }

        // -----------------------

        private void AddVelocity() {
            if (GetMovable(out Movable3D _movable)) {

                // Velocity mode.
                if (InstantVelocity.Value) {
                    _movable.AddInstantVelocity(Velocity.Value);
                } else {
                    _movable.AddForceVelocity(Velocity.Value);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to add a velocity to a <see cref="Movable3D"/>.
    /// </summary>
    [Tooltip("Adds a velocity to a Movable3D.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DAddVelocity : BaseMovable3DAddVelocity {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to add velocity to.")]
        [RequiredField, ObjectType(typeof(Movable3D))]
        public FsmObject Movable;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Movable = null;
        }

        // -----------------------

        public override bool GetMovable(out Movable3D _movable) {

            if (Movable.Value is Movable3D _temp) {

                _movable = _temp;
                return true;
            }

            _movable = null;
            return false;
        }
        #endregion
    }
}
