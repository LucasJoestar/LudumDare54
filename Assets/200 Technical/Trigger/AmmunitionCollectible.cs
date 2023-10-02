// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    public class AmmoCollectible : EnhancedBehaviour, IPlayerTrigger {
        #region Global Members
        [Section("Ammo Collectible")]

        [SerializeField, Enhanced, Required] private Ammunition ammunition = null;
        [SerializeField, Enhanced, Required] private Animator animator     = null;

        [Space(5f)]

        [SerializeField, Enhanced, Required] private Transform root        = null;
        [SerializeField, Enhanced, Required] private Transform bubble      = null;
        [SerializeField, Enhanced, Required] private Transform item        = null;
        [SerializeField, Enhanced, Required] private Transform shadow       = null;

        [Space(10f)]

        [SerializeField, Enhanced, Range(0f, 25f)] private int amount      = 1;
        [SerializeField] private bool isAnimated = false;
        [SerializeField] private bool instantCollect = false;

        [Space(10f)]

        [SerializeField] private ParticleSystemAsset fx = null;

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
        [SerializeField] private Ease burstMovementEase = Ease.OutSine;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 burstScale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField] private Ease burstScaleEase = Ease.OutSine;


        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float collectDuration = 1f;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 collectMovement = new Vector3(0f, .5f, 0f);
        [SerializeField] private Ease collectMovementEase = Ease.OutSine;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 2f)] private Vector3 collectScale = new Vector3(.9f, 1.1f, 1f);
        [SerializeField] private Ease collectScaleEase = Ease.OutSine;

        [Space(5f)]

        [SerializeField, Enhanced, Range(-45f, 45f)] private float collectRotation = -9f;
        [SerializeField] private AnimationCurve collectRotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
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

            root.ResetLocal();
            bubble.ResetLocal();
            item.ResetLocal();
        }
        #endregion

        #region Animation
        private static readonly int burs_Hash = Animator.StringToHash("Burst");

        private Sequence sequence = null;
        private Vector3 position  = Vector3.zero;

        // -------------------------------------------
        // Animation
        // -------------------------------------------

        public void PlayIdleAnimation() {

            if (sequence.IsActive())
                return;

            if (position == Vector3.zero) {
                position = root.position;
            } else {
                root.position = position;
            }

            sequence = DOTween.Sequence();
            float duration = idleDuration;

            Tween _movement = root.DOBlendableMoveBy(idleMovement, duration).SetEase(idleMovementCurve);
            Tween _scale    = root.DOScale(idleScale, duration).SetEase(idleScaleCurve);

            sequence.Append(_movement);
            sequence.Join(_scale);

            sequence.SetLoops(-1, LoopType.Restart).SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnKill() {
                sequence = null;
            }
        }

        public void PlayCollectAnimation() {

            if (sequence.IsActive())
                return;

            sequence = DOTween.Sequence();

            // Burst.
            Sequence _burst = DOTween.Sequence();
            {
                float duration = burstDuration;

                Tween _movement = bubble.DOBlendableMoveBy(burstMovement, duration).SetEase(burstMovementEase);
                Tween _scale    = bubble.DOScale(burstScale, duration).SetEase(burstScaleEase);
                Tween _scale2   = shadow.DOScale(0f, duration).SetEase(burstScaleEase);

                _burst.Append(_movement);
                _burst.Join(_scale);
                _burst.Join(_scale2);
            }


            // Collect.
            Sequence _collect = DOTween.Sequence();
            {
                float duration = collectDuration;

                Tween _movement = item.DOBlendableMoveBy(collectMovement, duration).SetEase(collectMovementEase);
                Tween _scale    = item.DOScale(collectScale, duration).SetEase(collectScaleEase);
                Tween _rotation = item.DOBlendableRotateBy(new Vector3(0f, 0f, collectRotation), duration, RotateMode.LocalAxisAdd).SetEase(collectRotationCurve);

                _collect.Append(_movement);
                _collect.Join(_scale);
                _collect.Join(_rotation);
            }

            // Final.
            sequence.Append(_burst);
            sequence.Join(_collect);
            sequence.Join(root.DOScale(1f, .2f));

            animator.Play(burs_Hash, 0, 0f);

            sequence.SetRecyclable(true).SetAutoKill(true).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnKill() {

                if (!instantCollect) {
                    AddToInventory();
                }

                sequence = null;
                Destroy(gameObject);
            }
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

        #region Collect
        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public void Collect() {

            if (isCollected)
                return;

            StopAnimation();
            PlayCollectAnimation();

            if (fx != null) {
                fx.Play(bubble, FeedbackPlayOptions.PlayAtPosition);
            }

            if (instantCollect) {
                AddToInventory();
            }

            isCollected = true;
        }

        private void AddToInventory() {

            InventoryManager _inventory = InventoryManager.Instance;
            if (_inventory != null) {
                _inventory.AddAmmunition(ammunition, amount);
            }
        }
        #endregion

        #region Trigger
        private bool isCollected = false;

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController _player) {
            Collect();
        }

        public void OnPlayerTriggerExit(PlayerController _player) { }
        #endregion
    }
}
