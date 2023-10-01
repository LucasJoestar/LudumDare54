// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core.Settings;
using UnityEngine;

using Min = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54
{
    /// <summary>
    /// Contains various global game settings.
    /// </summary>
    [CreateAssetMenu(fileName = MenuPrefix + "VariousSettings", menuName = MenuPath + "Various Settings", order = MenuOrder)]
    public class VariousSettings : BaseSettings<VariousSettings> {
        #region Global Members
        [Section("Various Settings", order = -1)]

        public LayerMask GroundMask = new LayerMask();
        public LayerMask WallMask   = new LayerMask();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [SerializeField, Enhanced, Range(0f, 5f)] public float UI_ActivateScaleDuration = .5f;
        [SerializeField] public Ease UI_ActivateScaleEase = Ease.OutSine;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 5f)] public float UI_DeactivateScaleDuration = .5f;
        [SerializeField] public Ease UI_DeactivateScaleEase = Ease.OutSine;

        [Space(10f)]

        [SerializeField, Enhanced, Range(0f, 5f)] public float UI_SelectScale = 1.5f;
        [SerializeField, Enhanced, Range(0f, 5f)] public float UI_SelectScaleDuration = .5f;
        [SerializeField] public Ease UI_SelectScaleEase = Ease.OutSine;

        [Space(5f)]

        [SerializeField, Enhanced, Range(0f, 5f)] public float UI_UnselectScaleDuration = .5f;
        [SerializeField] public Ease UI_UnselectScaleEase = Ease.OutSine;

        [Space(10f)]

        [SerializeField] public Material UI_SelectFont = null;
        [SerializeField] public Color UI_SelectColor   = Color.green;

        [Space(5f)]

        [SerializeField] public Material UI_UnselectFont = null;
        [SerializeField] public Color UI_UnselectColor   = Color.green;
        #endregion
    }
}
