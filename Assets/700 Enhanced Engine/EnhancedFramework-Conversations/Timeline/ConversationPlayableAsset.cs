// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using EnhancedFramework.Timeline;
using UnityEngine.Playables;

namespace EnhancedFramework.Conversations.Timeline {
    /// <summary>
    /// Base interface to inherit any <see cref="Conversation"/> <see cref="PlayableAsset"/> from.
    /// </summary>
    public interface IConversationPlayableAsset { }

    /// <summary>
    /// Base non-generic <see cref="Conversation"/> <see cref="PlayableAsset"/> class.
    /// </summary>
    public abstract class ConversationPlayableAsset : EnhancedPlayableAsset, IConversationPlayableAsset { }

    /// <summary>
    /// Base generic class for every <see cref="Conversation"/> <see cref="PlayableAsset"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EnhancedPlayableBehaviour"/> playable for this asset.</typeparam>
    public abstract class ConversationPlayableAsset<T> : EnhancedPlayableAsset<T, Conversation>, IConversationPlayableAsset
                                                         where T : EnhancedPlayableBehaviour<Conversation>, new() { }
}
