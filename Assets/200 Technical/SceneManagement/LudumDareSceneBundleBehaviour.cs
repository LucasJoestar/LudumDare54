// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using System;
using UnityEngine;

namespace LudumDare54.UI {
    /// <summary>
    /// Project-specific <see cref="SceneBundleBehaviour"/>.
    /// </summary>
    [Serializable, DisplayName(ProjectUtility.Name)]
    public class LudumDareSceneBundleBehaviour : SceneBundleBehaviour {
        #region Global Members
        [Enhanced, Required] public SceneBundle NextScene   = null;
        [Enhanced, Required] public Sprite LoadingScreen    = null;
        #endregion
    }
}
