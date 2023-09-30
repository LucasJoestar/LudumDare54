// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Physics3D.Timeline {
    /// <summary>
    /// Teleports a <see cref="Movable3D"/> to a specific position.
    /// </summary>
    [DisplayName("Movable/Teleport")]
    public class TeleportMovable3DClip : Movable3DPlayableAsset<TeleportMovable3DBehaviour> {
        #region Global Members
        public ExposedReference<Transform> Position = new ExposedReference<Transform>();
        #endregion

        #region Behaviour
        public override Playable CreatePlayable(PlayableGraph _graph, GameObject _owner) {
            Template.Position = Position.Resolve(_graph.GetResolver());
            return base.CreatePlayable(_graph, _owner);
        }
        #endregion

        #region Utility
        public override string ClipDefaultName {
            get { return "Teleport Movable"; }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="TeleportMovable3DClip"/>-related <see cref="PlayableBehaviour"/>.
    /// </summary>
    [Serializable]
    public class TeleportMovable3DBehaviour : Movable3DPlayableBehaviour {
        #region Global Members
        [Space(10f), HorizontalLine(SuperColor.Grey, 1f), Space(10f)]

        [Tooltip("Whether to use target Transform rotation or not")]
        public bool UseRotation = true;

        // -----------------------

        [NonSerialized] public Transform Position = null;
        #endregion

        #region Behaviour
        #if UNITY_EDITOR
        // Editor preview cache.
        private Vector3 fromPosition = Vector3.zero;
        private Quaternion fromRotation = Quaternion.identity;
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
                if (fromPosition.IsNull()) {
                    fromPosition = Movable.transform.position;
                    fromRotation = Movable.transform.rotation;
                }
            }
            #endif

            // Teleport.
            if (UseRotation) {
                Movable.SetPositionAndRotation(Position);
            } else {
                Movable.SetPosition(Position.position);
            }
        }

        protected override void OnStop(Playable _playable, FrameData _info, bool _completed) {
            base.OnStop(_playable, _info, _completed);

            #if UNITY_EDITOR
            if (!IsValid()) {
                return;
            }

            if (!Application.isPlaying) {

                // Complete preview.
                Vector3 _position       = _completed ? Position.position : fromPosition;
                Quaternion _rotation    = _completed ? Position.rotation : fromRotation;

                if (UseRotation) {
                    Movable.SetPositionAndRotation(_position, _rotation);
                } else {
                    Movable.SetPosition(_position);
                }

                return;
            }
            #endif
        }

        // -------------------------------------------
        // Button
        // -------------------------------------------

        /// <summary>
        /// Get if this clip is valid.
        /// </summary>
        public bool IsValid() {
            return (Movable != null) && (Position != null);
        }
        #endregion
    }
}
