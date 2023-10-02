// =============== https://github.com/LucasJoestar/LudumDare54/ =============== //
//
// Notes:
//
// ============================================================================ //

using DG.Tweening;
using EnhancedEditor;
using EnhancedFramework.Core;
using UnityEngine;

using Min   = EnhancedEditor.MinAttribute;
using Range = EnhancedEditor.RangeAttribute;

namespace LudumDare54 {
    /// <summary>
	/// <see cref="PlayerController"/> instance associated configurable attributes.
	/// </summary>
    [CreateAssetMenu(fileName = "PCA_PlayerControllerAttributes", menuName = ProjectUtility.MenuPath + "Attributes/Player Controller", order = ProjectUtility.MenuOrder)]
    public class PlayerControllerAttributes : ScriptableObject {
        #region Global Members
        [Section("Player Controller Attributes")]

        [Tooltip("Movement speed curve, in unit/second")]
        public AdvancedCurveValue MoveSpeed = new AdvancedCurveValue(new Vector2(0f, 1f), .5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));

        [Space(5f)]

        [Min(0f)] public float AimSpeed = 1f;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        public LayerMask ColliderMask = new LayerMask();
        public LayerMask TriggerMask  = new LayerMask();

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Cooldown interval between two fires (in seconds)")]
        [Enhanced, Range(0f, 5f)] public float FireCooldown = .5f;

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Enhanced, Range(0f, 5f)] public float SpawnDuration = .5f;
        [Enhanced, Range(0f, 5f)] public float SpawnMovementOffset = 1f;

        public AnimationCurve SpawnMovementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10f)]

        [Enhanced, Range(0f, 5f)] public float DespawnDuration = .5f;
        [Enhanced, Range(0f, 5f)] public float DespawnMovementOffset = 1f;

        public AnimationCurve DespawnMovementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Enhanced, Range(0f, 5f)] public float FallDelay        = .5f;
        [Enhanced, Range(0f, 5f)] public float FallDuration     = .5f;
        [Enhanced, Range(0f, 5f)] public float FallRespawnDelay = 1f;

        [Space(5f)]

        [Enhanced, Range(-3f, 0f)] public float FallMovementOffset   = -.5f;
        [Enhanced, Range(0f, 5f)]  public float FallMovementDuration = .2f;
        public Ease FallMovementEase = Ease.OutSine;

        [Space(5f)]

        public Ease FallScaleEase = Ease.OutSine;

        [Space(5f)]

        [Enhanced, Range(0f, 720f)] public float FallRotation = 360f;
        public Ease FallRotationEase = Ease.OutSine;
        #endregion

        #region Registration
        public void Register(PlayerController _player) {

            int _id = _player.InstanceID;
            MoveSpeed.Register(_id);
        }

        public void Unregister(PlayerController _player) {

            int _id = _player.InstanceID;
            MoveSpeed.Unregister(_id);
        }
        #endregion
    }
}
