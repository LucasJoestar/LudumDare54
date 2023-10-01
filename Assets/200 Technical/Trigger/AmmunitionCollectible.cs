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
        [SerializeField, Enhanced, Required] private Transform bubble      = null;
        [SerializeField, Enhanced, Required] private Transform item        = null;

        [Space(10f)]

        [SerializeField, Enhanced, Range(0f, 25f)] private int amount      = 1;
        [SerializeField] private bool isAnimated = false;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float idleDuration = 1f;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 idleMovement = new Vector3(0f, .5f, 0f);
        [SerializeField] private AnimationCurve idleMovementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 idleScale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField] private AnimationCurve idleScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float burstDuration = 1f;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 burstMovement = new Vector3(0f, .5f, 0f);
        [SerializeField] private AnimationCurve burstMovementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 burstScale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField] private AnimationCurve burstScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float collectDuration = 1f;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 collectMovement = new Vector3(0f, .5f, 0f);
        [SerializeField] private AnimationCurve collectMovementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(5f)]

        [SerializeField, Enhanced, Range(-45f, 45f)] private float collectRotation = -9f;
        [SerializeField] private AnimationCurve collectRotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 collectScale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField] private AnimationCurve collectScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            if (isAnimated) {
                PlayIdleAnimation();
            }
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            StopAnimation();
        }
        #endregion

        #region Animation
        private Sequence sequence = null;
        private Vector3 position  = Vector3.zero;

        // -------------------------------------------
        // Animation
        // -------------------------------------------

        public void PlayIdleAnimation() {

            if (sequence.IsActive())
                return;

            if (position == Vector3.zero) {
                position = transform.position;
            } else {
                transform.position = position;
            }

            sequence = DOTween.Sequence();
            float duration = idleDuration;

            Tween _movement = root.DOBlendableMoveBy(idleMovement, duration).SetEase(idleMovementCurve);
            Tween _scale    = root.DOScale(idleScale, duration).SetEase(idleScaleCurve);

            sequence.Append(_movement);
            sequence.Append(_scale);

            sequence.SetLoops(-1, LoopType.Restart).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnKill() {
                sequence = null;
            }
        }

        public void PlayCollectAnimation() {

            StopAnimation();
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        public void StopAnimation() {

            if (!sequence.IsActive())
                return;

            sequence.Kill();
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
