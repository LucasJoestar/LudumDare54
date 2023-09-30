// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

#if LOCALIZATION_PACKAGE
#define LOCALIZATION_ENABLED
#endif

using EnhancedFramework.Core;
using System;
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
using UnityEngine.Localization.Tables;
#endif

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// Behaviours used to get the next node to play from a <see cref="ConversationPlayer"/>.
    /// </summary>
    public enum NextNodeBehaviour {
        PlayFirst,
        PlayLast,
        Random
    }

    /// <summary>
    /// Base class for all <see cref="Conversation"/>-related settings.
    /// <br/> You can inherit from <see cref="ConversationSettings{T}"/> for a quick implementation.
    /// </summary>
    [Serializable]
    public abstract class ConversationSettings
                                               #if LOCALIZATION_ENABLED
                                               : ILocalizable
                                               #endif
    {
        #region Global Members
        /// <summary>
        /// Default behaviour used to determine the next node to play.
        /// </summary>
        public NextNodeBehaviour NextNodeBehaviour = NextNodeBehaviour.PlayFirst;

        /// <summary>
        /// Total count of speakers in the conversation.
        /// </summary>
        public abstract int SpeakerCount { get; }
        #endregion

        #region Speaker
        /// <summary>
        /// Get a speaker name at a given index.
        /// </summary>
        /// <param name="_index">Index of the speaker to get.</param>
        /// <returns>The name of the speaker.</returns>
        public abstract string GetSpeakerAt(int _index);
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        /// <inheritdoc cref="ILocalizable.GetLocalizationTables(Set{TableReference}, Set{TableReference})"/>
        public virtual void GetLocalizationTables(Set<TableReference> _stringTables, Set<TableReference> _assetTables) { }
        #endif
        #endregion
    }

    /// <summary>
    /// <see cref="ConversationSettings"/> class with a ready-to-use array of speakers.
    /// </summary>
    [Serializable]
    public abstract class ConversationSettings<T> : ConversationSettings {
        #region Global Members
        [Space(10f)]

        /// <summary>
        /// Speakers of the conversation.
        /// </summary>
        public T[] Speakers = new T[] { };

        public override int SpeakerCount {
            get { return Speakers.Length; }
        }
        #endregion

        #region Speaker
        /// <summary>
        /// Get the speaker of these settings at a specific index.
        /// </summary>
        /// <param name="_index">The index to the get the speaker at.</param>
        /// <returns>The speaker at the given index.</returns>
        public T GetSpeaker(int _index) {
            return Speakers[_index];
        }

        public override string GetSpeakerAt(int _index) {
            T _speaker = GetSpeaker(_index);
            return (_speaker != null) ? _speaker.ToString() : ConversationNode.DefaultSpeakerName;
        }
        #endregion
    }
}
