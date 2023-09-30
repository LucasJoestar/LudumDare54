// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LudumDare54.UI {
    /// <summary>
    /// Project-specific <see cref="SceneManagerBehaviour"/>.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.Name)]
    public class LudumDareSceneManagerBehaviour : SceneManagerBehaviour<LudumDareLoadSceneSettings, UnloadSceneSettings> {
        #region Global Members
        /// <summary>
        /// Minimum interval duration for any loading screen (in seconds).
        /// </summary>
        [SerializeField, Enhanced, MinMax(0f, 10f)] private Vector2 loadingMinDuration = new Vector2(.5f, .55f);
        #endregion

        #region Loading
        private const int ChronosPriority = 999;
        private readonly int chronosID = EnhancedUtility.GenerateGUID();

        // -------------------------------------------
        // Settings
        // -------------------------------------------

        private static LudumDareLoadSceneSettings settings = new LudumDareLoadSceneSettings(LoadingMode.Black, LoadingChronos.None);
        private static double loadingMinimumDuration = 0d;

        /// <summary>
        /// Settings used for the current loading.
        /// </summary>
        public static LudumDareLoadSceneSettings Settings {
            get { return settings; }
        }

        /// <summary>
        /// Mode to be displayed during loading.
        /// </summary>
        public static LoadingMode Mode {
            get { return settings.Mode; }
        }

        /// <summary>
        /// Chronos effect applied during loading.
        /// </summary>
        public static LoadingChronos Chronos {
            get { return settings.Chronos; }
        }

        // -------------------------------------------
        // Processors
        // -------------------------------------------

        public override bool StartLoading {
            get { return LoadingUI.Instance.StartLoading(); }
        }

        public override double LoadingMinimumDuration {
            get { return loadingMinimumDuration; }
        }

        public override bool CompleteLoading {
            get { return LoadingUI.Instance.CompleteLoading(); }
        }

        // -------------------------------------------
        // Behaviour
        // -------------------------------------------

        protected override void PrepareLoading(in PairCollection<SceneBundle, LoadSceneMode> _loadingBundles, LudumDareLoadSceneSettings _settings) {
            base.PrepareLoading(in _loadingBundles, _settings);

            settings = _settings;

            // UI.
            LoadingUI.Instance.Prepare(_settings, _loadingBundles);

            // Chronos freeze.
            switch (Chronos) {
                case LoadingChronos.FreezeInstant:
                    ChronosManager.Instance.PushOverride(chronosID, 0f, ChronosPriority);
                    break;

                case LoadingChronos.FreezeOnFaded:
                case LoadingChronos.None:
                default:
                    break;
            }
        }

        public override void OnStartLoading() {
            base.OnStartLoading();

            // Get this loading minimum duration.
            loadingMinimumDuration = loadingMinDuration.Random();

            // UI.
            LoadingUI.Instance.Display();

            // Chronos freeze.
            switch (Chronos) {
                case LoadingChronos.FreezeOnFaded:
                    ChronosManager.Instance.PushOverride(chronosID, 0f, ChronosPriority);
                    break;

                case LoadingChronos.FreezeInstant:
                case LoadingChronos.None:
                default:
                    break;
            }
        }

        public override void OnLoadingState(LoadingState _state) {
            base.OnLoadingState(_state);

            // UI.
            LoadingUI.Instance.OnLoadingState(_state);
        }

        public override void OnLoadingReady() {
            base.OnLoadingReady();

            // UI.
            LoadingUI.Instance.StartToHide();
        }

        public override void OnStopLoading() {
            base.OnStopLoading();

            // UI.
            LoadingUI.Instance.CompleteHide();

            // Unpause when exiting the loading state.
            switch (Chronos) {
                case LoadingChronos.FreezeInstant:
                case LoadingChronos.FreezeOnFaded:
                    ChronosManager.Instance.PopOverride(chronosID);
                    break;

                case LoadingChronos.None:
                default:
                    break;
            }
        }

        // -------------------------------------------
        // Other
        // -------------------------------------------

        protected override void OnEnterPlayModeEditor(in PairCollection<SceneBundle, LoadSceneMode> _loadingBundles, LudumDareLoadSceneSettings _settings) {
            base.OnEnterPlayModeEditor(_loadingBundles, _settings);

            // Enter play mode settings.
            _settings.Mode = LoadingMode.ScreenArt;
            _settings.Chronos = LoadingChronos.FreezeInstant;
        }
        #endregion
    }
}
