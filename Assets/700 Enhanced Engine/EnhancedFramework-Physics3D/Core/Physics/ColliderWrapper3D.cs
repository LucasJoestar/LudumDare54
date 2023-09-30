// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework-Physics3D ===== //
//
// Notes:
//
// ============================================================================================ //

using EnhancedEditor;
using System;
using UnityEngine;

namespace EnhancedFramework.Physics3D {
    /// <summary>
    /// Internal low-level wrapper for engine 3D primitive colliders.
    /// <para/>
    /// Used from the <see cref="PhysicsCollider3D"/> class for precise cast and overlap operations.
    /// </summary>
	internal abstract class ColliderWrapper3D {
        #region Global Members
        protected readonly Transform transform = null;

        // -----------------------

        protected ColliderWrapper3D(Collider _collider) {
            transform = _collider.transform;
        }

        // -------------------------------------------
        // Creator
        // -------------------------------------------

        /// <summary>
        /// Creates a new appropriated <see cref="ColliderWrapper3D"/> for a specific collider.
        /// </summary>
        /// <param name="_collider">Collider to get a new wrapper for.</param>
        /// <returns>Configured wrapper for the specified collider.</returns>
        public static ColliderWrapper3D Create(Collider _collider) {
            switch (_collider) {
                case BoxCollider _box:
                    return new BoxColliderWrapper3D(_box);

                case CapsuleCollider _capsule:
                    return new CapsuleColliderWrapper3D(_capsule);

                case SphereCollider _sphere:
                    return new SphereColliderWrapper3D(_sphere);

                default:
                    throw new NonPrimitiveColliderException();
            }
        }
        #endregion

        #region Physics
        /// <summary>
        /// Performs a raycast from this collider using specific parameters.
        /// </summary>
        public abstract bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction);

        /// <summary>
        /// Performs a cast from this collider using specific parameters.
        /// </summary>
        public abstract int Cast(Vector3 _direction, RaycastHit[] _buffer, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction);

        /// <summary>
        /// Get the current overlapping colliders.
        /// </summary>
        public abstract int Overlap(Collider[] _buffer, int _mask, QueryTriggerInteraction _triggerInteraction);
        #endregion

        #region Utility
        /// <summary>
        /// Get this collider world-space non-rotated extents.
        /// </summary>
        public abstract Vector3 GetExtents();

        /// <summary>
        /// Modifies the bounding box of this collider.
        /// </summary>
        /// <param name="_center">New center of the collider bounding box (measured in the object local space).</param>
        /// <param name="_size">New size of the collider bounding box (measured in the object local space).</param>
        public abstract void SetBounds(Vector3 _center, Vector3 _size);
        #endregion
    }

    internal class BoxColliderWrapper3D : ColliderWrapper3D {
        #region Global Members
        public readonly BoxCollider Collider = null;

        // -----------------------

        public BoxColliderWrapper3D(BoxCollider _collider) : base(_collider) {
            Collider = _collider;
        }
        #endregion

        #region Physics
        public override bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            Vector3 _offset = transform.rotation * Vector3.Scale(_direction, GetExtents());
            return Physics.Raycast(Collider.bounds.center + _offset, _direction.normalized, out _hit, _distance, _mask, _triggerInteraction);
        }

        public override int Cast(Vector3 _direction, RaycastHit[] _buffer, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            Vector3 _extents = GetExtents() - Physics.defaultContactOffset.ToVector3();
            return Physics.BoxCastNonAlloc(Collider.bounds.center, _extents, _direction.normalized, _buffer, transform.rotation, _distance, _mask, _triggerInteraction);
        }

        public override int Overlap(Collider[] _buffer, int _mask, QueryTriggerInteraction _triggerInteraction) {
            return Physics.OverlapBoxNonAlloc(Collider.bounds.center, GetExtents(), _buffer, transform.rotation, _mask, _triggerInteraction);
        }
        #endregion

        #region Utility
        public override Vector3 GetExtents() {
            return transform.TransformVector(Collider.size * .5f);
        }

        public override void SetBounds(Vector3 _center, Vector3 _size) {
            Collider.center = _center;
            Collider.size = _size;
        }
        #endregion
    }

    internal class CapsuleColliderWrapper3D : ColliderWrapper3D {
        #region Global Members
        public readonly CapsuleCollider Collider = null;

        // -----------------------

        public CapsuleColliderWrapper3D(CapsuleCollider _collider) : base(_collider) {
            Collider = _collider;
        }
        #endregion

        #region Physics
        public override bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            // Update position.
            Collider.transform.position = Collider.attachedRigidbody.position;
            _direction.Normalize();

            float _contactOffset = Physics.defaultContactOffset;
            Vector3 _position = Collider.ClosestPoint(Collider.bounds.center + (_direction * Collider.height)) - (_direction * _contactOffset);

            return Physics.Raycast(_position, _direction, out _hit, _distance + _contactOffset, _mask, _triggerInteraction);
        }

        public override int Cast(Vector3 _velocity, RaycastHit[] _buffer, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            Vector3 _offset = GetPointOffset();
            Vector3 _center = Collider.bounds.center;
            float _radius = Collider.radius - Physics.defaultContactOffset;

            return Physics.CapsuleCastNonAlloc(_center - _offset, _center + _offset, _radius, _velocity.normalized, _buffer, _distance, _mask, _triggerInteraction);
        }

        public override int Overlap(Collider[] _buffer, int _mask, QueryTriggerInteraction _triggerInteraction) {
            Vector3 _offset = GetPointOffset();
            Vector3 _center = Collider.bounds.center;

            return Physics.OverlapCapsuleNonAlloc(_center - _offset, _center + _offset, Collider.radius, _buffer, _mask, _triggerInteraction);
        }
        #endregion

        #region Utility
        public override Vector3 GetExtents() {
            float _radius = Collider.radius;
            float _height = Collider.height * .5f;
            Vector3 _extents;

            switch (Collider.direction) {
                // X axis.
                case 0:
                    _extents = new Vector3(_height, _radius, _radius);
                    break;

                // Y axis.
                case 1:
                    _extents = new Vector3(_radius, _height, _radius);
                    break;

                // Z axis.
                case 2:
                    _extents = new Vector3(_radius, _radius, _height);
                    break;

                // This never happen.
                default:
                    throw new InvalidCapsuleHeightException();
            }

            return transform.TransformVector(_extents);
        }

        public override void SetBounds(Vector3 _center, Vector3 _size) {
            Collider.center = _center;

            Collider.radius = _size.x;
            Collider.height = _size.y;
        }

        public Vector3 GetPointOffset() {
            float _height = (Collider.height * .5f) - Collider.radius;
            Vector3 _offset;

            switch (Collider.direction) {
                // X axis.
                case 0:
                    _offset = new Vector3(_height, 0f, 0f);
                    break;

                // Y axis.
                case 1:
                    _offset = new Vector3(0f, _height, 0f);
                    break;

                // Z axis.
                case 2:
                    _offset = new Vector3(0f, 0f, _height);
                    break;

                // This never happen.
                default:
                    throw new InvalidCapsuleHeightException();
            }

            return transform.TransformVector(_offset);
        }
        #endregion
    }

    internal class SphereColliderWrapper3D : ColliderWrapper3D {
        #region Global Members
        public readonly SphereCollider Collider = null;

        // -----------------------

        public SphereColliderWrapper3D(SphereCollider _collider) : base(_collider) {
            Collider = _collider;
        }
        #endregion

        #region Physics
        public override bool Raycast(Vector3 _direction, out RaycastHit _hit, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            _direction.Normalize();

            Vector3 _offset = transform.rotation * Vector3.Scale(_direction, GetExtents());
            return Physics.Raycast(Collider.bounds.center + _offset, _direction, out _hit, _distance, _mask, _triggerInteraction);
        }

        public override int Cast(Vector3 _velocity, RaycastHit[] _buffer, float _distance, int _mask, QueryTriggerInteraction _triggerInteraction) {
            float _radius = Collider.radius - Physics.defaultContactOffset;
            return Physics.SphereCastNonAlloc(Collider.bounds.center, _radius, _velocity.normalized, _buffer, _distance, _mask, _triggerInteraction);
        }

        public override int Overlap(Collider[] _buffer, int _mask, QueryTriggerInteraction _triggerInteraction) {
            return Physics.OverlapSphereNonAlloc(Collider.bounds.center, Collider.radius, _buffer, _mask, _triggerInteraction);
        }
        #endregion

        #region Utility
        public override Vector3 GetExtents() {
            float _radius = Collider.radius;
            Vector3 _extents = new Vector3(_radius, _radius, _radius);

            return transform.TransformVector(_extents);
        }

        public override void SetBounds(Vector3 _center, Vector3 _size) {
            Collider.center = _center;
            Collider.radius = _size.x;
        }
        #endregion
    }

    #region Exceptions
    /// <summary>
    /// Exception for any non-primitive collider, forbidding
    /// the usage of complex (cast or overlap) physics operations.
    /// </summary>
    public class NonPrimitiveColliderException : Exception {
        public NonPrimitiveColliderException() : base() { }

        public NonPrimitiveColliderException(string _message) : base(_message) { }

        public NonPrimitiveColliderException(string _message, Exception _innerException) : base(_message, _innerException) { }
    }

    /// <summary>
    /// Exception for invalid capsule height axis, making
    /// the associated collider cast or overlap operation impossible.
    /// </summary>
    public class InvalidCapsuleHeightException : Exception {
        public InvalidCapsuleHeightException() : base() { }

        public InvalidCapsuleHeightException(string _message) : base(_message) { }

        public InvalidCapsuleHeightException(string _message, Exception _innerException) : base(_message, _innerException) { }
    }
    #endregion
}
