// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using EnhancedFramework.Core.Settings;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
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

        public Color AvailableSpaceColor    = SuperColor.Green.Get(.75f);
        public Color UnavailableSpaceColor  = SuperColor.Crimson.Get(.75f);

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Enhanced, Range(0f, 1f)] public float SpellMovementSpeed   = .1f;
        [Enhanced, Range(0f, 5f)] public float SpellMovementHeight  = .7f;
        [Enhanced, Range(0f, 360f)] public float SpellRotation      = 90f;
        public Ease SpellMovementEase = Ease.OutSine;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Enhanced, Range(0f, 5f)] public float SpawnPlatformDuration = .5f;
        public Ease SpawnPlatformEase = Ease.OutBack;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        public GameObject SpellProjectile = null;
        public ParticleSystemAsset SpawnPlatformFX = null;

        [Space(5f)]

        public ParticleSystemAsset SpawnPlayerFX = null;
        public ParticleSystemAsset DespawnPlayerFX = null;
        #endregion
    }
}
