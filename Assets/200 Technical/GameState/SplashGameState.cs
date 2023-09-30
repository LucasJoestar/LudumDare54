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
    /// Project specific <see cref="SplashGameState{T}"/>.
    /// </summary>
    [Serializable, DisplayName("Splash/Splash [Ludum Dare]")]
    public class SplashGameState : SplashGameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.SplashPriority; }
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
