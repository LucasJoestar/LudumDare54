// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    /// <summary>
	/// <see cref="ScriptableObject"/> used to define an ammunitions.
	/// </summary>
    [CreateAssetMenu(fileName = "AMO_Ammunition", menuName = ProjectUtility.MenuPath + "Ammunition", order = ProjectUtility.MenuOrder)]
    public class Ammunition : ScriptableObject {
        #region Global Members
        [Section("Ammunition")]

        public GameObject Prefab = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        public Sprite Icon = null;
        public Color Color = SuperColor.Crimson.Get();
        #endregion
    }
}
