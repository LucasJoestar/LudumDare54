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
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Inventory")]

        [SerializeField] private List<InventoryAmmunition> ammunitions = new List<InventoryAmmunition>();

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private int selectedAmmunition = 0;

        // -----------------------

        public int AmmunitionCount {
            get { return ammunitions.Count; }
        }

        public int SelectedAmmunition {
            get {
                selectedAmmunition = Mathf.Clamp(selectedAmmunition, 0, ammunitions.Count - 1);
                return selectedAmmunition;
            }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnInit() {
            base.OnInit();

            InventoryUI.Instance.UpdateUI(ammunitions, SelectedAmmunition, true);
        }
        #endregion

        #region Selection
        public bool GetSelectedIndex(out InventoryAmmunition _ammunition) {

            if (ammunitions.Count != 0) {

                _ammunition = ammunitions[SelectedAmmunition];
                return true;
            }

            _ammunition = null;
            return false;
        }

        public int IncrementSelectedIndex(int _increment) {
            
            if (ammunitions.Count == 0) {
                selectedAmmunition = -1;
            } else {
                selectedAmmunition = Mathm.LoopIncrement(selectedAmmunition, ammunitions.Count, -_increment);
            }

            int _index = SelectedAmmunition;
            InventoryUI.Instance.Select(selectedAmmunition);

            return _index;
        }

        public int SetSelectedIndex(int _index) {

            if (ammunitions.Count == 0) {
                selectedAmmunition = -1;
            } else {
                selectedAmmunition = Mathf.Clamp(_index, 0, ammunitions.Count - 1);
            }

            _index = SelectedAmmunition;
            InventoryUI.Instance.Select(selectedAmmunition);

            return _index;
        }
        #endregion

        #region Utility
        [Button(ActivationMode.Play, SuperColor.HarvestGold)]
        public void AddAmmunition(Ammunition _ammunition, int _count) {

            InventoryAmmunition _resource = GetAmmunition(_ammunition);
            _resource.Count += _count;

            InventoryUI.Instance.UpdateUI(ammunitions, SelectedAmmunition, false);
        }

        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public void RemoveAmmunition(Ammunition _ammunition, int _count) {

            for (int i = 0; i < ammunitions.Count; i++) {
                InventoryAmmunition _temp = ammunitions[i];

                if (_temp.Ammunition == _ammunition) {

                    _temp.Count -= _count;
                    if (_temp.Count <= 0) {
                        ammunitions.RemoveAt(i);
                    }

                    InventoryUI.Instance.UpdateUI(ammunitions, SelectedAmmunition, false);
                    return;
                }
            }
        }

        [Button(ActivationMode.Play, SuperColor.Lavender)]
        public void Clear() {

            ammunitions.Clear();
            InventoryUI.Instance.UpdateUI(ammunitions, SelectedAmmunition, false);
        }

        // -----------------------

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
