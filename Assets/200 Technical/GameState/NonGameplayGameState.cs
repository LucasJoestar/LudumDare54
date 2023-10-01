// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;

namespace LudumDare54.GameStates {
    /// <summary>
    /// Gameplay sequence <see cref="GameState"/>, removing any control from the player.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Non Gameplay")]
    public class NonGameplayGameState : GameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.NonGameplayProprity; }
        }

        public override bool IsPersistent {
            get { return false; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="GameplayGameState"/>
        public NonGameplayGameState() : base(false) { }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // Remove control from the player.
            _state.HasControl = false;
            _state.CanPause = false;
        }
        #endregion
    }
}
