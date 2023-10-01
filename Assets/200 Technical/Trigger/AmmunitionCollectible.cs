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
        [SerializeField, Enhanced, Range(0f, 25f)] private int amount      = 1;

        [Space(10f)]

        [SerializeField] private bool isAnimated = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float animDuration = 1f;
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

            position = transform.position;

            idleSequence = DOTween.Sequence();

            Tween _movement = null;
            Tween _scale    = null;

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
            transform.position = position;
        }
        #endregion

        #region Trigger
        private DelayHandler loadDelay = default;
        private bool isLoading = false;

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController player) { }

        public void OnPlayerTriggerExit(PlayerController player) { }
        #endregion
    }
}
