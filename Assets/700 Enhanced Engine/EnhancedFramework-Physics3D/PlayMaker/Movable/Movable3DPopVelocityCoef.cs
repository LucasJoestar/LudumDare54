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
    /// Base <see cref="FsmStateAction"/> used to pop a velocity coefficient from a <see cref="Movable3D"/>.
    /// </summary>
    [Tooltip("Pops and remove a velocity coefficient from a Movable3D.")]
    [ActionCategory("Movable 3D")]
    public abstract class BaseMovable3DPopVelocityCoef : BaseMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Coefficient
        // -------------------------------------------

        [Tooltip("Velocity coefficient to pop.")]
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

            Pop();
            Finish();
        }

        // -----------------------

        private void Pop() {
            if (GetMovable(out Movable3D _movable)) {
                _movable.PopVelocityCoef(Coefficient.Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to pop a velocity coefficient from a <see cref="Movable3D"/>.
    /// </summary>
    [Tooltip("Pops and remove a velocity coefficient from a Movable3D.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DPopVelocityCoef : BaseMovable3DPopVelocityCoef {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to pop a velocity coefficient from.")]
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
