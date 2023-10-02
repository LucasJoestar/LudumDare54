// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.Settings;
using EnhancedFramework.Inputs;
using EnhancedFramework.UI;
using System;
using UnityEngine;

namespace LudumDare54 {
    /// <summary>
    /// Interface-related inputs wrapper.
    /// </summary>
    [Serializable]
    public class InterfaceInputs {
        #region Global Members
        [Enhanced, Required] public InputActionEnhancedAsset Navigation = null;
        [Enhanced, Required] public InputActionEnhancedAsset Validate   = null;

        [Space(5f)]

        [Enhanced, Required] public InputActionEnhancedAsset Submit     = null;
        [Enhanced, Required] public InputActionEnhancedAsset Cancel     = null;
        #endregion
    }

    /// <summary>
    /// Menu-related inputs wrapper.
    /// </summary>
    [Serializable]
    public class MenuInputs {
        #region Global Members
        [Enhanced, Required] public InputActionEnhancedAsset Pause      = null;
        #endregion
    }

    /// <summary>
    /// Player-related inputs wrapper.
    /// </summary>
    [Serializable]
    public class PlayerInputs {
        #region Content
        [Enhanced, Required] public AxisInputActionEnhancedAsset HorizontalAxis = null;
        [Enhanced, Required] public AxisInputActionEnhancedAsset ForwardAxis    = null;
        [Enhanced, Required] public InputActionEnhancedAsset SelectionAxis      = null;

        [Space(5f)]

        [Enhanced, Required] public InputActionEnhancedAsset ActionInput        = null;
        [Enhanced, Required] public InputActionEnhancedAsset FireInput          = null;
        [Enhanced, Required] public InputActionEnhancedAsset Fire2Input         = null;
        [Enhanced, Required] public InputActionEnhancedAsset AimInput           = null;
        [Enhanced, Required] public InputActionEnhancedAsset ResetInput         = null;
        #endregion
    }

    /// <summary>
    /// Global input-related game settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "InputSettings", menuName = MenuPath + "Input", order = MenuOrder)]
    public class InputSettings : BaseSettings<InputSettings> {
        #region Global Members
        [Section("Input Settings", order = -1)]

        [Space(20f, order = 0), Title("Maps", "Input maps", order = 1), Space(5f, order = 2)]

        [Enhanced, Required] public InputMapEnhancedAsset InterfaceMap          = null;
        [Enhanced, Required] public InputMapEnhancedAsset MenuMap               = null;
        [Enhanced, Required] public InputMapEnhancedAsset PlayerMap             = null;

        [Space(20f, order = 0), Title("Interface", "Interface-related inputs", order = 1), Space(5f, order = 2)]

        [Enhanced, Block] public InterfaceInputs InterfaceInputs = new InterfaceInputs();

        [Space(20f, order = 0), Title("Menus", "Menus-related inputs", order = 1), Space(5f, order = 2)]

        [Enhanced, Block] public MenuInputs MenuInputs = new MenuInputs();

        [Space(20f, order = 0), Title("Player", "Player-related inputs", order = 1), Space(5f, order = 2)]

        [Enhanced, Block] public PlayerInputs PlayerInputs = new PlayerInputs();

        [Space(20f, order = 0), Title("Utility", "Utility inputs", order = 1), Space(5f, order = 2)]

        [Enhanced, Required] public InputActionEnhancedAsset AnyButtonInput     = null;
        [Enhanced, Required] public InputActionEnhancedAsset SkipInput          = null;
        [Enhanced, Required] public InputActionEnhancedAsset ExitInput          = null;
        #endregion

        #region Behaviour
        protected override void Init() {
            base.Init();

            // Input management.
            FadingGroupInteractionController.OnEnableInputs  += EnableInterfaceMap;
            FadingGroupInteractionController.OnDisableInputs += DisableInterfaceMap;

            InterfaceInputs.Cancel.OnPerformed += OnInterfaceCancelPerformed;
        }
        #endregion

        #region Interface
        /// <summary>
        /// Called when the interface cancel input is performed.
        /// <br/> Must return true if the event was used, false otherwise.
        /// </summary>
        public static event Func<bool> OnInterfaceCancel = null;

        private DelayHandler delay = default;

        // -----------------------

        /// <summary>
        /// Enables the UI input map.
        /// <br/>Keep in mind to disable it once done.
        /// </summary>
        public void EnableInterfaceMap() {
            delay.Cancel();
            InterfaceMap.Enable();
        }

        /// <summary>
        /// Disables the UI input map.
        /// </summary>
        public void DisableInterfaceMap() {
            delay.Cancel();
            InterfaceMap.Disable();
        }

        /// <summary>
        /// Temporarily disables the UI input map.
        /// </summary>
        /// <param name="_duration">Time during which to disable the input map (in seconds).</param>
        public void PauseInterfaceMap(float _duration = 2f) {
            DisableInterfaceMap();
            delay = Delayer.Call(_duration, EnableInterfaceMap, true);
        }

        // -------------------------------------------
        // Callbacks
        // -------------------------------------------

        private void OnInterfaceCancelPerformed(InputActionEnhancedAsset _input) {

            // Registered event.
            if ((OnInterfaceCancel != null) && OnInterfaceCancel.Invoke()) {
                return;
            }

            if (FadingGroupInteractionController.OnGroupCancelEvent()) {
                return;
            }

            // No event.
            this.LogMessage("No Cancel Event Registered");
        }
        #endregion
    }
}
