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
    /// Project specific <see cref="PauseChronosGameState{T}"/>.
    /// </summary>
    [Serializable, DisplayName("Chronos/Pause [Ludum Dare]")]
    public class PauseChronosGameState : PauseChronosGameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.PauseChronosPriority; }
        }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // State update.
            _state.HasControl = false;
        }
        #endregion
    }
}
