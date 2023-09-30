// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization.Tables;
#endif

#if UNITY_EDITOR
using UnityEditor.AnimatedValues;
#endif

using ArrayUtility = EnhancedEditor.ArrayUtility;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="Conversation"/> node base class.
    /// <br/> Inherit from this to create your own nodes.
    /// <para/>
    /// Cannot be set as abstract for serialization purposes.
    /// </summary>
    [Serializable, Ethereal]
    public class ConversationNode
                                  #if LOCALIZATION_ENABLED
                                  : ILocalizable
                                  #endif
    {
        #region Global Members
        public const string DefaultSpeakerName  = "[NONE]";
        public const string DefaultText         = "EMPTY";

        [PreventCopy, SerializeField, Enhanced, ReadOnly] internal int guid = EnhancedUtility.GenerateGUID();
        [SerializeField] internal protected bool available = true;
        [SerializeField] internal protected bool enabled = true;

        #if UNITY_EDITOR
        // Conversation editor window related properties.

        [NonSerialized] internal bool isSelected = false;
        [SerializeField, HideInInspector] internal AnimBool foldout = new AnimBool(true);
        [SerializeReference, NonSerialized] internal ConversationNode parent = null;
        #endif

        [SerializeReference, HideInInspector, PreventCopy] internal protected ConversationNode[] nodes = new ConversationNode[0];

        // -----------------------

        /// <summary>
        /// The <see cref="string"/> text content of this node (empty is none).
        /// </summary>
        public virtual string Text {
            get { return string.Empty; }
            set { }
        }

        /// <summary>
        /// Unique guid of this node.
        /// </summary>
        public int Guid {
            get { return guid; }
        }

        /// <summary>
        /// Indicates if this node is available to be played.
        /// </summary>
        public virtual bool IsAvailable {
            get { return available; }
        }

        /// <summary>
        /// Index of this node speaker (-1 if none assigned).
        /// </summary>
        public virtual int SpeakerIndex {
            get { return -1; }
        }

        /// <summary>
        /// Total count of this node connections (<see cref="ConversationNode"/>).
        /// </summary>
        public virtual int NodeCount {
            get { return nodes.Length; }
        }

        /// <summary>
        /// Indicates if the conversation should be closed after playing this node (check for any available connection(s) by default).
        /// </summary>
        public virtual bool IsClosingNode {
            get { return (NodeCount == 0) || !Array.Exists(nodes, (n) => n.IsAvailable); }
        }

        /// <summary>
        /// Indicates if this node is a root node.
        /// </summary>
        public virtual bool IsRoot {
            get { return false; }
        }

        // -----------------------

        /// <summary>
        /// Short description of this node, displayed on top of the inspector.
        /// </summary>
        public virtual string Description {
            get { return string.Empty; }
        }

        /// <summary>
        /// Default speaker name displayed for this node type (especially used in the editor).
        /// </summary>
        public virtual string DefaultSpeaker {
            get { return DefaultSpeakerName; }
        }

        /// <summary>
        /// Indicates if this node connections (<see cref="ConversationNode"/>) should be displayed in the editor.
        /// </summary>
        internal protected virtual bool ShowNodes {
            get { return true; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <summary>
        /// Prevents from creating new instances of this base class.
        /// </summary>
        protected ConversationNode() { }
        #endregion

        #region Operator
        public static implicit operator int(ConversationNode _node) {
            return _node.guid;
        }

        public override string ToString() {
            return ((int)this).ToString();
        }
        #endregion

        #region Node Management
        /// <summary>
        /// Get this node connection at a specific index (<see cref="ConversationNode"/>).
        /// <br/> Use <see cref="NodeCount"/> to get the total amount of connection nodes.
        /// </summary>
        /// <param name="_index">The index to get the connection node at.</param>
        /// <returns>This node connection at the given index (<see cref="ConversationNode"/>).</returns>
        public virtual ConversationNode GetNodeAt(int _index) {
            return nodes[_index];
        }

        /// <summary>
        /// Adds a new <see cref="ConversationNode"/> to this node connections.
        /// </summary>
        /// <param name="_node">The <see cref="ConversationNode"/> to add as a connection.</param>
        public virtual void AddNode(ConversationNode _node) {
            ArrayUtility.Add(ref nodes, _node);
        }

        /// <summary>
        /// Copies all the values of a specific <see cref="ConversationNode"/> into this one.
        /// </summary>
        /// <param name="_source">The source <see cref="ConversationNode"/> to copy the values from.</param>
        /// <param name="_copyConnections">Whether the connection nodes should also be copied or not.</param>
        /// <returns>This node instance.</returns>
        internal ConversationNode CopyNode(ConversationNode _source, bool _copyConnections = true) {
            EnhancedUtility.CopyObjectContent(_source, this);

            if (_copyConnections) {
                Array.Resize(ref nodes, _source.nodes.Length);

                for (int i = 0; i < nodes.Length; i++) {
                    ConversationNode _innerNode = _source.nodes[i];
                    ConversationNode _new = System.Activator.CreateInstance(_innerNode.GetType()) as ConversationNode;

                    nodes[i] = _new.CopyNode(_innerNode, _copyConnections);
                }
            }

            return this;
        }

        /// <summary>
        /// Transmutes this <see cref="ConversationNode"/> into a node of another type.
        /// </summary>
        /// <param name="_conversation"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_conversation']"/></param>
        /// <param name="_type">The new node type in which to transmute this node.
        /// <br/> Must inherit from <see cref="ConversationNode"/>.</param>
        /// <param name="_doTransmuteSelf">Whether this node should be transmuted or not.</param>
        /// <param name="_doTransmuteConnections">Whether this node connections should be transmuted or not.</param>
        /// <returns>The new transmuted node instance.</returns>
        internal ConversationNode Transmute(Conversation _conversation, Type _type, bool _doTransmuteSelf = true, bool _doTransmuteConnections = true) {
            // Connections.
            if (_doTransmuteConnections) {
                for (int i = 0; i < nodes.Length; i++) {
                    nodes[i].Transmute(_conversation, _type);
                }
            }

            // Self.
            if (_doTransmuteSelf) {
                var _new = System.Activator.CreateInstance(_type);
                ConversationNode _node = EnhancedUtility.CopyObjectContent(this, _new, true) as ConversationNode;

                if (_node is ConversationLink _link) {
                    _link.RemoveLink();
                }

                UpdateLink(_conversation.Root);

                // ----- Local Method ----- \\

                void UpdateLink(ConversationNode _root) {
                    for (int i = 0; i < _root.nodes.Length; i++) {
                        if (_root.nodes[i] == this) {
                            _root.nodes[i] = _node;
                        }

                        UpdateLink(_root.nodes[i]);
                    }
                }

                return _node;
            }

            return this;
        }
        #endregion

        #region Behaviour
        /// <summary>
        /// Called to reset this node behaviour.
        /// </summary>
        internal protected virtual void Reset() { }

        /// <summary>
        /// Plays this <see cref="ConversationNode"/>.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_player']"/></param>
        public virtual void Play(ConversationPlayer _player) { }

        /// <summary>
        /// Quits this <see cref="ConversationNode"/>, before moving to the next one.
        /// <para/>
        /// Override this to implement a specific behaviour.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_player']"/></param>
        /// <param name="_isClosingConversation">Indicates if the conversation is being closed or will continue to be played.</param>
        /// <param name="_onQuit">Delegate to be called once this node was quit.</param>
        public virtual void Quit(ConversationPlayer _player, bool _isClosingConversation, Action _onQuit) {
            _onQuit?.Invoke();
        }

        /// <summary>
        /// Skips this node content.
        /// </summary>
        /// <param name="_player"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_player']"/></param>
        public virtual void Skip(ConversationPlayer _player) {
            _player.PlayNextNode();
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public virtual void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) {
            // If this node connections are hidden, ignore them.
            // Avoids cyclic loops with links.
            if (!ShowNodes) {
                return;
            }

            for (int i = 0; i < NodeCount; i++) {
                GetNodeAt(i).GetLocalizationTables(_stringTables, _assetTables);
            }
        }
        #endif
        #endregion

        #region Editor Utility
        /// <summary>
        /// Called when this node is drawn in the editor.
        /// </summary>
        /// <param name="_conversation"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_conversation']"/></param>
        internal protected virtual void OnEditorDraw(Conversation _conversation) { }

        /// <summary>
        /// Get the name of this node speaker (used to display the associated color in the editor).
        /// </summary>
        /// <param name="_settings"><inheritdoc cref="Doc(Conversation, ConversationSettings, ConversationPlayer)" path="/param[@name='_settings']"/></param>
        /// <returns>Editor-related displayed name of this node speaker.</returns>
        internal protected virtual string GetEditorSpeakerName(ConversationSettings _settings) {
            int _speakerIndex = SpeakerIndex;

            if ((_speakerIndex < 0) || (_speakerIndex >= _settings.SpeakerCount)) {
                return DefaultSpeaker;
            }

            return _settings.GetSpeakerAt(_speakerIndex);
        }

        /// <summary>
        /// Get the name of the icons to display next to this node in the editor.
        /// <br/>
        /// The icons to load must be located in the 'Editor Default Resources' folder, at the root of the project.
        /// </summary>
        /// <param name="_index">Index of the icon to load.</param>
        /// <param name="_iconName">Name of the icon to load.</param>
        /// <returns>Total number of icon(s) to be loaded.</returns>
        internal protected virtual int GetEditorIcon(int _index, out string _iconName) {
            _iconName = string.Empty;
            return 0;
        }

        /// <summary>
        /// Get the text to be displayed for this node in the editor.
        /// <br/> The edited text is always <see cref="Text"/>.
        /// </summary>
        /// <returns>This node displayed text.</returns>
        internal protected virtual string GetEditorDisplayedText() {
            string _text = Text;
            return string.IsNullOrEmpty(_text) ? DefaultText : _text;
        }

        /// <summary>
        /// Get the additional context menu items to be displayed for this node in the editor.
        /// </summary>
        /// <param name="_index">Menu item index.</param>
        /// <param name="_content"><see cref="GUIContent"/> to be display on the item.</param>
        /// <param name="_callback">Callback when the item is clicked.</param>
        /// <param name="_enabled">Whether this menu item should be enabled or not.</param>
        /// <returns>Total number of item(s) to be added to the menu.</returns>
        internal protected virtual int OnEditorContextMenu(int _index, out GUIContent _content, out Action _callback, out bool _enabled) {
            _content = null;
            _callback = null;
            _enabled = false;

            return 0;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// Documentation only method.
        /// </summary>
        /// <param name="_conversation">The source <see cref="Conversation"/> of this node.</param>
        /// <param name="_settings">The <see cref="ConversationSettings"/> of this node <see cref="Conversation"/>.</param>
        /// <param name="_player">The <see cref="ConversationPlayer"/> used to play this node.</param>
        #pragma warning disable IDE0051
        private void Doc(Conversation _conversation, ConversationSettings _settings, ConversationPlayer _player) { }
        #endregion
    }
}
