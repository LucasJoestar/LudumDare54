// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    /// <summary>
    /// <see cref="InventoryManager"/>-related ammunition infos.
    /// </summary>
    [Serializable]
    public class InventoryAmmunition {
        #region Content
        public Ammunition Ammunition = null;
        public int Count             = 0;
        #endregion
    }

    /// <summary>
    /// Inventory singleton instance.
    /// </summary>
    [ScriptGizmos(false, true)]
    [DefaultExecutionOrder(999)]
    [AddComponentMenu(ProjectUtility.MenuPath + "Core/Inventory"), SelectionBase, DisallowMultipleComponent]
    public class InventoryManager : EnhancedSingleton<InventoryManager> {
        #region Global Members
        [Section("Inventory")]

        [SerializeField] private List<InventoryAmmunition> ammunitions = new List<InventoryAmmunition>();
        #endregion

        #region Utility

        #endregion
    }
}
