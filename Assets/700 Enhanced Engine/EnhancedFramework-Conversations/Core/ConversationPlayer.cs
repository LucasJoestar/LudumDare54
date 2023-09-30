// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// Base class used to play a <see cref="Conversations.Conversation"/>.
    /// <br/>Non-generic global version of <see cref="ConversationPlayer{T}"/>.
    /// <para/>
    /// Should never be directly inherited from, always prefer using <see cref="ConversationPlayer{T}"/> instead.
    /// </summary>
    [Serializable]
    public abstract class ConversationPlayer {
        #region Global Members
        /// <summary>
        /// The playing <see cref="Conversations.Conversation"/>.
        /// </summary>
        public Conversation Conversation = null;

        /// <summary>
        /// Currently playing <see cref="ConversationNode"/>.
        /// </summary>
        public ConversationNode CurrentNode = null;

        /// <summary>
        /// Whether this player is currently active or not.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Name of this player <see cref="Conversations.Conversation"/>.
        /// </summary>
        public string Name {
            get { return Conversation.name; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents inheriting from this class in other assemblies.
        /// </summary>
        private protected ConversationPlayer() { }
        #endregion

        #region State
        /// <inheritdoc cref="Setup(Conversation, ConversationNode)"/>
        public void Setup(Conversation _conversation) {
            Setup(_conversation, _conversation.Root);
        }

        /// <summary>
        /// Setups this player with the <see cref="Conversations.Conversation"/> to play.
        /// </summary>
        /// <param name="_conversation">The <see cref="Conversations.Conversation"/> to play</param>
        /// <param name="_currentNode">The first <see cref="ConversationNode"/> to play.</param>
        public virtual void Setup(Conversation _conversation, ConversationNode _currentNode) {
            Conversation = _conversation;
            CurrentNode = _currentNode;

            IsPlaying = true;
            OnSetup();
        }

        /// <summary>
        /// Stop playing the conversation.
        /// </summary>
        /// <param name="_onNodeQuit">Delegate to be called once the current node was quit.</param>
        public void Close(Action _onNodeQuit = null) {
            _onNodeQuit += () => Conversation.OnPlayerClosed(this);

            if (!IsPlaying) {
                _onNodeQuit?.Invoke();
                return;
            }

            IsPlaying = false;
            OnClose(_onNodeQuit);
        }

        // -----------------------

        /// <summary>
        /// Called once this player is setup.
        /// <para/>
        /// By default, plays the first node of this player.
        /// <br/> Use this to update the game current state and interface.
        /// </summary>
        protected virtual void OnSetup() {

            if (CurrentNode.IsAvailable) {
                PlayCurrentNode();
            } else {
                Close();
            }
        }

        /// <summary>
        /// Called when this player is being closed.
        /// <para/>
        /// By default, quits the current playing node.
        /// <br/> Use this to update the game current state and interface.
        /// </summary>
        /// <param name="_onNodeQuit">Delegate to be called once the current node has been quit.</param>
        protected virtual void OnClose(Action _onNodeQuit = null) {
            CurrentNode.Quit(this, true, _onNodeQuit);
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Replays the current node from the start.
        /// </summary>
        public virtual void ReplayCurrentNode() {
            PlayNode(CurrentNode);
        }

        /// <summary>
        /// Quit the current node and play the next one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_node">The next <see cref="ConversationNode"/> to play.</param>
        public virtual void PlayNode(ConversationNode _node, bool _play = true) {
            ConversationNode _previous = CurrentNode;
            CurrentNode = _node;

            Action _onQuit;

            if (_play) {
                _onQuit = PlayCurrentNode;
            } else {
                _onQuit = null;
            }

            _previous.Quit(this, false, _onQuit);
        }

        /// <summary>
        /// Plays the next <see cref="ConversationNode"/>, based on the current one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        public virtual void PlayNextNode(bool _play = true) {
            // If there is no other node to play, terminate playing the conversation.
            if (!GetNextNode(out ConversationNode _next)) {

                Close();
                return;
            }

            PlayNode(_next, _play);
        }

        /// <summary>
        /// Plays this player <see cref="CurrentNode"/>.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        public virtual void PlayCurrentNode() {

            if (!CurrentNode.enabled) {

                PlayNextNode();
                return;
            }

            CurrentNode.Play(this);
        }

        // -----------------------

        /// <summary>
        /// Get the next <see cref="ConversationNode"/> to be played.
        /// </summary>
        /// <param name="_next">The next <see cref="ConversationNode"/> to play.</param>
        /// <returns>True if a new node to play was successfully found, false otherwise.</returns>
        public abstract bool GetNextNode(out ConversationNode _next);

        /// <param name="_behaviour">Behaviour used to get the next node.</param>
        /// <param name="_node">Current node to get the next node from.</param>
        /// <inheritdoc cref="GetNextNode(out ConversationNode))"/>
        public virtual bool GetNextNode(NextNodeBehaviour _behaviour, ConversationNode _node, out ConversationNode _next) {
            switch (_behaviour) {

                // Get the first available node.
                case NextNodeBehaviour.PlayFirst:
                    for (int i = 0; i < _node.NodeCount; i++) {
                        _next = _node.GetNodeAt(i);

                        if (_next.IsAvailable) {
                            return true;
                        }
                    }
                    break;

                // Get the last available node.
                case NextNodeBehaviour.PlayLast:
                    for (int i = _node.NodeCount; i-- > 0;) {
                        _next = _node.GetNodeAt(i);

                        if (_next.IsAvailable) {
                            return true;
                        }
                    }
                    break;

                // Play a random node.
                case NextNodeBehaviour.Random:
                    int _count = 0;

                    for (int i = _node.NodeCount; i-- > 0;) {
                        if (_node.GetNodeAt(i).IsAvailable) {
                            _count++;
                        }
                    }

                    if (_count != 0) {

                        // Random.
                        int _random;
                        if (_count == 1) {

                            _random = 0;

                        } else {

                            _random = Mathm.RandomNoRepeat(0, _count, Conversation.lastSelectedIndex);
                            Conversation.lastSelectedIndex = _random;
                        }

                        // Get.
                        for (int i = _node.NodeCount; i-- > 0;) {
                            _next = _node.GetNodeAt(i);

                            if (_next.IsAvailable) {
                                if (_random == 0) {
                                    return true;
                                }

                                _random--;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            _next = null;
            return false;
        }
        #endregion

        #region Utility
        /// <returns><inheritdoc cref="GetSettings{T}(out T)" path="/param[@name='_settings']"/></returns>
        /// <inheritdoc cref="GetSettings{T}(out T)"/>
        public abstract T GetSettings<T>() where T : ConversationSettings;

        /// <summary>
        /// Get this player associated <see cref="ConversationSettings"/>.
        /// </summary>
        /// <typeparam name="T">The expected <see cref="ConversationSettings"/> type.</typeparam>
        /// <param name="_settings">This player associated <see cref="ConversationSettings"/>.</param>
        /// <returns>True if this player settings could be casted to the expected type, false otherwise.</returns>
        public abstract bool GetSettings<T>(out T _settings) where T : ConversationSettings;
        #endregion
    }

    /// <summary>
    /// Base class to inherit all <see cref="Conversation"/> players from.
    /// <br/> Use this to implement a specific behaviour when playing the associated <see cref="Conversation"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="ConversationSettings"/> type required by this player.</typeparam>
    [Serializable]
    public abstract class ConversationPlayer<T> : ConversationPlayer where T : ConversationSettings, new() {
        #region Global Members
        /// <summary>
        /// The playing <see cref="Conversation"/> associated <see cref="ConversationSettings"/>.
        /// </summary>
        public T Settings = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances using the class constructor.
        /// <br/> To create a new player, always use <see cref="Conversation.CreatePlayer()"/>.
        /// </summary>
        internal protected ConversationPlayer() : base() { }
        #endregion

        #region State
        public sealed override void Setup(Conversation _conversation, ConversationNode _currentNode) {
            // Get settings.
            Settings = _conversation.Settings as T;

            base.Setup(_conversation, _currentNode);
        }
        #endregion

        #region Behaviour
        public override bool GetNextNode(out ConversationNode _next) {
            return GetNextNode(Settings.NextNodeBehaviour, CurrentNode, out _next);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="GetSettings{U}"/>
        public T GetSettings() {
            return GetSettings<T>();
        }

        public sealed override U GetSettings<U>() {
            if (GetSettings(out U _settings)) {
                return _settings;
            }

            throw new InvalidCastException($"Could not cast settings of type \'{Settings.GetType().Name}\' in type \'{typeof(U).Name}\'.");
        }

        public override bool GetSettings<U>(out U _settings) {
            return EnhancedUtility.IsType(Settings, out _settings);
        }
        #endregion
    }
}
