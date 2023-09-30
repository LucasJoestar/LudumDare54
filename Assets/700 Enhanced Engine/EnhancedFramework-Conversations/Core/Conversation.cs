// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
//  Use the [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Namespace", "Assembly", "Class")]
//  attribute to remove a managed reference error when renaming a script or an assembly.
//
// ================================================================================================ //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

using DisplayName = EnhancedEditor.DisplayNameAttribute;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using ArrayUtility = EnhancedEditor.ArrayUtility;

[assembly: InternalsVisibleTo("EnhancedFramework.Conversations.Editor")]
namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="Conversation"/> root node class.
    /// </summary>
    [Serializable, Ethereal]
    public sealed class ConversationRoot : ConversationNode {
        #region Global Members
        #if UNITY_EDITOR
        /// <summary>
        /// In the editor only, used to display and edit the conversation name.
        /// </summary>
        [SerializeField] internal Conversation conversation = null;

        // -----------------------

        public override string Text {
            get {
                if (conversation == null) {
                    return base.Text;
                }

                return conversation.name.Replace(conversation.name.GetPrefix(), string.Empty);
            }
            set {
                if (conversation == null) {
                    return;
                }

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(conversation), $"{conversation.name.GetPrefix()}{value}");
            }
        }
        #endif

        public override string DefaultSpeaker {
            get { return "[ROOT]"; }
        }

        public override bool IsRoot {
            get { return true; }
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {
            base.Play(_player);

            // Automatically play the next node.
            _player.PlayNextNode();
        }
        #endregion

        #region Editor Utility
        internal protected override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Profiler.Custom";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="ConversationPlayer"/> class, only sending logs about its current state.
    /// </summary>
    [Serializable, DisplayName("<None>")]
    public class ConversationDefaultPlayer : ConversationPlayer<ConversationDefaultSettings> {
        #region State
        protected override void OnSetup() {
            base.OnSetup();

            this.LogMessage($"Setup \'{Name}\', ready to be played", Conversation);
        }

        protected override void OnClose(Action _onNodeQuit = null) {
            base.OnClose(_onNodeQuit);

            this.LogMessage($"Closing \'{Name}\'", Conversation);
            CancelPlay();
        }
        #endregion

        #region Behaviour
        private DelayHandler delayedCall = default;

        // -----------------------

        public override void PlayCurrentNode() {
            base.PlayCurrentNode();

            this.LogMessage($"Playing node {CurrentNode.Guid} - \"{CurrentNode.Text}\"", Conversation);

            // Use a delay before playing the next node,
            // avoiding infinite loops on referenced links.
            delayedCall = Delayer.Call(.1f, () => PlayNextNode(true), false);
        }

        private void CancelPlay() {
            delayedCall.Cancel();
        }
        #endregion
    }

    /// <summary>
    /// Default <see cref="ConversationSettings"/> class, only containing an array of <see cref="string"/> for the speakers.
    /// </summary>
    [Serializable, DisplayName("<Default>")]
    public class ConversationDefaultSettings : ConversationSettings<string> {
        #region Global Members
        /// <inheritdoc cref="ConversationDefaultSettings"/>
        public ConversationDefaultSettings() {
            Speakers = new string[] { "Player", "NPC" };
        }
        #endregion

        #region Speaker
        public override string GetSpeakerAt(int _index) {
            return Speakers[_index];
        }
        #endregion
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> database for a conversation.
    /// </summary>
    [CreateAssetMenu(fileName = FilePrefix + "NewConversation", menuName = FrameworkUtility.MenuPath + "Conversation", order = FrameworkUtility.MenuOrder + 50)]
    public class Conversation : EnhancedScriptableObject
                                #if LOCALIZATION_ENABLED
                                , ILocalizable
                                #endif
    {
        public const string FilePrefix = "CNV_";

        #region Global Members
        [Section("Conversation")]

        [Tooltip("Node type to be used when creating a new default node in this conversation")]
        [SerializeField, DisplayName("Default Node")]
        private SerializedType<ConversationNode> defaultNodeType = new SerializedType<ConversationNode>(SerializedTypeConstraint.None, typeof(ConversationTextLine),
                                                                                                                                       #if LOCALIZATION_ENABLED
                                                                                                                                       typeof(ConversationLocalizedLine),
                                                                                                                                       #endif
                                                                                                                                       typeof(ConversationLink),
                                                                                                                                       typeof(ConversationResetNode));

        [Tooltip("Node type to be used when creating a new link in this conversation")]
        [SerializeField, DisplayName("Default Link")]
        private SerializedType<ConversationLink> defaultLinkType = new SerializedType<ConversationLink>(SerializedTypeConstraint.BaseType, typeof(ConversationLink));

        [Space(5f)]

        [Tooltip("Class used to play this conversation, managing its behaviour")]
        [SerializeField, DisplayName("Conversation Player")]
        private SerializedType<ConversationPlayer> playerType = new SerializedType<ConversationPlayer>(SerializedTypeConstraint.None, typeof(ConversationDefaultPlayer));

        /// <summary>
        /// Node type to be used when creating a new default node in this conversation (must be derived from <see cref="ConversationNode"/>).
        /// </summary>
        public Type DefaultNodeType {
            get { return defaultNodeType.Type; }
            set { defaultNodeType.Type = value; }
        }

        /// <summary>
        /// Node type to be used when creating a new link in this conversation (must be derived from <see cref="ConversationLink"/>).
        /// </summary>
        public Type DefaultLinkType {
            get { return defaultLinkType.Type; }
            set { defaultLinkType.Type = value; }
        }

        /// <summary>
        /// Type class used to play this conversation, managing its behaviour (must be derived from <see cref="ConversationPlayer{T}"/>).
        /// </summary>
        public Type PlayerType {
            get { return playerType.Type; }
            set {
                playerType.Type = value;

                var _settings = Activator.CreateInstance(GetSettingsType(value));
                settings = EnhancedUtility.CopyObjectContent(settings, _settings) as ConversationSettings;
            }
        }

        // -----------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeReference, Enhanced, Block] protected ConversationSettings settings = new ConversationDefaultSettings();

        private static string[] speakers = new string[0];

        /// <summary>
        /// <see cref="ConversationPlayer"/>-related settings of this conversation.
        /// </summary>
        public ConversationSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// Speaker names of this conversation.
        /// <br/> Especially used by property drawers.
        /// </summary>
        public string[] Speakers {
            get {

                int _count = settings.SpeakerCount;
                if (speakers.Length != _count) {
                    Array.Resize(ref speakers, _count);
                }

                for (int i = 0; i < speakers.Length; i++) {
                    speakers[i] = $"{settings.GetSpeakerAt(i)} [{i + 1}]";
                }

                return speakers;
            }
        }

        /// <summary>
        /// Get if there exist any duplicate speaker name in this conversation.
        /// </summary>
        public bool HasDuplicateName {
            get {

                string[] _speakers = Speakers;
                for (int i = 0; i < _speakers.Length; i++) {

                    string _speaker = _speakers[i];
                    for (int j = i + 1; j < _speakers.Length; j++) {

                        if (_speakers[j] == _speaker) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        // -----------------------

        [SerializeReference, HideInInspector] internal ConversationRoot root = new ConversationRoot();

        /// <summary>
        /// The root <see cref="ConversationNode"/> of this <see cref="Conversation"/>.
        /// </summary>
        public ConversationRoot Root {
            get { return root; }
        }

        /// <summary>
        /// Indicates if this <see cref="Conversation"/> has any available node to play.
        /// </summary>
        public bool IsPlayable {
            get {
                foreach (var _node in root.nodes) {
                    if (_node.IsAvailable) {
                        return true;
                    }
                }

                return false;
            }
        }

        // -------------------------------------------
        // Events
        // -------------------------------------------

        /// <summary>
        /// Called whenver this <see cref="Conversation"/> starts being played.
        /// </summary>
        public Action<Conversation, ConversationPlayer> OnPlayed = null;

        /// <summary>
        /// Called whenever this <see cref="Conversation"/> is being closed.
        /// </summary>
        public Action<Conversation, ConversationPlayer> OnClosed = null;
        #endregion

        #region Scriptable Object
        #if UNITY_EDITOR
        private void Awake() {
            // Root conversation setup.
            root.conversation = this;

            RefreshValues();
        }

        protected override void OnValidate() {
            base.OnValidate();

            RefreshValues();
        }

        // -----------------------

        private void RefreshValues() {
            if (Application.isPlaying) {
                return;
            }

            // Settings type update.
            if (GetSettingsType(PlayerType) != settings.GetType()) {
                PlayerType = playerType;
            }

            ResetNodes();
        }
        #endif
        #endregion

        #region Player
        private ConversationPlayer player = null;
        private bool needReset = false;

        // -----------------------

        /// <inheritdoc cref="CreatePlayer(ConversationNode)"/>
        public ConversationPlayer CreatePlayer() {

            // Reset.
            if (needReset) {

                ResetNodes();
                needReset = false;
            }

            player = Activator.CreateInstance(PlayerType) as ConversationPlayer;
            player.Setup(this);

            // Event.
            OnPlayed?.Invoke(this, player);
            return player;
        }

        /// <summary>
        /// Creates and setup a new <see cref="ConversationPlayer"/> for this conversation.
        /// <br/> Use this to play its content.
        /// </summary>
        /// <param name="_currentNode">First node to play.</param>
        /// <returns>The newly created <see cref="ConversationPlayer"/> to play this conversation.</returns>
        public ConversationPlayer CreatePlayer(ConversationNode _currentNode) {

            // Reset.
            if (needReset) {

                ResetNodes();
                needReset = false;
            }

            player = Activator.CreateInstance(PlayerType) as ConversationPlayer;
            player.Setup(this, _currentNode);

            // Event.
            OnPlayed?.Invoke(this, player);

            return player;
        }

        /// <summary>
        /// Closes the last created <see cref="ConversationPlayer"/> for this conversation.
        /// </summary>
        /// <returns>True if the player could be successfully closed, false otherwise.</returns>
        public bool ClosePlayer() {
            if (player != null) {
                player.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a <see cref="ConversationPlayer"/> of this conversation is closed.
        /// </summary>
        /// <param name="_player">The <see cref="ConversationPlayer"/> being closed.</param>
        internal void OnPlayerClosed(ConversationPlayer _player) {
            if (player == _player) {
                player = null;

                // Event.
                OnClosed?.Invoke(this, _player);
            }
        }

        /// <summary>
        /// Get this <see cref="Conversation"/> current active <see cref="ConversationPlayer"/>.
        /// </summary>
        /// <param name="_player">This conversation active <see cref="ConversationPlayer"/> (null if none).</param>
        /// <returns>True if an active <see cref="ConversationPlayer"/> could be found, false otherwise.</returns>
        public bool GetPlayer(out ConversationPlayer _player) {
            if ((player != null) && player.IsPlaying) {
                _player = player;
                return true;
            }

            _player = null;
            return false;
        }
        #endregion

        #region Nodes
        /// <summary>
        /// Adds a new default node to this conversation, at a specific root node.
        /// </summary>
        /// <inheritdoc cref="AddNode(ConversationNode, Type)"/>
        public ConversationNode AddDefaultNode(ConversationNode _root) {
            return AddNode(_root, DefaultNodeType);
        }

        /// <summary>
        /// Adds a new specific type of <see cref="ConversationNode"/> to a specific root node from this conversation.
        /// </summary>
        /// <param name="_root">The root <see cref="ConversationNode"/> to add a new node to.</param>
        /// <param name="_nodeType">The type of node to create and add (must inherit from <see cref="ConversationNode"/>).</param>
        /// <returns>The newly created node.</returns>
        public ConversationNode AddNode(ConversationNode _root, Type _nodeType) {
            if (!_nodeType.IsSubclassOf(typeof(ConversationNode))) {
                return null;
            }

            ConversationNode _node = Activator.CreateInstance(_nodeType) as ConversationNode;
            _root.AddNode(_node);

            return _node;
        }

        /// <summary>
        /// Removes a specific <see cref="ConversationNode"/> from this conversation.
        /// </summary>
        /// <param name="_node">The <see cref="ConversationNode"/> to remove.</param>
        public void RemoveNode(ConversationNode _node) {
            if (FindNode(_node, out ConversationNode _root)) {
                ArrayUtility.Remove(ref _root.nodes, _node);
            }
        }

        /// <summary>
        /// Finds the <see cref="ConversationNode"/> matching a given guid.
        /// </summary>
        /// <param name="_guid">GUID to find matching node.</param>
        /// <param name="_node">Found node matching the given guid (null if none).</param>
        /// <returns>True if a node matching the given guid was successfully found, false otherwise.</returns>
        public bool FindNode(int _guid, out ConversationNode _node) {
            return DoFindNode(root, out _node);

            // ----- Local Method ----- \\

            bool DoFindNode(ConversationNode _root, out ConversationNode _doNode) {

                foreach (ConversationNode _innerNode in _root.nodes) {

                    if (_innerNode.Guid == _guid) {
                        _doNode = _innerNode;
                        return true;
                    }

                    if (!_root.ShowNodes) {
                        continue;
                    }

                    if (DoFindNode(_innerNode, out _doNode)) {
                        return true;
                    }
                }

                _doNode = null;
                return false;
            }
        }

        /// <summary>
        /// Finds a given <see cref="ConversationNode"/> with its root node.
        /// </summary>
        /// <param name="_node">The node to find.</param>
        /// <param name="_root">Root of the given node (null if not found).</param>
        /// <returns>True if the given node was successfully found, false otherwise.</returns>
        public bool FindNode(ConversationNode _node, out ConversationNode _root) {
            return DoFindNode(root, out _root);

            // ----- Local Method ----- \\

            bool DoFindNode(ConversationNode _root, out ConversationNode _doRoot) {

                foreach (ConversationNode _innerNode in _root.nodes) {

                    if (_innerNode == _node) {
                        _doRoot = _root;
                        return true;
                    }

                    if (!_root.ShowNodes) {
                        continue;
                    }

                    if (DoFindNode(_innerNode, out _doRoot)) {
                        return true;
                    }
                }

                _doRoot = null;
                return false;
            }
        }

        /// <summary>
        /// Resets all nodes. Called to clear behaviour when exiting play mode.
        /// </summary>
        public void ResetNodes() {
            DoResetNode(root);

            // ----- Local Method ----- \\

            void DoResetNode(ConversationNode _root) {
                _root.Reset();

                if (!_root.ShowNodes) {
                    return;
                }

                foreach (ConversationNode _innerNode in _root.nodes) {
                    DoResetNode(_innerNode);
                }
            }
        }

        /// <summary>
        /// Mark this database as requiring to be reset.
        /// </summary>
        public void ResetForNextPlay() {
            needReset = true;
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public void GetLocalizationTables(Set<TableReference> _stringTables,  Set<TableReference> _assetTables) {

            root.GetLocalizationTables(_stringTables, _assetTables);
            settings.GetLocalizationTables(_stringTables, _assetTables);
        }
        #endif
        #endregion

        #region Utility
        internal int lastSelectedIndex = -1;

        // -----------------------

        /// <summary>
        /// Get this conversation <see cref="ConversationSettings"/> type (<see cref="ConversationPlayer{T}"/>-related).
        /// </summary>
        /// <param name="_player">The <see cref="ConversationPlayer{T}"/> type to get the associated settings.</param>
        /// <returns>This conversation settings type.</returns>
        private Type GetSettingsType(Type _player) {
            while (_player.BaseType != null) {
                _player = _player.BaseType;

                if (_player.IsGenericType && (_player.GetGenericTypeDefinition() == typeof(ConversationPlayer<>))) {
                    return _player.GetGenericArguments()[0];
                }
            }

            return null;
        }
        #endregion
    }
}
