// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedFramework.Core;
using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Physics3D.PlayMaker {
    /// <summary>
    /// Base <see cref="FsmStateAction"/> used to teleport a <see cref="Movable3D"/> to a specific position.
    /// </summary>
    public abstract class BaseMovable3DTeleportTo : BaseMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Position
        // -------------------------------------------

        [Tooltip("Destination position to teleport the Movable to.")]
        [RequiredField]
        public FsmOwnerDefault Position;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Position = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            Move();
            Finish();
        }

        // -----------------------

        private void Move() {
            GameObject _gameObject = Fsm.GetOwnerDefaultTarget(Position);

            if (_gameObject.IsValid() && GetMovable(out Movable3D _movable)) {
                _movable.SetPositionAndRotation(_gameObject.transform);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to teleport a <see cref="Movable3D"/> to a specific position.
    /// </summary>
    [Tooltip("Teleports a Movable3D to a specific position in space.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DTeleportTo : BaseMovable3DTeleportTo {
        #region Global Members
        // -------------------------------------------
        // Position - Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to teleport.")]
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
