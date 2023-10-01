// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LudumDare54 {
    /// <summary>
    /// <see cref="InventoryUI"/> single ammunition controller.
    /// </summary>
    public class AmmunitionUI : EnhancedBehaviour {
        #region Global Members
        [Section("Ammunition UI")]

        [SerializeField, Enhanced, Required] private Image image = null;
        [SerializeField, Enhanced, Required] private Image outline = null;
        [SerializeField, Enhanced, Required] private TextMeshProUGUI text = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private bool isActive   = false;
        [SerializeField, Enhanced, ReadOnly] private bool isSelected = false;

        [Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private Ammunition ammunition = null;

        // -----------------------

        private RectTransform group = null;
        private TweenCallback refreshGroupDelegate = null;

        // -----------------------

        public bool IsActive {
            get { return gameObject.activeInHierarchy; }
        }

        public Ammunition Ammunition {
            get { return ammunition; }
        }
        #endregion

        #region Enhanced Behaviour
        private void Awake() {

            group = GetComponentInParent<HorizontalLayoutGroup>().GetComponent<RectTransform>();
            refreshGroupDelegate = RefreshCanvasGrouo;
        }
        #endregion

        #region Behaviour
        private Sequence sequence = null;

        // -------------------------------------------
        // Activation
        // -------------------------------------------

        public void Activate(InventoryAmmunition _ammunition, bool _instant) {

            Unselect(_instant);

            image.sprite = _ammunition.Ammunition.Icon;
            outline.sprite = _ammunition.Ammunition.Icon;
            text.text = _ammunition.Count.ToString("0");

            ammunition = _ammunition.Ammunition;

            // Already active.
            if (isActive) {

                if (_instant) {
                    CompleteAnimation();
                }

                return;
            }

            gameObject.SetActive(true);
            isActive = true;

            // Animation.
            CancelAnimation();
            transform.localScale = Vector3.zero;

            sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(1f, VariousSettings.I.UI_ActivateScaleDuration).SetEase(VariousSettings.I.UI_ActivateScaleEase));
            sequence.OnUpdate(refreshGroupDelegate).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // Instant.
            if (_instant) {
                CompleteAnimation();
            }

            // ----- Local Method ----- \\

            void OnKill() {

                sequence = null;
            }
        }

        public void Deactivate(bool _instant) {

            Vector3 _scale = transform.localScale;
            Unselect(true);

            // Already Inactive.
            if (!isActive) {

                if (_instant) {
                    CompleteAnimation();
                }

                return;
            }

            gameObject.SetActive(false);
            isActive = false;

            // Animation.
            CancelAnimation();
            transform.localScale = _scale;

            sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(0f, VariousSettings.I.UI_DeactivateScaleDuration).SetEase(VariousSettings.I.UI_DeactivateScaleEase));
            sequence.OnUpdate(refreshGroupDelegate).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // Instant.
            if (_instant) {
                CompleteAnimation();
            }

            // ----- Local Method ----- \\

            void OnKill() {

                sequence = null;
                gameObject.SetActive(false);
            }
        }

        // -------------------------------------------
        // Selection
        // -------------------------------------------

        public void Select(bool _instant) {

            // Already selected.
            if (isSelected) {

                if (_instant) {
                    CompleteAnimation();
                }

                return;
            }

            CancelAnimation();
            isSelected = true;

            text.fontMaterial = VariousSettings.I.UI_SelectFont;
            outline.color = VariousSettings.I.UI_SelectColor;

            // Animation.
            sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(VariousSettings.I.UI_SelectScale, VariousSettings.I.UI_SelectScaleDuration).SetEase(VariousSettings.I.UI_SelectScaleEase));
            sequence.OnUpdate(refreshGroupDelegate).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // Instant.
            if (_instant) {
                CompleteAnimation();
            }

            // ----- Local Method ----- \\

            void OnKill() {
                sequence = null;
            }
        }

        public void Unselect(bool _instant) {

            // Already unselected.
            if (!isSelected) {

                if (_instant) {
                    CompleteAnimation();
                }

                return;
            }

            CancelAnimation();
            isSelected = false;

            text.fontMaterial = VariousSettings.I.UI_UnselectFont;
            outline.color = VariousSettings.I.UI_UnselectColor;

            // Animation.
            sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(1f, VariousSettings.I.UI_UnselectScaleDuration).SetEase(VariousSettings.I.UI_UnselectScaleEase));
            sequence.OnUpdate(refreshGroupDelegate).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // Instant.
            if (_instant) {
                CompleteAnimation();
            }

            // ----- Local Method ----- \\

            void OnKill() {
                sequence = null;
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        private void CancelAnimation() {

            if (sequence.IsActive()) {
                sequence.Kill();
            }
        }

        private void CompleteAnimation() {

            if (sequence.IsActive()) {
                sequence.Complete(true);
            }
        }

        private void RefreshCanvasGrouo() {
            //Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(group);
        }
        #endregion
    }
}
