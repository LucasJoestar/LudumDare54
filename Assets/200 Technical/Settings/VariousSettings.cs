// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using UnityEngine;

namespace LudumDare54
{
    /// <summary>
    /// Contains various global game settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "VariousSettings", menuName = MenuPath + "Various Settings", order = MenuOrder)]
    public class VariousSettings : BaseSettings<VariousSettings> {
        #region Global Members
        [Section("Various Settings", order = -1)]

        public LayerMask GroundMask = new LayerMask();
        public LayerMask WallMask   = new LayerMask();
        #endregion
    }
}
