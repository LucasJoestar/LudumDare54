// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LudumDare54.UI {
    /// <summary>
    /// Singleton class used to perform loading-related UI animations and transitions.
    /// </summary>
    [ScriptGizmos(false, true)]
    [AddComponentMenu(ProjectUtility.MenuPath + "UI/Loading UI"), DisallowMultipleComponent]
    public class LoadingUI : FadingObjectSingleton<LoadingUI>, ILoadingProcessor {
        #region State
        /// <summary>
        /// Lists all possible display states of this group.
        /// </summary>
        public enum State {
            Inactive = 0,
            Prepare,
            Ready,
            Show,
            Active,
            Processor,
            Hide,
            Complete
        }
        #endregion

        #region Loading Processor
        public override bool IsLoadingProcessor => true;

        public bool IsProcessing {
            get {
                // Don't process until the end of loading.
                if (CurrentState != State.Processor) {
                    return false;
                }

                if (delay.IsValid) {
                    return true;
                }

                switch (Settings.Mode) {

                    // True while playing.
                    case LoadingMode.Cutscene:
                        return Settings.Cutscene.IsPlaying;

                    case LoadingMode.Video:
                        return Settings.Video.IsPlaying;

                    // Inactive.
                    case LoadingMode.ScreenArt:
                    case LoadingMode.Black:
                    case LoadingMode.Default:
                    default:
                        return false;
                }
            }
        }
        #endregion

        #region Global Members
        [Section("Loading")]

        [SerializeField, Enhanced, Required] private Image loadingScreen            = null;
        [SerializeField, Enhanced, Required] private Image cutsceneRender           = null;

        [Space(5f)]

        [SerializeField, Enhanced, Required] private Sprite defaultLoadingScreen    = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, ReadOnly] private State state = State.Inactive;

        // -----------------------

        /// <inheritdoc cref="LudumDareSceneManagerBehaviour.Settings"/>
        private LudumDareLoadSceneSettings Settings {
            get { return LudumDareSceneManagerBehaviour.Settings; }
        }

        /// <summary>
        /// The current display state of this loading UI.
        /// </summary>
        public State CurrentState {
            get { return state; }
        }

        public TransitionFadingGroupBehaviour TransitionGroup {
            get { return fadingObject as TransitionFadingGroupBehaviour; }
        }
        #endregion

        #region Behaviour
        private DelayHandler delay = default;

        // -----------------------

        /// <summary>
        /// Starts to show this loading.
        /// </summary>
        /// <param name="_settings">Settings of this loading.</param>
        /// <param name="_loadingBundles">Loading bundle(s).</param>
        public void Prepare(LudumDareLoadSceneSettings _settings, in PairCollection<SceneBundle, LoadSceneMode> _loadingBundles) {
            switch (_settings.Mode) {

                // Get and display the loading screen.
                case LoadingMode.ScreenArt:
                    Sprite _screen;

                    if (_loadingBundles.Count != 0) {
                        int _index = Mathf.Max(0, _loadingBundles.FindIndex(p => !p.First.IsCoreBundle()));
                        SceneBundle _bundle = _loadingBundles[_index].First;

                        if (_bundle.GetBehaviour(out LudumDareSceneBundleBehaviour _behaviour)) {
                            _screen = _behaviour.LoadingScreen;
                        } else {
                            this.LogErrorMessage($"Invalid Scene Bundle Behaviour ({_bundle.Behaviour.GetType().Name.Bold()})", _bundle);

                            _screen = defaultLoadingScreen;
                        }
                    } else {
                        _screen = defaultLoadingScreen;
                    }

                    loadingScreen.sprite = _screen;

                    cutsceneRender.enabled = false;
                    loadingScreen.enabled = true;

                    break;

                // Only display cutscene.
                case LoadingMode.Cutscene:

                    cutsceneRender.enabled = true;
                    loadingScreen.enabled = false;

                    break;

                // Don't display anything.
                case LoadingMode.Video:
                case LoadingMode.Black:
                case LoadingMode.Default:
                default:

                    cutsceneRender.enabled = false;
                    loadingScreen.enabled = false;

                    break;
            }

            SetState(State.Prepare);
            TransitionGroup.StartFadeIn(null, OnComplete);

            // ----- Local Method ----- \\

            void OnComplete() {
                SetState(State.Ready);
            }
        }

        /// <summary>
        /// Displays this loading.
        /// </summary>
        public void Display() {
            switch (Settings.Mode) {

                // Play cutscene.
                case LoadingMode.Cutscene:
                    var _cutscene = Settings.Cutscene;

                    #if DEVELOPMENT
                    // Security.
                    if (_cutscene.IsNull()) {
                        this.LogErrorMessage("Invalid Cutscene");
                        break;
                    }
                    #endif

                    _cutscene.transform.SetParent(null, true);
                    DontDestroyOnLoad(_cutscene.gameObject);

                    delay = Delayer.Call(Settings.Delay, PlayCutscene, true);

                    // ----- Local Method ----- \\

                    void PlayCutscene() {

                        _cutscene.Play();
                    }

                    break;

                // Play video.
                case LoadingMode.Video:
                    var _video = Settings.Video;

                    #if DEVELOPMENT
                    // Security.
                    if (_video.IsNull()) {
                        this.LogErrorMessage("Invalid Video");
                        break;
                    }
                    #endif

                    _video.transform.SetParent(null, true);
                    DontDestroyOnLoad(_video.gameObject);

                    delay = Delayer.Call(Settings.Delay, PlayVideo, true);

                    // ----- Local Method ----- \\

                    void PlayVideo() {
                        _video.Play();
                    }
                    break;

                case LoadingMode.ScreenArt:
                case LoadingMode.Black:
                case LoadingMode.Default:
                default:
                    break;
            }

            SetState(State.Show);
            TransitionGroup.CompleteFade(OnComplete);

            // ----- Local Method ----- \\

            void OnComplete() {
                SetState(State.Active);
            }
        }

        // -----------------------

        /// <summary>
        /// Starts hiding this group.
        /// </summary>
        public void StartToHide() {
            SetState(State.Hide);
            TransitionGroup.StartFadeOut(OnFaded);

            // ----- Local Method ----- \\

            void OnFaded() {
                SetState(State.Complete);
            }
        }

        /// <summary>
        /// Completes this group current fade operation.
        /// </summary>
        public void CompleteHide() {
            switch (Settings.Mode) {

                // Destroy loading objects.
                case LoadingMode.Cutscene:
                    Destroy(Settings.Cutscene.gameObject);
                    break;

                case LoadingMode.Video:
                    Destroy(Settings.Video.gameObject);
                    break;

                case LoadingMode.ScreenArt:
                case LoadingMode.Black:
                case LoadingMode.Default:
                default:
                    break;
            }

            SetState(State.Inactive);
            TransitionGroup.CompleteFade();
        }

        // -------------------------------------------
        // Callback
        // -------------------------------------------

        /// <summary>
        /// Called whenever the current <see cref="LoadingState"/> changes.
        /// </summary>
        /// <param name="_state">Current <see cref="LoadingState"/>.</param>
        public void OnLoadingState(LoadingState _state) {
            if (_state == LoadingState.WaitForInitialization) {
                SetState(State.Processor);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Whether the loading is ready to start or not.
        /// </summary>
        /// <returns>True if the loading is ready to start, false otherwise.</returns>
        public bool StartLoading() {
            return state == State.Ready;
        }

        /// <summary>
        /// Whether the loading is ready to be compeleted or not.
        /// </summary>
        /// <returns>True if the loading is ready to be completed, false otherwise.</returns>
        public bool CompleteLoading() {
            return state == State.Complete;
        }

        /// <summary>
        /// Set this loading state.
        /// </summary>
        /// <param name="_state">Current loading state.</param>
        private void SetState(State _state) {
            state = _state;
        }
        #endregion
    }
}
