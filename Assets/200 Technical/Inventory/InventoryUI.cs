// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LudumDare54 {
    /// <summary>
    /// <see cref="InventoryManager"/>-related UI.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class InventoryUI : EnhancedSingleton<InventoryUI> {
        #region Global Members
        [Section("Inventory UI")]

        [SerializeField, Enhanced, Required] private FadingObjectBehaviour group = null;
        [SerializeField, Enhanced, Required] private AmmunitionUI prefab         = null;
        [SerializeField, Enhanced, Required] private RectTransform root          = null;

        [Space(10)]

        [SerializeField] private List<AmmunitionUI> ammunitions = new List<AmmunitionUI>();
        [SerializeField] private List<AmmunitionUI> pool        = new List<AmmunitionUI>();
        #endregion

        #region Enhanced Behaviour
        private void Awake() {

            foreach (Transform _child in root) {
                _child.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Behaviour
        private static readonly List<AmmunitionUI> buffer = new List<AmmunitionUI>();

        // -----------------------

        public void UpdateUI(List<InventoryAmmunition> _ammunitions, int _selectedIndex, bool _instant = false) {

            buffer.Clear();
            buffer.AddRange(ammunitions);
            ammunitions.Clear();

            // Content update.
            for (int i = 0; i < _ammunitions.Count; i++) {

                InventoryAmmunition _ammunition = _ammunitions[i];
                AmmunitionUI _ui = GetAmmunition(_ammunition);

                _ui.Activate(_ammunition, _instant);
                ammunitions.Add(_ui);
            }

            // Deactive unused.
            for (int i = 0; i < buffer.Count; i++) {

                AmmunitionUI _instance = buffer[i];
                _instance.Deactivate(_instant);

                pool.Add(_instance);
            }

            buffer.Clear();

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);

            // Update selection.
            Select(_selectedIndex, _instant);

            // ----- Local Methods ----- \\

            AmmunitionUI GetAmmunition(InventoryAmmunition _ammunition) {

                for (int i = 0; i < buffer.Count; i++) {

                    AmmunitionUI _ui = buffer[i];

                    if (_ui.Ammunition == _ammunition.Ammunition) {

                        buffer.RemoveAt(i);
                        return _ui;
                    }
                }

                return GetUI();
            }
        }

        private AmmunitionUI GetUI() {

            // Get from pool.
            for (int i = pool.Count; i-- > 0;) {

                AmmunitionUI _ui = pool[i];
                if (!_ui.IsActive) {

                    pool.RemoveAt(i);
                    _ui.transform.SetAsLastSibling();

                    return _ui;
                }
            }

            // Create new instance.
            AmmunitionUI _instance = Instantiate(prefab, root);
            _instance.transform.ResetLocal();
            _instance.name = $"ITEM {ammunitions.Count:00}";

            _instance.transform.SetAsLastSibling();
            return _instance;
        }

        // -------------------------------------------
        // Selection
        // -------------------------------------------

        public void Select(int _selectedIndex, bool _instant = false) {

            _selectedIndex = (ammunitions.Count != 0) ? Mathf.Clamp(_selectedIndex, 0, ammunitions.Count - 1) : -1;

            for (int i = 0; i < ammunitions.Count; i++) {

                if (i == _selectedIndex) {
                    ammunitions[i].Select(_instant);
                } else {
                    ammunitions[i].Unselect(_instant);
                }
            }
        }
        #endregion
    }
}
