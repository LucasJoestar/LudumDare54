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
    /// Base <see cref="FsmStateAction"/> used to turn a <see cref="CreatureMovable3D"/> in a direction.
    /// </summary>
    public abstract class BaseMovable3DTurnTo : BaseCreatureMovable3DFSM {
        #region Global Members
        // -------------------------------------------
        // Direction - Events
        // -------------------------------------------

        [Tooltip("Forward direction to turn the Movable to.")]
        [RequiredField]
        public FsmOwnerDefault Forward;

        [Tooltip("Event to send when the turn operation is stopped.")]
        public FsmEvent StopEvent;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Forward = null;
            StopEvent = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            TurnTo();
            Finish();
        }

        // -----------------------

        private void TurnTo() {

            GameObject _gameObject = Fsm.GetOwnerDefaultTarget(Forward);

            if (_gameObject.IsValid() && GetMovable(out CreatureMovable3D _movable)) {
                _movable.TurnTo(_gameObject.transform.forward, OnComplete);
            } else {
                OnComplete();
            }

            // ----- Local Method ----- \\

            void OnComplete() {
                Fsm.Event(StopEvent);
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="FsmStateAction"/> used to turn a <see cref="CreatureMovable3D"/> in a direction.
    /// </summary>
    [Tooltip("Turns a Movable3D in a direction.")]
    [ActionCategory("Movable 3D")]
    public class Movable3DTurnTo : BaseMovable3DTurnTo {
        #region Global Members
        // -------------------------------------------
        // Movable
        // -------------------------------------------

        [Tooltip("The Movable instance to turn.")]
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
