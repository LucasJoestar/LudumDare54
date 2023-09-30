// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Conversations ===== //
// 
// Notes:
//
// ================================================================================================ //

using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("EnhancedFramework.Conversations.Editor")]
namespace EnhancedFramework.Conversations {
    /// <summary>
    /// Contains multiple <see cref="ConversationNode"/>-related utilties,
    /// making connections between the editor and the runtime system.
    /// </summary>
    public static class ConversationNodeUtility {
        #region Content
        #if UNITY_EDITOR
        [SerializeReference] internal static ConversationNode copyBuffer = null;
        #endif

        /// <summary>
        /// Clipboard buffer for a <see cref="ConversationNode"/> reference (editor only).
        /// </summary>
        public static ConversationNode CopyBuffer {
            get {
                #if UNITY_EDITOR
                return copyBuffer;
                #else
                return null;
                #endif
            }
        }
        #endregion
    }
}
