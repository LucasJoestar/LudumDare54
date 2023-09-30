// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using System;
using UnityEngine;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="ConversationNode"/> used to redirect to another existing node.
    /// <br/> Use this to avoid duplicates.
    /// </summary>
    [Serializable, DisplayName("Base/Link")]
    public class ConversationLink : ConversationNode {
        #region Global Members
        /// <summary>
        /// Whether to skip the linked line content or not.
        /// </summary>
        [Tooltip("Whether to skip the linked line content or not")]
        public bool SkipNode = false;

        /// <summary>
        /// This link redirection <see cref="ConversationNode"/>.
        /// </summary>
        public ConversationNode Link {
            get {
                GetLink(out ConversationNode _link);
                return _link;
            }
            set {
                SetLink(value);
            }
        }

        // -----------------------

        public override string Text {
            get {
                return GetLink(out ConversationNode _link)
                     ? _link.Text
                     : base.Text;
            } set {
                if (GetLink(out ConversationNode _link)) {
                    _link.Text = value;
                }
            }
        }

        public override bool IsAvailable {
            get {
                return GetLink(out ConversationNode _link)
                     ? _link.IsAvailable
                     : false;
            }
        }

        public override int SpeakerIndex {
            get {
                return GetLink(out ConversationNode _link)
                     ? _link.SpeakerIndex
                     : base.SpeakerIndex;
            }
        }

        public override int NodeCount {
            get { return GetLink(out ConversationNode _link)
                       ? _link.NodeCount
                       : 0;
            }
        }

        public override bool IsClosingNode {
            get {
                return GetLink(out ConversationNode _link)
                       ? _link.IsClosingNode
                       : true;
            }
        }

        // -----------------------

        public override string DefaultSpeaker {
            get { return "[LINK]"; }
        }

        internal protected override bool ShowNodes {
            get { return false; }
        }
        #endregion

        #region Link Management
        /// <summary>
        /// Get this link redirection node.
        /// </summary>
        /// <param name="_link"><inheritdoc cref="Link" path="/summary"/></param>
        /// <returns>True if this node link was successfully found, false otherwise.</returns>
        public bool GetLink(out ConversationNode _link) {
            if (nodes.Length == 0) {
                _link = null;
                return false;
            }

            _link = nodes[0];
            return true;
        }

        /// <summary>
        /// Set this link redirection node.
        /// </summary>
        /// <param name="_node"><inheritdoc cref="Link" path="/summary"/></param>
        public void SetLink(ConversationNode _node) {
            if (_node == null) {
                return;
            }

            if (_node is ConversationLink _link) {
                if (_node.nodes.Length == 0) {
                    return;
                }

                _node = _link.Link;
            }

            if (nodes.Length == 0) {
                base.AddNode(_node);
            } else {
                nodes[0] = _node;
            }
        }

        internal void RemoveLink() {
            Array.Resize(ref nodes, 0);
        }

        // -----------------------

        public override ConversationNode GetNodeAt(int _index) {
            return Link.GetNodeAt(_index);
        }

        public override void AddNode(ConversationNode _node) {
            SetLink(_node);
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {
            if (SkipNode) {
                Link.Skip(_player);
            } else {
                Link.Play(_player);
            }
        }

        public override void Quit(ConversationPlayer _player, bool _isClosingConversation, Action _onQuit) {
            Link.Quit(_player, _isClosingConversation, _onQuit);
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Linked";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }

        protected internal override string GetEditorDisplayedText() {
            return GetLink(out ConversationNode _link)
                 ? _link.Text
                 : "NULL";
        }

        protected internal override int OnEditorContextMenu(int _index, out GUIContent _content, out Action _callback, out bool _enabled) {
            switch (_index) {
                case 0:
                    _content = new GUIContent("Paste Link as Override", "Overrides this link with the last one copied in the clipboard.");
                    _callback = PasteLink;

                    ConversationNode _link = ConversationNodeUtility.CopyBuffer;
                    _enabled = (_link != null) && !(_link is ConversationLink);

                    break;

                default:
                    _content = null;
                    _callback = null;
                    _enabled = false;

                    break;
            }

            return 1;

            // ----- Local Method ----- \\

            void PasteLink() {
                SetLink(ConversationNodeUtility.CopyBuffer);
            }
        }
        #endregion
    }
}
