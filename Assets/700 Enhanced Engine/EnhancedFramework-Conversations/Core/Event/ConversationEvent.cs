// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// Base class to dervie any <see cref="Conversation"/> event from.
    /// <br/> Designed to be played and stopped according to any node.
    /// </summary>
    [Serializable]
    public abstract class ConversationEvent {
        #region Global Members
        /// <summary>
        /// Whether this event can be played or not.
        /// </summary>
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// Indicates if this event is currently playing.
        /// </summary>
        public abstract bool IsPlaying { get; }
        #endregion

        #region Behaviour
        /// <summary>
        /// Plays all given events.
        /// </summary>
        /// <param name="_player"><see cref="ConversationPlayer"/> of the conversation being played.</param>
        /// <param name="_events">Events to play.</param>
        public static void Play(ConversationPlayer _player, params ConversationEvent[] _events) {
            foreach (var _event in _events) {
                _event.Play(_player);
            }
        }

        /// <summary>
        /// Stops from playing all given events.
        /// </summary>
        /// <param name="_player"><see cref="ConversationPlayer"/> of the conversation being played.</param>
        /// <param name="_isClosingConversation">Indicates if the associated conversation is being closed or not.</param>
        /// <param name="_onComplete">Delegate to call once all events are stopped.</param>
        /// <param name="_events">Events to stop.</param>
        public static void Stop(ConversationPlayer _player, bool _isClosingConversation, Action _onComplete, params ConversationEvent[] _events) {
            foreach (var _event in _events) {
                _event.Stop(_player, _isClosingConversation);
            }

            CompleteQuit(_onComplete);
        }

        // -------------------------------------------
        // Internal
        // -------------------------------------------

        /// <summary>
        /// Plays this event.
        /// </summary>
        /// <param name="_player"><see cref="ConversationPlayer"/> of the conversation being played.</param>
        /// <returns>True if this event could be successfully played, false otherwise.</returns>
        internal bool Play(ConversationPlayer _player) {
            if (IsAvailable) {
                return OnPlay(_player);
            }

            return false;
        }

        /// <summary>
        /// Stops from playing this event.
        /// </summary>
        /// <param name="_player"><see cref="ConversationPlayer"/> of the conversation being played.</param>
        /// <param name="_isClosingConversation">Indicates if the associated conversation is being closed or not.</param>
        internal void Stop(ConversationPlayer _player, bool _isClosingConversation) {
            if (IsAvailable) {

                RegisterPendingEvent(this);

                if (OnStop(_player, _isClosingConversation, Unregister)) {
                    Unregister();
                }

                // ----- Local Method ----- \\

                void Unregister() {
                    UnregisterPendingEvent(this);
                }
            }
        }

        // -----------------------

        /// <inheritdoc cref="Play(ConversationPlayer)"/>
        protected abstract bool OnPlay(ConversationPlayer _player);

        /// <param name="_onComplete">If returned false, delegate to call once this event is stopped.</param>
        /// <returns>True if this event was successfully stopped, false if it requires some delay to call the associated event.</returns>
        /// <inheritdoc cref="Stop(ConversationPlayer, bool)"/>
        protected virtual bool OnStop(ConversationPlayer _player, bool _isClosingConversation, Action _onComplete) {
            return true;
        }
        #endregion

        #region Complete
        private static readonly List<ConversationEvent> pendingBuffer = new List<ConversationEvent>();
        private static Action onCompleteDelegate = null;

        // -----------------------

        /// <summary>
        /// Set the delegate to be called once all events are stopped.
        /// </summary>
        /// <param name="_onComplete">Delegate to call once all events are stopped.</param>
        internal static void CompleteQuit(Action _onComplete) {
            if (pendingBuffer.Count == 0) {
                _onComplete.Invoke();
                return;
            }

            onCompleteDelegate = _onComplete;
        }

        // -----------------------

        private static void RegisterPendingEvent(ConversationEvent _event) {
            pendingBuffer.Add(_event);
        }

        private static void UnregisterPendingEvent(ConversationEvent _event) {
            pendingBuffer.Remove(_event);

            if ((pendingBuffer.Count == 0) && (onCompleteDelegate != null)) {

                onCompleteDelegate.Invoke();
                onCompleteDelegate = null;
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ConversationEvent"/> array wrapper.
    /// </summary>
    /// <typeparam name="T"><see cref="ConversationEvent"/> type contained in this group.</typeparam>
    [Serializable]
    public class ConversationEventGroup<T> : ConversationEvent where T : ConversationEvent {
        #region Global Members
        /// <summary>
        /// All events contained in this group.
        /// </summary>
        [SerializeField, DisplayName("Name", true)] private BlockArray<T> events = new BlockArray<T>();

        /// <summary>
        /// Displayed name of this group.
        /// </summary>
        public string Name {
            get {
                #if UNITY_EDITOR
                string _name = typeof(T).Name.Replace(typeof(ConversationEvent).Name, string.Empty);
                return ObjectNames.NicifyVariableName($"{_name}s");
                #else
                return typeof(ConversationEvent).Name;
                #endif
            }
        }

        // -----------------------

        public override bool IsAvailable {
            get { return events.Count != 0; }
        }

        public override bool IsPlaying {
            get {

                for (int i = 0; i < events.Count; i++) {

                    T _event= events[i];
                    if (_event.IsPlaying) {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Behaviour
        protected override bool OnPlay(ConversationPlayer _player) {
            bool _success = false;

            for (int i = 0; i < events.Count; i++) {

                T _event= events[i];
                if (_event.Play(_player)) {
                    _success = true;
                }
            }

            return _success;
        }

        protected override bool OnStop(ConversationPlayer _player, bool _isClosingConversation, Action _onQuit) {

            for (int i = 0; i < events.Count; i++) {

                T _event= events[i];
                _event.Stop(_player, _isClosingConversation);
            }

            return true;
        }
        #endregion
    }
}
