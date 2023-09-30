// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using EnhancedFramework.Core;
using System;
using UnityEngine;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// Physics wrapper for any engine 3D primitive collider.
    /// <br/> Use this to perform precise cast and overlap operations.
    /// </summary>
    [Serializable]
    public class PhysicsCollider3D {
        #region Global Members
        [SerializeField, Enhanced, Required] private Collider collider = null;

        /// <summary>
        /// Default mask used for collision detections.
        /// </summary>
        [NonSerialized] public int CollisionMask = 0;

        private ColliderWrapper3D wrapper = null;

        // -----------------------

        /// <summary>
        /// Wrapped <see cref="UnityEngine.Collider"/> reference.
        /// </summary>
        public Collider Collider {
            get { return collider; }
            set {
                collider = value;
                Initialize();
            }
        }

        /// <summary>
        /// World-space center of the collider bounding box.
        /// </summary>
        public Vector3 Center {
            get { return collider.bounds.center; }
        }

        /// <summary>
        /// World-space non-rotated extents of the collider.
        /// </summary>
        public Vector3 Extents {
            get { return wrapper.GetExtents(); }
        }
        #endregion

        #region Initialization
        /// <inheritdoc cref="Initialize(int)"/>
        public void Initialize() {
            int _layer = Physics3DUtility.GetLayerCollisionMask(collider.gameObject);
            Initialize(_layer);
        }

        /// <summary>
        /// Initializes this <see cref="PhysicsCollider3D"/>.
        /// <br/> Should be called before any use, preferably during <see cref="EnhancedBehaviour.OnInit"/>.
        /// </summary>
        /// <param name="_collisionMask">Default mask to be used for collider collision detections.</param>
        public void Initialize(int _collisionMask) {
            CollisionMask = _collisionMask;
            wrapper = ColliderWrapper3D.Create(collider);
        }
        #endregion

        #region Overlap
        private static readonly Collider[] overlapBuffer = new Collider[16];

        // -------------------------------------------
        // Overlap
        // -------------------------------------------

        /// <inheritdoc cref="Overlap(int, QueryTriggerInteraction)"/>
        public int Overlap(QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide) {
            return Overlap(CollisionMask, _triggerInteraction);
        }

        /// <inheritdoc cref="Overlap(Collider[], int, QueryTriggerInteraction)"/>
        public int Overlap(int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide) {
            return wrapper.Overlap(overlapBuffer, _mask, _triggerInteraction);
        }

        /// <inheritdoc cref="Overlap(Collider[], int, QueryTriggerInteraction)"/>
        public int Overlap(Collider[] _ignoredColliders, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide) {
            return Overlap(_ignoredColliders, CollisionMask, _triggerInteraction);
        }

        /// <summary>
        /// Get detailed informations about the current overlapping colliders.
        /// <para/>
        /// Note that this collider itself may be found depending on the used detection mask.
        /// </summary>
        /// <param name="_ignoredColliders">Colliders to ignore.</param>
        /// <param name="_mask"><see cref="LayerMask"/> to use for detection.</param>
        /// <param name="_triggerInteraction">Determines if triggers should be detected.</param>
        /// <returns>Total amount of overlapping colliders.</returns>
        public int Overlap(Collider[] _ignoredColliders, int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide) {
            int _amount = Overlap(_mask, _triggerInteraction);

            for (int i = 0; i < _amount; i++) {
                if (ArrayUtility.Contains(_ignoredColliders, overlapBuffer[i])) {
                    overlapBuffer[i] = overlapBuffer[--_amount];
                    i--;
                }
            }

            return _amount;
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get the overlapping collider at a given index.
        /// <br/> Note that the last overlap is from the whole game loop, not specific to this collider.
        /// <para/>
        /// Use <see cref="Overlap(Collider[], int, QueryTriggerInteraction)"/> to get the total count of overlapping colliders.
        /// </summary>
        /// <param name="_index">Index to get the collider at.</param>
        /// <returns>The overlapping collider at the given index.</returns>
        public static Collider GetOverlapCollider(int _index) {
            return overlapBuffer[_index];
        }

        /// <summary>
        /// Sorts overlapping colliders by distance, using a specific <see cref="Vector3"/> reference position.
        /// </summary>
        /// <inheritdoc cref="Physics3DUtility.SortCollidersByDistance(Collider[], int, Vector3)"/>
        public static void SortOverlapCollidersByDistance(int _count, Vector3 _reference) {
            Physics3DUtility.SortCollidersByDistance(overlapBuffer, _count, _reference);
        }
        #endregion

        #region Raycast
        /// <inheritdoc cref="Raycast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction)"/>
        public bool Raycast(Vector3 _velocity, out RaycastHit _hit, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore) {
            return Raycast(_velocity, out _hit, CollisionMask, _triggerInteraction);
        }

        /// <inheritdoc cref="Raycast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction)"/>
        public bool Raycast(Vector3 _velocity, out RaycastHit _hit, int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore) {
            float _distance = _velocity.magnitude;
            return Raycast(_velocity, out _hit, _distance, _mask, _triggerInteraction);
        }

        /// <inheritdoc cref="Raycast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction)"/>
        public bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore) {
            return Raycast(_direction, out _hit, _distance, CollisionMask, _triggerInteraction);
        }

        /// <summary>
        /// Performs a raycasts from this collider in a given direction.
        /// </summary>
        /// <param name="_direction">Raycast direction.</param>
        /// <param name="_hit">Detailed informations on raycast hit.</param>
        /// <param name="_distance">Maximum raycast distance.</param>
        /// <param name="_mask"><see cref="LayerMask"/> used for collisions detection.</param>
        /// <param name="_triggerInteraction">Determines if triggers should be detected.</param>
        /// <returns>True if the raycast hit something, false otherwise.</returns>
        public bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore) {
            return wrapper.Raycast(_direction, out _hit, _distance, _mask, _triggerInteraction);
        }
        #endregion

        #region Cast
        /// <summary>
        /// Maximum distance when compared to the first hit of a cast, to be considered as valid.
        /// </summary>
        public const float MaxCastDifferenceDetection = .001f;

        internal static readonly RaycastHit[] castBuffer = new RaycastHit[8];

        // -------------------------------------------
        // Cast
        // -------------------------------------------

        /// <param name="_velocity"><inheritdoc cref="CastAll(Vector3, out RaycastHit, QueryTriggerInteraction, Collider[])" path="/param[@name='_velocity']"/></param>
        /// <inheritdoc cref="Cast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public bool Cast(Vector3 _velocity, out float _distance, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                         Collider[] _ignoredColliders = null) {
            bool _doHit = Cast(_velocity, out RaycastHit _hit, CollisionMask, _triggerInteraction, _ignoredColliders);

            _distance = _hit.distance;
            return _doHit;
        }

        /// <param name="_velocity"><inheritdoc cref="CastAll(Vector3, out RaycastHit, QueryTriggerInteraction, Collider[])" path="/param[@name='_velocity']"/></param>
        /// <inheritdoc cref="Cast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public bool Cast(Vector3 _velocity, out RaycastHit _hit, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                         Collider[] _ignoredColliders = null) {
            return Cast(_velocity, out _hit, CollisionMask, _triggerInteraction, _ignoredColliders);
        }

        /// <param name="_velocity"><inheritdoc cref="CastAll(Vector3, out RaycastHit, QueryTriggerInteraction, Collider[])" path="/param[@name='_velocity']"/></param>
        /// <inheritdoc cref="Cast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public bool Cast(Vector3 _velocity, out RaycastHit _hit, int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                         Collider[] _ignoredColliders = null) {
            float _distance = _velocity.magnitude;
            return Cast(_velocity, out _hit, _distance, _mask, _triggerInteraction, _ignoredColliders);
        }

        /// <inheritdoc cref="Cast(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public bool Cast(Vector3 _direction, out RaycastHit _hit, float _distance, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                         Collider[] _ignoredColliders = null) {
            return Cast(_direction, out _hit, _distance, CollisionMask, _triggerInteraction, _ignoredColliders);
        }

        /// <returns>True if this collider hit something, false otherwise.</returns>
        /// <inheritdoc cref="CastAll(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public bool Cast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                         params Collider[] _ignoredColliders) {
            return CastAll(_direction, out _hit, _distance, _mask, _triggerInteraction, _ignoredColliders) != 0;
        }

        // -------------------------------------------
        // Cast All
        // -------------------------------------------

        /// <param name="_velocity"><inheritdoc cref="CastAll(Vector3, out RaycastHit, QueryTriggerInteraction, Collider[])" path="/param[@name='_velocity']"/></param>
        /// <inheritdoc cref="CastAll(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public int CastAll(Vector3 _velocity, out float _distance, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                           Collider[] _ignoredColliders = null) {
            int _amount = CastAll(_velocity, out RaycastHit _hit, _triggerInteraction, _ignoredColliders);

            _distance = _hit.distance;
            return _amount;
        }

        /// <param name="_velocity">Velocity used to perform this cast.</param>
        /// <inheritdoc cref="CastAll(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public int CastAll(Vector3 _velocity, out RaycastHit _hit, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                           Collider[] _ignoredColliders = null) {
            float _distance = _velocity.magnitude;
            return CastAll(_velocity, out _hit, _distance, CollisionMask, _triggerInteraction, _ignoredColliders);
        }

        /// <inheritdoc cref="CastAll(Vector3, out RaycastHit, float, int, QueryTriggerInteraction, Collider[])"/>
        public int CastAll(Vector3 _direction, out RaycastHit _hit, float _distance, QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore,
                           Collider[] _ignoredColliders = null) {
            return CastAll(_direction, out _hit, _distance,  CollisionMask, _triggerInteraction, _ignoredColliders);
        }

        /// <summary>
        /// Performs a cast from this collider in a given direction.
        /// </summary>
        /// <param name="_direction">Cast direction.</param>
        /// <param name="_hit">Main trajectory hit detailed informations.</param>
        /// <param name="_distance">Maximum cast distance.</param>
        /// <param name="_mask"><see cref="LayerMask"/> used for collisions detection.</param>
        /// <param name="_triggerInteraction">Determines if triggers should be detected.</param>
        /// <param name="_ignoredColliders">Colliders to ignore.</param>
        /// <returns>Total amount of consistent hits on the trajectory.</returns>
        public int CastAll(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction,
                           params Collider[] _ignoredColliders) {
            // Distance security.
            float _contactOffset = Physics.defaultContactOffset;
            _distance += _contactOffset * 2f;

            int _amount = wrapper.Cast(_direction, castBuffer, _distance, _mask, _triggerInteraction);

            if (_amount > 0) {
                if ((_ignoredColliders == null) || (_ignoredColliders.Length == 0)) {
                    // Remove this object collider if detected.
                    if (castBuffer[_amount - 1].collider == collider) {
                        _amount--;
                        if (_amount == 0) {
                            _hit = GetDefaultHit();
                            return 0;
                        }
                    }

                    #if DEVELOPMENT
                    // Debug utility. Should be remove at some point.
                    for (int i = 0; i < _amount; i++) {
                        if (castBuffer[i].collider == collider) {
                            collider.LogError($"This object collider found => {i}/{_amount}");
                        }
                    }
                    #endif
                } else {
                    // Ignored colliders.
                    for (int i = 0; i < _amount; i++) {
                        if (ArrayUtility.Contains(_ignoredColliders, castBuffer[i].collider)) {
                            castBuffer[i] = castBuffer[--_amount];
                            i--;
                        }
                    }
                }

                Physics3DUtility.SortRaycastHitByDistance(castBuffer, _amount);

                _hit = castBuffer[0];
                _hit.distance = Mathf.Max(0f, _hit.distance - _contactOffset);

                for (int i = 1; i < _amount; i++) {
                    if (castBuffer[i].distance > (_hit.distance + MaxCastDifferenceDetection))
                        return i;
                }
            } else {
                // No hit, so get full distance.
                _hit = GetDefaultHit();
            }

            return _amount;

            // ----- Local Method ----- \\

            RaycastHit GetDefaultHit() {
                return new RaycastHit {
                    distance = _distance - _contactOffset
                };
            }
        }

        // -------------------------------------------
        // Utility
        // -------------------------------------------

        /// <summary>
        /// Get detailed informations from the last cast at a given index.
        /// <br/> Note that the last cast is from the whole game loop, not specific to this collider.
        /// </summary>
        /// <param name="_index">Index to get the hit at.</param>
        /// <returns>Detailed informations about the hit at the given index.</returns>
        public static RaycastHit GetCastHit(int _index) {
            return castBuffer[_index];
        }

        /// <summary>
        /// Sorts detected hits by distance.
        /// </summary>
        /// <inheritdoc cref="Physics3DUtility.SortRaycastHitByDistance(RaycastHit[], int)"/>
        public static void SortCastHitByDistance(int _count) {
            Physics3DUtility.SortRaycastHitByDistance(castBuffer, _count);
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="ColliderWrapper3D.SetBounds(Vector3, Vector3)"/>
        public void SetBounds(Vector3 _center, Vector3 _size) {
            wrapper.SetBounds(_center, _size);
        }
        #endregion
    }
}
