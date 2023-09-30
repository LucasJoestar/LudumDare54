// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Timeline;
using System.ComponentModel;
using UnityEngine.Timeline;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Conversations.Timeline {
    /// <summary>
    /// <see cref="TrackAsset"/> class for every <see cref="IConversationPlayableAsset"/>.
    /// </summary>
    [TrackColor(.627f, .125f, .941f)] // Purple
    [TrackClipType(typeof(IConversationPlayableAsset))]
    [TrackBindingType(typeof(Conversation), TrackBindingFlags.None)]
    [DisplayName("Enhanced Framework/Conversation Track")]
    public class ConversationTrack : EnhancedTrack { }
}
