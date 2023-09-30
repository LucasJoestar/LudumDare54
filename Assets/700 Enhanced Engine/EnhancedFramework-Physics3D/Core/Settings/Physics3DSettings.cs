// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using UnityEngine;

using Min = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// 3D Physics related global settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "Physics3DSettings", menuName = MenuPath + "Physics 3D", order = MenuOrder)]
	public class Physics3DSettings : BaseSettings<Physics3DSettings> {
        #region Global Members
        [Section("Physics 3D Settings")]

        [Enhanced, Max(0f)] public float Gravity    = -9.81f;

        [Tooltip("Maximum gravity velocity for an object")]
        [Enhanced, Max(0f)] public float MaxGravity = -25f;

        [Space(5f)]

        // -----------------------

        [Tooltip("Maximum angle force a surface to be considered as ground")]
        [Enhanced, Range(.1f, 90f)] public float GroundAngle    = 30f;

        [Tooltip("Maximum default height used to climb steps and surfaces")]
        [Enhanced, Min(0f)] public float ClimbHeight            = .2f;

        [Tooltip("Maximum default height used for snapping to the nearest surface")]
        [Enhanced, Min(0f)] public float SnapHeight             = .2f;

        // -----------------------

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Coefficient applied on an Movable force when hitting ground")]
        [Enhanced, Range(0f, 1f)] public float OnGroundedForceMultiplier = .55f;

        [Space(5f)]

        [Tooltip("Deceleration applied on an Movable force while on ground")]
        [Enhanced, Min(0f)] public float GroundForceDeceleration    = 17f;

        [Tooltip("Deceleration applied on an object force while in the air")]
        [Enhanced, Min(0f)] public float AirForceDeceleration       = 5f;
        #endregion
    }
}
