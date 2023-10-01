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

        public InventoryAmmunition(Ammunition _amuntion, int _count = 0) {
            Ammunition = _amuntion;
            Count = _count;
        }
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

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            InventoryUI.Instance.InitUI(ammunitions);
        }
        #endregion

        #region Utility
        public void AddAmmunition(Ammunition _ammunition, int _count) {

            InventoryAmmunition _resource = GetAmmunition(_ammunition);
            _resource.Count += _count;

            InventoryUI.Instance.UpdateUI(ammunitions);
        }

        public InventoryAmmunition GetAmmunition(Ammunition _ammunition) {

            for (int i = 0; i < ammunitions.Count; i++) {
                InventoryAmmunition _temp = ammunitions[i];

                if (_temp.Ammunition == _ammunition)
                    return _temp;
            }

            InventoryAmmunition _resource = new InventoryAmmunition(_ammunition, 0);
            ammunitions.Add(_resource);

            return _resource;
        }
        #endregion
    }
}
