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
    /// Main gameplay sequence <see cref="GameState"/>, giving the control to the player for moving and interacting with the world.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Gameplay")]
    public class GameplayGameState : InputGameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.GameplayProprity; }
        }

        // Should only be created once and never removed from the stack while playing.
        public override bool IsPersistent {
            get { return true; }
        }

        public override InputGameStateMode Mode {
            get { return InputGameStateMode.Activation; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="GameplayGameState"/>
        public GameplayGameState() : base(InputSettings.I.PlayerMap) { }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // Give control to the player.
            // Might be overriden by states with a higher proprity.
            _state.HasControl = true;
            _state.CanPause = true;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="GameState"/> applied while having the controller of the player, but restraining to open the inventory or pause the game.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Cinematic/Cinematic Gameplay")]
    public class CinematicGameplayGameState : GameplayGameState, IBoundGameState<ISkippableElement> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.ActiveGameplayProprity; }
        }

        public override bool IsPersistent {
            get { return false; }
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

        /// <inheritdoc cref="CinematicGameplayGameState"/>
        public CinematicGameplayGameState() : base() { }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // Restrain menus.
            _state.CanPause = false;
        }
        #endregion

        #region Bound
        public void Bound(ISkippableElement _object) {
            SkippableObject = _object;
        }
        #endregion

        #region Behaviour
        protected override void EnableInput() {
            base.EnableInput();

            // Don't enable input if not skippable.
            if ((SkippableObject == null) || !SkippableObject.IsSkippable) {
                return;
            }

            // Skip delegate.
            SkipInput.Enable();
            SkipInput.OnPerformed += Skip;
        }

        protected override void DisableInput() {
            base.DisableInput();

            // Skip delegate.
            SkipInput.OnPerformed -= Skip;
            SkipInput.Disable();
        }

        // -----------------------

        private void Skip(InputActionEnhancedAsset _input) {
            SkippableObject.Skip();
        }
        #endregion
    }
}
