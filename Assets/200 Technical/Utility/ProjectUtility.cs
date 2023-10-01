// ===== Twin Peaks - https://www.plasticscm.com/orgs/blueroseteam/repos/TwinPeaks/ ===== //
//
// Notes:
//
// ====================================================================================== //

using UnityEngine;

namespace LudumDare54 {
    /// <summary>
    /// Contains multiple project-related utilities.
    /// </summary>
    public static class ProjectUtility {
        #region Menu
        /// <summary>
        /// Name of this project.
        /// </summary>
        public const string Name = "LudumDare-54";

        /// <summary>
        /// Menu path prefix used for creating new <see cref="ScriptableObject"/>, or any other special menu.
        /// </summary>
        public const string MenuPath = Name + "/";

        /// <summary>
        /// Menu item path used for project utilities.
        /// </summary>
        public const string MenuItemPath = "Tools/" + MenuPath;

        /// <summary>
        /// Menu order used for creating new <see cref="ScriptableObject"/> from the asset menu.
        /// </summary>
        public const int MenuOrder = 200;
        #endregion
    }
}
