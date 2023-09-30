// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// <see cref="Movable3D"/>-related class used to perform root motion as instant movement,
    /// according to the current animator state.
    /// </summary>
    [ScriptGizmos(false, true)]
    [RequireComponent(typeof(EnhancedAnimatorHandler))]
    [AddComponentMenu(FrameworkUtility.MenuPath + "Physics 3D/Movable 3D Root Motion"), DisallowMultipleComponent]
    public class Movable3DRootMotion : EnhancedBehaviour {
        #region Global Members
        [Section("Root Motion")]

        [SerializeField, Enhanced, Required] private Movable3D movable = null;

        // -----------------------

        [SerializeField, HideInInspector] private EnhancedAnimatorHandler animator = null;
        #endregion

        #region Enhanced Behaviour
        private void OnAnimatorMove() {
            UpdateRootMotion();
        }

        // -------------------------------------------
        // Editor
        // -------------------------------------------

        #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            if (!animator) {
                animator = GetComponent<EnhancedAnimatorHandler>();
            }

            if (!movable) {
                movable = GetComponentInParent<Movable3D>();
            }
        }
#endif
        #endregion

        #region Root Motion
        /// <summary>
        /// Updates this object root motion.
        /// <para/>
        /// Can be called in editor.
        /// </summary>
        public void UpdateRootMotion() {

            EnhancedAnimatorController _controller = animator.Controller;
            Animator _animator = animator.Animator;

            int _layerCount = Mathf.Min(_controller.LayerCount, _animator.layerCount);

            for (int i = 0; i < _layerCount; i++) {

                EnhancedAnimatorLayer _layer = _controller.GetLayer(i);
                if (!_layer.RootMotion) {
                    continue;
                }

                AnimatorStateInfo _currentInfo  = _animator.GetCurrentAnimatorStateInfo(i);
                AnimatorStateInfo _nextInfo     = _animator.GetNextAnimatorStateInfo(i);

                int _currentHash    = _currentInfo.shortNameHash;
                int _nextHash       = _nextInfo.shortNameHash;

                EnhancedAnimatorState _current;
                EnhancedAnimatorState _next;

                // Identify the active states.
                if (_layer.GetState(_currentHash, out _current) && _current.RootMotion) {

                    // Current state is enabled - determine next state.
                    if (!_layer.GetState(_nextHash, out _next) || _next.RootMotion) {
                        _next = _current;
                    }

                } else if (_layer.GetState(_nextHash, out _current) && _current.RootMotion) {

                    // Only next state is enabled.
                    _next = _current;

                } else {

                    // No active state enabled.
                    continue;
                }

                PerformRootMotion(_animator, _current, _next);
            }
        }

        /// <summary>
        /// Performs root motion according to a specific state.
        /// </summary>
        /// <param name="_current">Current motion state.</param>
        /// <param name="_next">Next motion state.</param>
        private void PerformRootMotion(Animator _animator, EnhancedAnimatorState _current, EnhancedAnimatorState _next) {

            Vector3 _positionMotion = _animator.deltaPosition.RotateInverse(movable.Transform.rotation);
            Quaternion _rotationMotion = _animator.deltaRotation;

            _current.ApplyMotion(_next, ref _positionMotion, ref _rotationMotion);
            _positionMotion = _positionMotion.Rotate(movable.Transform.rotation);

            #if UNITY_EDITOR
            // Editor motion.
            if (!Application.isPlaying) {

                movable.transform.position += _positionMotion;
                movable.transform.rotation *= _rotationMotion;

                return;
            }
            #endif

            movable.AddInstantMovementVelocity(_positionMotion);
            movable.OffsetRotation(_rotationMotion);
        }
        #endregion
    }
}
