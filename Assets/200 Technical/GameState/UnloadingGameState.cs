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
    /// Project specific <see cref="UnloadingGameState{T}"/>.
    /// </summary>
    [Serializable, DisplayName("Loading/Unloading [Ludum Dare]")]
    public class UnloadingGameState : UnloadingGameState<LudumDareGameStateOverride> {
        #region Global Members
        public override int Priority {
            get { return GameStateUtility.UnloadingPriority; }
        }
        #endregion

        #region State Override
        public override void OnStateOverride(LudumDareGameStateOverride _state) {
            base.OnStateOverride(_state);

            // All required overrides are made during loading.
            // Nothing unloading specific to implement, for now.
        }
        #endregion
    }
}
