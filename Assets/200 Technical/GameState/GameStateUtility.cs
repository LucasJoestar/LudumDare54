// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedFramework.Core.GameStates;

namespace LudumDare54.GameStates {
    /// <summary>
    /// Project <see cref="GameState"/>-related static utility class.
    /// </summary>
    public static class GameStateUtility {
        #region Priority
        // -------------------------------------------
        // Twin Peaks
        // -------------------------------------------

        public const int GameplayProprity               = 10;   // Neutral priority.
        public const int ActiveGameplayProprity         = 15;   // Neutral priority.
        public const int NonGameplayProprity            = 25;   // Neutral priority.
        public const int CinematicActionProprity        = 50;   // Above gameplay priority.
        public const int MainMenuPriority               = 70;   // Menu low priority.

        public const int BackroundCinematicProprity     = 101;   // Priority below active conversation or collection.

        public const int LocationTransitionPriority     = 300;   // High priority.

        public const int PressAnyButtonPriority         = 690;   // High priority to be active.
        public const int CinematicProprity              = 700;   // High priority to always apply its overrides.

        public const int UnloadingPriority              = DefaultUnloadingGameState.PriorityConst;      // 0
        public const int SplashPriority                 = DefaultSplashGameState.PriorityConst;         // 900
        public const int LoadingPriority                = DefaultLoadingGameState.PriorityConst;        // 999

        public const int PauseChronosPriority           = DefaultPauseChronosGameState.PriorityConst;   // 2 147 482 648 | [2 147 483 647 - 999]

        // -------------------------------------------
        // Enhanced Framework
        // -------------------------------------------

        public const int DefaultPriority                = DefaultGameState.PriorityConst;               // -100
        public const int MuteAudioPriority              = MuteAudioGameState.PriorityConst;             // Low priority.
        public const int QuitPriority                   = QuitGameState.PriorityConst;                  // 0
        #endregion
    }
}
