// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedFramework.Timeline;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Timeline;

using DisplayName = System.ComponentModel.DisplayNameAttribute;

namespace EnhancedFramework.Physics3D.Timeline {
    /// <summary>
    /// <see cref="TrackAsset"/> class for every <see cref="IMovable3DPlayableAsset"/>.
    /// </summary>
    [TrackColor(.19f, .76f, .65f)] // Turquoise
    [TrackClipType(typeof(IMovable3DPlayableAsset))]
    [TrackBindingType(typeof(Movable3D), TrackBindingFlags.AllowCreateComponent)]
    [DisplayName("Enhanced Framework/Movable 3D Track")]
    public class Movable3DTrack : EnhancedTrack {
        #region Behaviour
        public override void GatherProperties(PlayableDirector _director, IPropertyCollector _driver) {
            base.GatherProperties(_director, _driver);

            Object _object = _director.GetGenericBinding(this);
            if ((_object == null) || !(_object is Movable3D _movable) || (_movable == null)) {
                return;
            }

            // Regiser Transform properties.
            _driver.AddFromComponent(_movable.gameObject, _movable.transform);
        }
        #endregion
    }
}
