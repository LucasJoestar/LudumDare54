// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace LudumDare54 {
    public class Block : EnhancedBehaviour {
        public override UpdateRegistration UpdateRegistration => base.UpdateRegistration | UpdateRegistration.Init;

        #region Global Members
        [Section("Block")]

        [SerializeField] private Vector2[] gridCells = new Vector2[] { Vector2.zero };

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private bool isInit = false;

        [Space(5f)]

        [SerializeField, Enhanced, ReadOnly] private SpriteRenderer[]  sprites    = new SpriteRenderer[0];
        [SerializeField, Enhanced, ReadOnly] private SpriteBehaviour[] behaviours = new SpriteBehaviour[0];

        // -----------------------

        private Sequence projectileSequence = null;
        private Sequence sequence = null;
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourEnabled() {
            base.OnBehaviourEnabled();

            LevelManager.RegisterBlock(this);
        }

        protected override void OnInit() {
            base.OnInit();

            isInit = true;
            transform.position = ProjectUtility.GetCoords(transform.position);
        }

        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            StopAnimation();
            LevelManager.UnregisterBlock(this);
        }

        protected override void OnValidate() {
            base.OnValidate();

            sprites    = GetComponentsInChildren<SpriteRenderer>();
            behaviours = GetComponentsInChildren<SpriteBehaviour>();
        }
        #endregion

        #region Preview
        public bool UpdatePreview(Vector2 _position) {

            // Availability.
            _position = ProjectUtility.GetCoords(_position);
            transform.position = _position;

            bool _available = LevelManager.Instance.IsAvailableCoords(_position);

            for (int i = 0; i < gridCells.Length; i++) {

                if (!LevelManager.Instance.IsAvailableCoords(_position + gridCells[i])) {
                    _available = false;
                    break;
                }
            }

            // Color.
            Color _color =_available ? VariousSettings.I.AvailableSpaceColor : VariousSettings.I.UnavailableSpaceColor;

            foreach (SpriteRenderer _sprite in sprites) {
                _sprite.color = _color;
            }

            foreach (SpriteBehaviour _behaviour in behaviours) {
                _behaviour.enabled = false;
                _behaviour.SetOrder(32766);
            }

            return _available;
        }

        public void SpawnWithProjectile(GameObject _projectile) {

            projectileSequence = DOTween.Sequence();

            // Animation.
            transform.localScale = Vector3.zero;
            _projectile.transform.localScale = Vector3.zero;

            sequence = DOTween.Sequence();

            float _magnitude = (transform.position - _projectile.transform.position).magnitude;
            float _duration = _magnitude * VariousSettings.I.SpellMovementSpeed;

            sequence.Append(_projectile.transform.DOJump(transform.position, VariousSettings.I.SpellMovementHeight, 1, _duration)
                    .SetEase(VariousSettings.I.SpellMovementEase));

            sequence.Join(_projectile.transform.DOBlendableRotateBy(new Vector3(0f, 0f, VariousSettings.I.SpellRotation * _magnitude), _duration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            sequence.Join(_projectile.transform.DOScale(1f, Mathf.Min(.2f, _duration)));
            sequence.SetAutoKill(true).SetRecyclable(true).OnComplete(OnComplete).OnKill(OnKill);

            // ----- Local Method ----- \\

            void OnComplete() {
                Spawn();
            }

            void OnKill() {

                projectileSequence = null;
                Destroy(_projectile.gameObject);
            }
        }

        public void Spawn() {

            enabled = true;
            StopAnimation();

            // Color.
            foreach (SpriteRenderer _sprite in sprites) {
                _sprite.DOColor(Color.white, .2f);
            }

            foreach (SpriteBehaviour _behaviour in behaviours) {
                _behaviour.enabled = true;
            }

            // Animation.
            transform.localScale = Vector3.zero;
            sequence = DOTween.Sequence();

            sequence.Append(transform.DOScale(1f, VariousSettings.I.SpawnPlatformDuration).SetEase(VariousSettings.I.SpawnPlatformEase));
            sequence.SetAutoKill(true).SetRecyclable(true).OnKill(OnKill);

            // FX.
            ParticleSystemAsset _particle = VariousSettings.I.SpawnPlatformFX;
            if (_particle != null) {
                _particle.Play(transform, FeedbackPlayOptions.PlayAtPosition);
            }

            // ----- Local Method ----- \\

            void OnKill() {
                sequence = null;
            }
        }
        #endregion

        #region Utility
        public bool IsOnCoords(Vector2 _coords) {

            Vector2 _worldPosition = ProjectUtility.GetCoords(transform.position.ToVector2());

            if (_worldPosition == _coords)
                return true;

            foreach (Vector2 _cell in gridCells) {
                if ((_worldPosition + _cell) == _coords)
                    return true;
            }

            return false;
        }

        private void StopAnimation() {

            if (sequence.IsActive()) {
                sequence.Kill();
            }

            if (projectileSequence.IsActive()) {
                projectileSequence.Kill();
            }
        }
        #endregion
    }
}
