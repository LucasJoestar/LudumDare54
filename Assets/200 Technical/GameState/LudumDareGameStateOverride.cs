// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core.GameStates;
using System;
using UnityEngine;

namespace LudumDare54.GameStates {
    /// <summary>
    /// Project-specific <see cref="GameStateOverride"/>.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.Name)]
    public class LudumDareGameStateOverride : DefaultGameStateOverride {
        #region Global Members
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Indicates if a cutscene is currently being played")]
        public bool IsCinematic = false;

        [Tooltip("Indicates if a debug mode is currently enabled")]
        public bool IsDebug = false;
        #endregion

        #region Behaviour
        public override GameStateOverride Reset() {
            base.Reset();

            // State.
            HasControl = false;

            IsCinematic = false;
            IsDebug = false;

            // Hide cursor.
            CursorLockMode = CursorLockMode.Confined;
            IsCursorVisible = true;

            return this;
        }

        protected override void Apply() {
            base.Apply();

            // Inputs.
            MenuInputs _menuInputs = InputSettings.I.MenuInputs;
            _menuInputs.Pause.IsEnabled = CanPause || GameState.IsActive<PauseChronosGameState>(out GameState _state, true);
        }
        #endregion
    }
}
