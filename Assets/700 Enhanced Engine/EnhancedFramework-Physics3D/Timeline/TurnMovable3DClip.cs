// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Physics3D.Timeline {
    /// <summary>
    /// Makes a <see cref="CreatureMovable3D"/> turn to a specific forward axis.
    /// </summary>
    [DisplayName("Creature/Turn")]
    public class TurnMovable3DClip : CreatureMovable3DPlayableAsset<TurnMovable3DBehaviour> {
        #region Global Members
        public ExposedReference<Transform> Forward = new ExposedReference<Transform>();
        #endregion

        #region Behaviour
        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner) {
            Template.Forward = Forward.Resolve(_graph.GetResolver());
            return base.CreatePlayable(_graph, _owner);
        }
        #endregion

        #region Utility
        public override string ClipDefaultName {
            get { return "Turn Creature"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="TurnMovable3DClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public class TurnMovable3DBehaviour : CreatureMovable3DPlayableBehaviour {
        #region Global Members
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("If true, completes the turn operation when exiting this clip")]
        public bool CompleteOnExit = true;

        // -----------------------

        [NonSerialized] public Transform Forward = null;
        #endregion

        #region Behaviour
        #if UNITY_EDITOR
        // Editor preview cache.
        private Quaternion fromRotation = default;
        #endif

        // -----------------------

        protected override void OnPlay(Playable _playable, FrameData _info) {
            base.OnPlay(_playable, _info);

            if (!IsValid()) {
                return;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying) {

                // Preview origin.
                if (fromRotation == default) {
                    fromRotation = Creature.transform.rotation;
                }

                return;
            }
            #endif

            // Turn.
            Creature.TurnTo(Forward.forward);
        }

        public override void ProcessFrame(Playable _playable, FrameData _info, object _playerData) {
            base.ProcessFrame(_playable, _info, _playerData);

            #if UNITY_EDITOR
            if (!IsValid()) {
                return;
            }

            if (!Application.isPlaying) {

                // Rotation preview.
                Quaternion _rotation = Quaternion.Lerp(fromRotation, Forward.rotation, GetNormalizedTime(_playable));
                Creature.SetRotation(_rotation);

                return;
            }
            #endif
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            if (!IsValid()) {
                return;
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying) {

                // Complete preview.
                Quaternion _rotation = _completed ? Forward.rotation : fromRotation;
                Creature.SetRotation(_rotation);

                return;
            }
            #endif

            // Complete turn.
            if (CompleteOnExit) {

                Creature.StopTurnTo();
                Creature.SetRotation(Forward.rotation);
            }
        }

        // -------------------------------------------
        // Button
        // -------------------------------------------

        /// <summary>
        /// Get if this clip is valid.
        /// </summary>
        public bool IsValid() {
            return (Creature != null) && (Forward != null);
        }
        #endregion
    }
}
