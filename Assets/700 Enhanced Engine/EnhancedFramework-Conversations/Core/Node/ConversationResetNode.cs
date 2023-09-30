// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using System;

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="ConversationNode"/> used to reset a conversation, like it's never been played before.
    /// </summary>
    [Serializable, DisplayName("Base/Reset")]
    public class ConversationResetNode : ConversationNode {
        #region Global Members
        public override string Description {
            get { return "Resets all nodes in this conversation on play, like they've never been played before"; }
        }

        public override string DefaultSpeaker {
            get { return "[RESET]"; }
        }
        #endregion

        #region Behaviour
        public override void Play(ConversationPlayer _player) {

            // Reset all conversation nodes.
            ResetNode(_player.Conversation.root);
            _player.PlayNextNode();

            // ----- Local Method ----- \\

            void ResetNode(ConversationNode _node) {
                _node.Reset();

                foreach (ConversationNode _subNode in _node.nodes) {
                    ResetNode(_subNode);
                }
            }
        }
        #endregion

        #region Editor Utility
        protected internal override int GetEditorIcon(int _index, out string _iconName) {
            switch (_index) {
                case 0:
                    _iconName = "Grid.EraserTool";
                    break;

                default:
                    _iconName = string.Empty;
                    break;
            }

            return 1;
        }

        protected internal override string GetEditorDisplayedText() {
            return "RESET";
        }
        #endregion
    }
}
