// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using HutongGames.PlayMaker;
using UnityEngine;

using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace EnhancedFramework.Conversations.PlayMaker {
    /// <summary>
    /// <see cref="FsmStateAction"/> used to close a <see cref="Conversations.Conversation"/>.
    /// </summary>
    [Tooltip("Closes a Conversation")]
    [ActionCategory("Conversation")]
    public class ConversationClose : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable
        // -------------------------------------------

        [Tooltip("The Conversation to close.")]
        [RequiredField, ObjectType(typeof(ConversationBehaviour))]
        public FsmObject Conversation = null;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Conversation = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Conversation.Value is ConversationBehaviour _behaviour) {
                _behaviour.Conversation.ClosePlayer();
            }

            Finish();
        }
        #endregion
    }
}
