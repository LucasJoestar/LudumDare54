// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using LudumDare54.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Range = UnityEngine.RangeAttribute;

namespace LudumDare54 {
    /// <summary>
    /// Level manager <see cref="EnhancedSingleton{T}"/> instance.
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class LevelManager : EnhancedSingleton<LevelManager> {
        #region Global Members
        [Section("Level Manager")]

        [SerializeField, Enhanced, Required] private Transform spawn = null;
        [SerializeField, Enhanced, Required] private SceneBundle nextLevel = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 10f)] private float delay     = 0f;

        [Space(10f)]

        [SerializeField] private LudumDareLoadSceneSettings settings = new LudumDareLoadSceneSettings();

        // -----------------------

        public Transform Spawn {
            get { return spawn; }
        }
        #endregion

        #region Enhanced Behaviour
        protected override void OnBehaviourDisabled() {
            base.OnBehaviourDisabled();

            loadDelay.Cancel();
        }
        #endregion

        #region Registration
        private static readonly List<Block> blocks = new List<Block>();

        // -----------------------

        public static void RegisterBlock(Block _block) {
            blocks.Add(_block);
        }

        public static void UnregisterBlock(Block _block) {
            blocks.Remove(_block);
        }
        #endregion

        #region Loading
        private DelayHandler loadDelay = default;
        private bool isLoading = false;

        // -----------------------

        /// <summary>
        /// Loads the next level.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.HarvestGold)]
        public bool LoadNextLevel(float _delay = 0f) {

            if (isLoading || loadDelay.IsValid)
                return false;

            loadDelay = Delayer.Call(delay + _delay, Load, true);
            return true;

            // ----- Local Method ----- \\

            void Load() {

                EnhancedSceneManager _sceneManager = EnhancedSceneManager.Instance;
                if (_sceneManager.LoadingState != LoadingState.Inactive)
                    return;

                _sceneManager.LoadSceneBundle(nextLevel, LoadSceneMode.Single, settings);
                isLoading = true;
            }
        }

        /// <summary>
        /// Reloads this level.
        /// </summary>
        [Button(ActivationMode.Play, SuperColor.Crimson)]
        public void ReloadLevel() {

            EnhancedSceneManager _sceneManager = EnhancedSceneManager.Instance;

            if (_sceneManager.LoadingState != LoadingState.Inactive)
                return;

            LudumDareLoadSceneSettings _settings = new LudumDareLoadSceneSettings(LoadingMode.Transition, LoadingChronos.FreezeOnFaded);
            int _sceneCount = _sceneManager.LoadedBundleCount;

            if (_sceneCount == 0)
                return;

            PlayerController.Instance.RemoveControl(true);

            for (int i = 0; i < _sceneCount; i++) {

                LoadSceneMode _mode = (i == 0) ? LoadSceneMode.Single : LoadSceneMode.Additive;
                _sceneManager.LoadSceneBundle(_sceneManager.GetLoadedBundleAt(i), _mode, _settings);
            }
        }
        #endregion

        #region Utility
        public bool IsAvailableCoords(Vector2 _coords) {

            _coords = ProjectUtility.GetCoords(_coords);

            foreach (Block _block in blocks) {
                if (_block.IsOnCoords(_coords))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
