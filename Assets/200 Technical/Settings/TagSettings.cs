// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using UnityEngine;

namespace LudumDare54 {
    /// <summary>
    /// <see cref="MultiTags"/>-related game settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "TagSettings", menuName = MenuPath + "Tag", order = MenuOrder)]
    public class TagSettings : BaseSettings<TagSettings> {
        #region Global Members
        [Section("Tag Settings")]

        [Tooltip("Identifier tag of the in-game Player")]
        public Tag Player = new Tag();
        #endregion

        #region Utility
        /// <summary>
        /// Get if a <see cref="Component"/> has the Player identifier tag.
        /// </summary>
        /// <param name="_component"><see cref="Component"/> to check.</param>
        /// <returns>True if the given <see cref="Component"/> has the Player identifier tag, false otherwise.</returns>
        public bool HasPlayerTag(Component _component) {
            return _component.HasTag(Player);
        }
        #endregion
    }
}
