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
    /// <see cref="FsmStateAction"/> used to send an event when a <see cref="Conversations.Conversation"/> is being played.
    /// </summary>
    [Tooltip("Sends an Event when a Conversation starts being played")]
    [ActionCategory("Conversation")]
    public class ConversationPlayedEvent : FsmStateAction {
        #region Global Members
        // -------------------------------------------
        // Variable - Event
        // -------------------------------------------

        [Tooltip("The Conversation used by the event.")]
        [RequiredField, ObjectType(typeof(Conversation))]
        public FsmObject Conversation = null;

        [Tooltip("Event to send when the Conversation starts being played.")]
        public FsmEvent PlayedEvent;
        #endregion

        #region Behaviour
        public override void Reset() {
            base.Reset();

            Conversation = null;
            PlayedEvent = null;
        }

        public override void OnEnter() {
            base.OnEnter();

            if (Conversation.Value is Conversation _conversation) {
                _conversation.OnPlayed += OnPlayed;
            }

            Finish();
        }

        public override void OnExit() {
            base.OnExit();

            if (Conversation.Value is Conversation _conversation) {
                _conversation.OnPlayed -= OnPlayed;
            }
        }

        // -----------------------

        private void OnPlayed(Conversation _conversation, ConversationPlayer _player) {
            Fsm.Event(PlayedEvent);
        }
        #endregion
    }
}
