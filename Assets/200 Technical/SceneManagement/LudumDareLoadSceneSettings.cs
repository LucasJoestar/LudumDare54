// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;

namespace LudumDare54.UI {
    /// <summary>
    /// Lists all the different chronos-related effects that can occur during a loading.
    /// </summary>
    public enum LoadingChronos {
        /// <summary>
        /// Don't freeze the game for the loading.
        /// </summary>
        None = 0,

        /// <summary>
        /// Freezes the game as soon as the loading starts being initialized.
        /// </summary>
        FreezeInstant,

        /// <summary>
        /// Freezes the game once the screen has faded to black.
        /// </summary>
        FreezeOnFaded,
    }

    /// <summary>
    /// Lists all the different loading modes that can be displayed.
    /// </summary>
    public enum LoadingMode {
        /// <summary>
        /// Displays the default loading screen.
        /// </summary>
        Default     = 0,

        /// <summary>
        /// Only displays a simple black screen
        /// </summary>
        Transition       = 9,
    }

    /// <summary>
    /// Project-specific <see cref="LoadSceneSettings"/>.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.Name)]
    public class LudumDareLoadSceneSettings : LoadSceneSettings {
        #region Global Members
        /// <summary>
        /// The type of loading to display with these settings.
        /// </summary>
        public LoadingMode Mode = LoadingMode.Default;

        /// <summary>
        /// Determines how the game chronos will be affected with these settings.
        /// </summary>
        public LoadingChronos Chronos = LoadingChronos.FreezeInstant;

        /// <summary>
        /// Delay before playing a cutscene or a video.
        /// </summary>
        public float Delay = 0f;

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="LudumDareLoadSceneSettings(LoadingMode, LoadingChronos)"/>
        public LudumDareLoadSceneSettings() : this(LoadingMode.Default, LoadingChronos.FreezeInstant) { }

        /// <param name="_mode"><inheritdoc cref="Mode" path="/summary"/></param>
        /// <param name="_chronos"><inheritdoc cref="Chronos" path="/summary"/></param>
        /// <inheritdoc cref="LudumDareLoadSceneSettings"/>
        public LudumDareLoadSceneSettings(LoadingMode _mode, LoadingChronos _chronos) {
            Mode = _mode;
            Chronos = _chronos;
        }
        #endregion
    }
}
