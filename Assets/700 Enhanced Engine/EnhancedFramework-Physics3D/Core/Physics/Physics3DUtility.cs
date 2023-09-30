// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// Contains multiple 3D Physics related utility methods.
    /// </summary>
    public static class Physics3DUtility {
        #region Raycast Hit
        private static readonly List<RaycastHit> hitBuffer = new List<RaycastHit>();
        private static readonly Comparison<RaycastHit> hitComparison = CompareRaycastHits;

        // -----------------------

        /// <summary>
        /// Sort an array of <see cref="RaycastHit"/> by their distance.
        /// </summary>
        /// <param name="_hits">Hits to sort.</param>
        /// <param name="_count">Total count of hits to sort.</param>
        public static void SortRaycastHitByDistance(RaycastHit[] _hits, int _count) {

            // Use List.Sort instead of Array.Sort to avoid any memory allocation.
            hitBuffer.Resize(_count);
            for (int i = 0; i < _count; i++) {
                hitBuffer[i] = _hits[i];
            }

            hitBuffer.Sort(hitComparison);

            // Update array content.
            for (int i = 0; i < _count; i++) {
                _hits[i] = hitBuffer[i];
            }
        }

        // -----------------------

        private static int CompareRaycastHits(RaycastHit a, RaycastHit b) {
            return a.distance.CompareTo(b.distance);
        }
        #endregion

        #region Collider
        private static readonly List<Collider> colliderBuffer = new List<Collider>();
        private static readonly Comparison<Collider> colliderComparison = CompareColliders;

        private static Vector3 reference = Vector3.zero;

        // -----------------------

        /// <summary>
        /// Sort an array of <see cref="Collider"/> by their distance from a reference <see cref="Vector3"/>.
        /// </summary>
        /// <param name="_colliders">Colliders to sort.</param>
        /// <param name="_count">Total count of colliders to sort.</param>
        /// <param name="_reference">Reference position used for sorting.</param>
        public static void SortCollidersByDistance(Collider[] _colliders, int _count, Vector3 _reference) {

            // Store reference position.
            reference = _reference;

            // Use List.Sort instead of Array.Sort to avoid any memory allocation.
            colliderBuffer.Resize(_count);
            for (int i = 0; i < _count; i++) {
                colliderBuffer[i] = _colliders[i];
            }

            colliderBuffer.Sort(colliderComparison);

            // Update array content.
            for (int i = 0; i < _count; i++) {
                _colliders[i] = colliderBuffer[i];
            }
        }

        // -----------------------

        static int CompareColliders(Collider a, Collider b) {
            return (a.transform.position - reference).sqrMagnitude.CompareTo((b.transform.position - reference).sqrMagnitude);
        }
        #endregion

        #region Collision Mask
        /// <summary>
        /// Get the collision layer mask that indicates which layer(s) the specified <see cref="GameObject"/> can collide with.
        /// </summary>
        /// <param name="_gameObject">The <see cref="GameObject"/> to retrieve the collision layer mask for.</param>
        public static int GetLayerCollisionMask(GameObject _gameObject) {
            int _layer = _gameObject.layer;
            return GetLayerCollisionMask(_layer);
        }

        /// <summary>
        /// Get the collision layer mask that indicates which layer(s) the specified layer can collide with.
        /// </summary>
        /// <param name="_layer">The layer to retrieve the collision layer mask for.</param>
        public static int GetLayerCollisionMask(int _layer) {
            int _layerMask = 0;
            for (int i = 0; i < 32; i++) {
                if (!Physics.GetIgnoreLayerCollision(_layer, i))
                    _layerMask |= 1 << i;
            }

            return _layerMask;
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IsGroundSurface(Collider, Vector3, Vector3)"/>
        /// <param name="_hit">Hit result of the surface to stand on.</param>
        public static bool IsGroundSurface(RaycastHit _hit, Vector3 _up) {
            return IsGroundSurface(_hit.collider, _hit.normal, _up);
        }

        /// <summary>
        /// Get if a specific surface can be considered as a ground (surface to stand on) or not.
        /// </summary>
        /// <param name="_collider">Collider attached to the testing surface.</param>
        /// <param name="_normal">The normal surface to check.</param>
        /// <param name="_up">Referential up vector of the object to stand on the surface.</param>
        public static bool IsGroundSurface(Collider _collider, Vector3 _normal, Vector3 _up) {
            float _angle = Vector3.Angle(_normal, _up);
            return (_angle <= Physics3DSettings.I.GroundAngle) && !_collider.TryGetComponent<NonGroundSurface3D>(out _);
        }
        #endregion
    }
}
