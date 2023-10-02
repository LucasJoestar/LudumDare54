// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54
{
    public class TrampolineTrigger : EnhancedBehaviour, IPlayerTrigger
    {
        #region Global Members
        [Section("Trampoline")]

        [SerializeField, Enhanced, Required] private Animator animator   = null;
        [SerializeField, Enhanced, Range(0f, 9f)] private float distance = 3f;
        #endregion

        #region Trigger
        private static readonly int action_Hash = Animator.StringToHash("Action");

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController _player) {

            if (_player.Jump(transform.position, distance)) {
                animator.Play(action_Hash, 0, 0f);
            }
        }

        public void OnPlayerTriggerExit(PlayerController _player) { }
        #endregion
    }
}
