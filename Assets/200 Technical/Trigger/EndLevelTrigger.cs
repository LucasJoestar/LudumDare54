// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using LudumDare54.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

using Range = UnityEngine.RangeAttribute;

namespace LudumDare54
{
    public class EndLevelTrigger : EnhancedBehaviour, IPlayerTrigger
    {
        #region Global Members
        [Section("End Level Trigger")]

        [SerializeField, Enhanced, Required] private SceneBundle nextScene = null;
        [SerializeField, Enhanced, Range(0f, 10f)] private float delay     = 0f;

        [Space(10f)]

        [SerializeField] private LudumDareLoadSceneSettings settings = new LudumDareLoadSceneSettings();
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            loadDelay.Cancel();
        }
        #endregion

        #region Trigger
        private DelayHandler loadDelay = default;
        private bool isLoading = false;

        // -----------------------

        public void OnPlayerTriggerEnter(PlayerController player) {

            if (isLoading || loadDelay.IsValid)
                return;

            player.RemoveControl(true);
            loadDelay = Delayer.Call(delay, Load, true);

            // ----- Local Method ----- \\

            void Load() {

                EnhancedSceneManager _sceneManager = EnhancedSceneManager.Instance;
                if (_sceneManager.LoadingState != LoadingState.Inactive)
                    return;

                _sceneManager.LoadSceneBundle(nextScene, LoadSceneMode.Single, settings);
                isLoading = true;
            }
        }

        public void OnPlayerTriggerExit(PlayerController player) { }
        #endregion
    }
}
