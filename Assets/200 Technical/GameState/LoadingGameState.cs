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
    /// Project specific <see cref="LoadingGameState{T}"/>.
    /// </summary>
    [Serializable, DisplayName("Loading/Loading [Ludum Dare]")]
    public class LoadingGameState : LoadingGameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.LoadingPriority; }
        }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // State update.
            _state.HasControl = false;
            _state.FreezeChronos = true;
        }
        #endregion
    }
}
