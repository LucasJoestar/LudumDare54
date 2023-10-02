// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedFramework.Core;

namespace LudumDare54
{
    public class EndLevelTrigger : EnhancedBehaviour, IPlayerTrigger
    {
        #region Trigger
        private bool isLoading = false;

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController _player) {

            if (isLoading)
                return;

            _player.RemoveControl(true);
            float _delay = _player.Despawn(transform.position);

            isLoading = LevelManager.Instance.LoadNextLevel(_delay);
        }

        public void OnPlayerTriggerExit(PlayerController _player) { }
        #endregion
    }
}
