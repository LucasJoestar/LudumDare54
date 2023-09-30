using EnhancedEditor;
using EnhancedFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LudumDare54
{
    public class PlayerController : EnhancedSingleton<PlayerController> {
        #region Global Members

        #endregion

        #region Behaviour
        [Button(ActivationMode.Editor, SuperColor.Green)]
        public void CreateBlockHere() {
            this.LogError("Coucou");
        }
        #endregion
    }
}
