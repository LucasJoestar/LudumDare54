// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Conversations.Timeline {
    /// <summary>
    /// Plays a <see cref="ConversationNode"/> during this clip.
    /// </summary>
    [DisplayName("Conversation/Play Conversation Node")]
    public class PlayConversationNodeClip : ConversationPlayableAsset<PlayConversationNodeBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "Play Conversation Node"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="PlayConversationNodeClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public class PlayConversationNodeBehaviour : EnhancedPlayableBehaviour<Conversation> {
        #region Global Members
        [Tooltip("If true, plays the next node from the active conversation")]
        public bool PlayNext = true;

        [Tooltip("ID of the node to play")]
        [Enhanced, ShowIf("PlayNext", ConditionType.False)] public int NodeGUID = 0;

        // -----------------------

        protected override bool CanExecuteInEditMode {
            get { return false; }
        }
        #endregion

        #region Behaviour
        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            if (bindingObject.IsNull()) {
                return;
            }

            // Play node.
            if (bindingObject.GetPlayer(out ConversationPlayer _player)) {

                if (PlayNext) {
                    _player.PlayNextNode();
                    return;

                } else if (bindingObject.FindNode(NodeGUID, out ConversationNode _node)) {

                    _player.PlayNode(_node);
                    return;
                }
            }

            // Failed.
            bindingObject.LogWarningMessage("Node could not be played");
        }
        #endregion
    }
}
