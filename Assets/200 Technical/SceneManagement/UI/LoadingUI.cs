// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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

                switch (Settings.Mode) {

                    // Inactive.
                    case LoadingMode.Transition:
                    case LoadingMode.Default:
                    default:
                        return false;
                }
            }
        }
        #endregion

        #region Global Members
        [Section("Loading")]

        [SerializeField, Enhanced, Required] private FadingObjectBehaviour group        = null;
        [SerializeField, Enhanced, Required] private RectTransform transitionTransform  = null;

        [Space(5f)]

        [SerializeField, Enhanced, Required] private RectTransform transitionAnchorIn   = null;
        [SerializeField, Enhanced, Required] private RectTransform transitionAnchorWait = null;
        [SerializeField, Enhanced, Required] private RectTransform transitionAnchorOut = null;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField] private float showDuration = 1f;
        [SerializeField] private float hideDuration = 1f;

        [Space(5f)]

        [SerializeField] private Ease showEase = Ease.Linear;
        [SerializeField] private Ease hideEase = Ease.Linear;

        [Space(10f)]

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
        #endregion

        #region Behaviour
        /// <summary>
        /// Starts to show this loading.
        /// </summary>
        /// <param name="_settings">Settings of this loading.</param>
        /// <param name="_loadingBundles">Loading bundle(s).</param>
        public void Prepare(LudumDareLoadSceneSettings _settings, in PairCollection<SceneBundle, LoadSceneMode> _loadingBundles) {

            SetState(State.Prepare);

            switch (_settings.Mode) {

                // Don't display anything.
                case LoadingMode.Transition:

                    group.Show(true);

                    transitionTransform.position = transitionAnchorIn.position;
                    transitionTransform.DOMove(transitionAnchorWait.position, showDuration).SetEase(showEase).OnComplete(OnComplete).SetUpdate(true);

                    break;

                case LoadingMode.Default:
                default:

                    group.Hide(true);
                    Show(false, OnComplete);

                    transitionTransform.position = transitionAnchorWait.position;

                    break;
            }

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

                case LoadingMode.Transition:
                case LoadingMode.Default:
                default:
                    break;
            }

            SetState(State.Show);
            SetState(State.Active);
        }

        // -----------------------

        /// <summary>
        /// Starts hiding this group.
        /// </summary>
        public void StartToHide() {
            SetState(State.Hide);
            SetState(State.Complete);
        }

        /// <summary>
        /// Completes this group current fade operation.
        /// </summary>
        public void CompleteHide() {

            switch (Settings.Mode) {

                // Don't display anything.
                case LoadingMode.Transition:

                    group.Show(true);

                    transitionTransform.position = transitionAnchorWait.position;
                    transitionTransform.DOMove(transitionAnchorOut.position, hideDuration).SetEase(hideEase).OnComplete(OnComplete).SetUpdate(true);

                    break;

                case LoadingMode.Default:
                default:

                    group.Hide(true);
                    Hide(false, OnComplete);

                    break;
            }

            SetState(State.Inactive);

            // ----- Local Method ----- \\

            void OnComplete() {
                //SetState(State.Inactive);
            }
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
