// ===== Twin Peaks - https://www.plasticscm.com/orgs/blueroseteam/repos/TwinPeaks/ ===== //
//
// Notes:
//
// ====================================================================================== //

using EnhancedFramework.Core;
using UnityEngine;

namespace LudumDare54 {
    /// <summary>
    /// Contains multiple project-related utilities.
    /// </summary>
    public static class ProjectUtility {
        #region Menu
        /// <summary>
        /// Name of this project.
        /// </summary>
        public const string Name = "LudumDare-54";

        /// <summary>
        /// Menu path prefix used for creating new <see cref="ScriptableObject"/>, or any other special menu.
        /// </summary>
        public const string MenuPath = Name + "/";

        /// <summary>
        /// Menu item path used for project utilities.
        /// </summary>
        public const string MenuItemPath = "Tools/" + MenuPath;

        /// <summary>
        /// Menu order used for creating new <see cref="ScriptableObject"/> from the asset menu.
        /// </summary>
        public const int MenuOrder = 200;
        #endregion

        #region Coordinates
        public static Vector2 GetCoords(Vector2 worldPosition) {
            return new Vector2(Mathf.Round(worldPosition.x), Mathf.Round(worldPosition.y));
        }
        #endregion

        #region Camera
        private const float HeightMarginsCoef = .86222221f;

        // -----------------------

        public static Bounds GetCameraBounds() {

            Camera _camera = MainCameraBehaviour.MainCamera;

            float _heightMargins = _camera.orthographicSize * (1f - HeightMarginsCoef);

            float _height = _camera.orthographicSize * HeightMarginsCoef;
            float _width  = _camera.aspect * _camera.orthographicSize;

            return new Bounds(new Vector3(0f, -_heightMargins), new Vector2(_width, _height) * 2f);
        }

        public static Vector2 ClampInCameraBounds(Vector2 _position) {

            Bounds _bounds    = GetCameraBounds();
            Vector3 _extents = _bounds.extents;

            _position.x = Mathf.Clamp(_position.x, _bounds.center.x - _extents.x, _bounds.center.x + _extents.x);
            _position.y = Mathf.Clamp(_position.y, _bounds.center.y - _extents.y, _bounds.center.y + _extents.y);

            return _position;
        }
        #endregion
    }
}
