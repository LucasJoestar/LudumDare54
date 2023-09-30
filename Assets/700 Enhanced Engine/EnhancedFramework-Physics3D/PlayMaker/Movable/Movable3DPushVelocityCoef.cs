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
    /// Base <see cref="FsmStateAction"/> used to push a velocity coefficient on a <see cref="Movable3D"/>.
    /// </summary>
    public abstract class BaseMovable3DPushVelocityCoef : BaseMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Coefficient
        // -------------------------------------------

        [Tooltip("Velocity coefficient to push and apply.")]
        [RequiredField]
        public FsmFloat Coefficient;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Coefficient = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Push();
            Finish();
        }

        // -----------------------

        private void Push() {
            if (GetMovable(out Movable3D _movable)) {
                _movable.PushVelocityCoef(Coefficient.Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to push a velocity coefficient on a <see cref="Movable3D"/>.
    /// </summary>
    [Tooltip("Pushes and apply a velocity coefficient on a Movable3D.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DPushVelocityCoef : BaseMovable3DPushVelocityCoef {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to push a velocity coefficient on.")]
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
