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
    /// Base <see cref="FsmStateAction"/> used to move a <see cref="Movable3D"/> in a direction.
    /// </summary>
    public abstract class BaseMovable3DMove : BaseMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Velocity - Every Frame
        // -------------------------------------------

        [Tooltip("Direction used to move the object.")]
        [RequiredField]
        public FsmVector3 Direction;

        [Tooltip("Repeat every frame.")]
        public bool EveryFrame;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Direction = null;
            EveryFrame = false;
        }

        public override void OnEnter() {
            base.OnEnter();

            Move();

            if (!EveryFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            base.OnUpdate();

            Move();
        }

        // -----------------------

        private void Move() {
            if (GetMovable(out Movable3D _movable)) {

                _movable.Move(Direction.Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to move a <see cref="Movable3D"/> in a direction.
    /// </summary>
    [Tooltip("Moves a Movable3D in a direction.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DMove : BaseMovable3DMove {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to move.")]
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
