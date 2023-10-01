// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using LudumDare54.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    public class AmmoCollectible : EnhancedBehaviour, IPlayerTrigger {
        #region Global Members
        [Section("Ammo Collectible")]

        [SerializeField, Enhanced, Required] private Ammunition ammunition = null;
        [SerializeField, Enhanced, Required] private Transform root        = null;

        [Space(10f)]

        [SerializeField, Enhanced, Range(0f, 25f)] private int amount      = 1;
        [SerializeField] private bool isAnimated = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float animDuration = 1f;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 movement = new Vector3(0f, .5f, 0f);
        [SerializeField, Enhanced, Range(0f, 2f)] private AnimationCurve movementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 scale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField, Enhanced, Range(0f, 2f)] private AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            if (isAnimated) {
                StartAnimation();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            StopAnimation();
        }
        #endregion

        #region Animation
        private Sequence idleSequence = null;
        private Vector3 position      = Vector3.zero;

        // -----------------------

        public void StartAnimation() {

            if (idleSequence.IsActive())
                return;

            if (position == Vector3.zero) {
                position = transform.position;
            } else {
                transform.position = position;
            }

            idleSequence = DOTween.Sequence();
            float duration = animDuration;

            Tween _movement = root.DOBlendableMoveBy(movement, duration).SetEase(movementCurve);
            Tween _scale    = root.DOScale(scale, duration).SetEase(scaleCurve);

            idleSequence.Append(_movement);
            idleSequence.Append(_scale);

            idleSequence.SetLoops(-1, LoopType.Restart).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnKill() {
                idleSequence = null;
            }
        }

        public void StopAnimation() {

            if (!idleSequence.IsActive())
                return;

            idleSequence.Kill();
        }
        #endregion

        #region Trigger
        private bool isCollected = false;

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController player) {

            if (isCollected)
                return;

            InventoryManager.Instance.AddAmmunition(ammunition, amount);
            isCollected = true;
        }

        public void OnPlayerTriggerExit(PlayerController player) { }
        #endregion
    }
}
