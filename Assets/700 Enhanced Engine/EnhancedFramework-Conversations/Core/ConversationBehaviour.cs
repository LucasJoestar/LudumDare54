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
using UnityEngine;

#if LOCALIZATION_ENABLED
using EnhancedFramework.Localization;
#endif

namespace EnhancedFramework.Conversations {
    /// <summary>
    /// <see cref="Component"/> wrapper for a <see cref="Conversations.Conversation"/>.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Conversation/Conversation")]
    public class ConversationBehaviour : EnhancedBehaviour
                                         #if LOCALIZATION_ENABLED
                                         , IResourceBehaviour<LocalizationResourceLoader>
                                         #endif
    {
        #region Global Members
        [Section("Conversation")]

        [SerializeField, Enhanced, Required] private Conversation conversation = null;

        /// <summary>
        /// The <see cref="Conversations.Conversation"/> of this behaviour.
        /// </summary>
        public Conversation Conversation {
            get { return conversation; }
        }
        #endregion

        #region Operator
        public static implicit operator Conversation(ConversationBehaviour _behaviour) {
            return _behaviour.Conversation;
        }
        #endregion

        #region Localization
        #if LOCALIZATION_ENABLED
        void IResourceBehaviour<LocalizationResourceLoader>.FillResource(LocalizationResourceLoader _resource) {
            // Fill conversation tables for preload.
            _resource.FillTables(conversation);
        }
        #endif
        #endregion
    }
}
