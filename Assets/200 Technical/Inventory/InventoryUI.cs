// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    public class InventoryUI : EnhancedSingleton<InventoryUI> {
        #region Global Members
        [Section("Inventory UI")]

        [SerializeField, Enhanced, Required] private FadingGroupBehaviour group = null;
        #endregion

        #region Behaviour
        public void InitUI(List<InventoryAmmunition> _ammunitions) {

        }

        public void UpdateUI(List<InventoryAmmunition> _ammunitions) {

        }
        #endregion
    }
}
