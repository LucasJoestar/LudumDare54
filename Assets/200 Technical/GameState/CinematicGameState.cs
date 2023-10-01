// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.GameStates;
using EnhancedFramework.Inputs;
using EnhancedFramework.Inputs.GameStates;
using System;

namespace LudumDare54.GameStates {
    /// <summary>
    /// <see cref="GameState"/> applied while any kind of cinematic is being played (like videos and timelines).
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Utility/Cinematic")]
    public class CinematicGameState : InputGameState<LudumDareGameStateOverride>, IBoundGameState<ISkippableElement> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.CinematicProprity; }
        }

        // True for displaying videos or timelines during loading (like during the splash).
        public override bool IsPersistent {
            get { return true; }
        }

        public override InputGameStateMode Mode {
            get { return InputGameStateMode.Creation; }
        }

        /// <summary>
        /// The <see cref="InputActionEnhancedAsset"/> used to skip this state bound element.
        /// </summary>
        public static InputActionEnhancedAsset SkipInput {
            get { return InputSettings.I.SkipInput; }
        }

        // -----------------------

        /// <summary>
        /// The <see cref="ISkippableElement"/> associated with this state.
        /// </summary>
        public ISkippableElement SkippableObject = null;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="CinematicGameState"/>
        public CinematicGameState() : base(SkipInput) { }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // Indicate that a cinematic is being played.
            _state.HasControl = false;
            _state.IsCinematic = true;
        }
        #endregion

        #region Bound
        public void Bound(ISkippableElement _object) {
            SkippableObject = _object;
        }
        #endregion

        #region Behaviour
        protected override void EnableInput() {

            // Don't enable input if not skippable.
            if ((SkippableObject == null) || !SkippableObject.IsSkippable) {
                return;
            }

            base.EnableInput();

            // Skip delegate.
            SkipInput.OnPerformed += Skip;
        }

        protected override void DisableInput() {
            base.DisableInput();

            // Skip delegate.
            SkipInput.OnPerformed -= Skip;
        }

        // -----------------------

        private void Skip(InputActionEnhancedAsset _input) {
            SkippableObject.Skip();
        }
        #endregion
    }

    /// <summary>
    /// <see cref="GameState"/> applied while a background cinematic is being played (like during a conversation).
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Utility/Background Cinematic")]
    public class BackgroundCinematicGameState : CinematicGameState {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.BackroundCinematicProprity; }
        }

        public override bool IsPersistent {
            get { return false; }
        }
        #endregion
    }
}
