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
    /// <see cref="GameState"/> applied when transitioning from one area location to another.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.MenuPath + "Location Transition")]
    public class LocationTransitionGameState : GameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.LocationTransitionPriority; }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="LocationTransitionGameState"/>
        public LocationTransitionGameState() : base() { }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // Remove any player control.
            _state.HasControl = false;

            _state.IsPaused = true;
            _state.FreezeChronos = true;
        }
        #endregion
    }
}
