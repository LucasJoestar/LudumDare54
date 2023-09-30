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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="Conversations.Conversation"/> is being closed.
    /// </summary>
    [Tooltip("Sends an Event when a Conversation is being closed")]
    [ActionCategory("Conversation")]
    public class ConversationClosedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Conversation used by the event.")]
        [RequiredField, ObjectType(typeof(Conversation))]
        public FsmObject Conversation = null;

        [Tooltip("Event to send when the Conversation is being closed.")]
        public FsmEvent ClosedEvent;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Conversation = null;
            ClosedEvent = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Conversation.Value is Conversation _conversation) {
                _conversation.OnClosed += OnClosed;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Conversation.Value is Conversation _conversation) {
                _conversation.OnClosed -= OnClosed;
            }
        }

        // -----------------------

        private void OnClosed(Conversation _conversation, ConversationPlayer _player) {
            Fsm.Event(ClosedEvent);
        }
        #endregion
    }
}
