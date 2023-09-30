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
    /// Plays a <see cref="Conversation"/> for the duration of the clip.
    /// </summary>
    [DisplayName("Conversation/Play Conversation")]
    public class PlayConversationClip : ConversationPlayableAsset<PlayConversationBehaviour> {
        #region Utility
        public override string ClipDefaultName {
            get { return "Conversation"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="PlayConversationClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public class PlayConversationBehaviour : EnhancedPlayableBehaviour<Conversation> {
        #region Global Members
        [Tooltip("If true, automatically closes the conversation on exit")]
        public bool AutoClose = true;

        // -----------------------

        protected override bool CanExecuteInEditMode {
            get { return false; }
        }
        #endregion

        #region Behaviour
        private ConversationPlayer player = null;

        // -----------------------

        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            if (bindingObject.IsNull()) {
                return;
            }

            // Play.
            player = bindingObject.CreatePlayer();
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            // Close.
            if (AutoClose && (player != null)) {
                player.Close();
            }
        }
        #endregion
    }
}
