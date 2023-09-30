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
    /// Base <see cref="FsmStateAction"/> used to stop a <see cref="CreatureMovable3D"/> current navigation.
    /// </summary>
    public abstract class BaseMovable3DStopNavigation : BaseCreatureMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Complete
        // -------------------------------------------

        [Tooltip("Whether to complete the navigation path or not.")]
        [RequiredField]
        public FsmBool Complete;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Complete = true;
        }

        public override void OnEnter() {
            base.OnEnter();

            StopNavigation();
            Finish();
        }

        // -----------------------

        private void StopNavigation() {
            if (GetMovable(out CreatureMovable3D _movable)) {

                // Stop mode.
                if (Complete.Value) {
                    _movable.CompleteNavigation();
                } else {
                    _movable.CancelNavigation();
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to stop a <see cref="CreatureMovable3D"/> current navigation.
    /// </summary>
    [Tooltip("Stops a Movable3D current navigation.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DStopNavigation : BaseMovable3DStopNavigation {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to stop navigation.")]
        [RequiredField, ObjectType(typeof(CreatureMovable3D))]
        public FsmObject Movable;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Movable = null;
        }

        // -----------------------

        public override bool GetMovable(out CreatureMovable3D _movable) {

            if (Movable.Value is CreatureMovable3D _temp) {

                _movable = _temp;
                return true;
            }

            _movable = null;
            return false;
        }
        #endregion
    }
}
