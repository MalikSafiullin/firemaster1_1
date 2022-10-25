using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Cameras
{
    public class ProtectCameraFromWallClip : MonoBehaviour
    {
        public float clipMoveTime = 0.05f;              // time taken to move when avoiding cliping (low value = fast, which it should be)
        public float returnTime = 0.4f;                 // time taken to move back towards desired position, when not clipping (typically should be a higher value than clipMoveTime)
        public float sphereCastRadius = 0.1f;           // the radius of the sphere used to test for object between camera and target
        public bool visualiseInEditor;                  // toggle for visualising the algorithm through lines for the raycast in the editor
        public float closestDistance = 0.5f;            // the closest distance the camera can be from the target
        public bool Protecting { get; private set; }    // used for determining if there is an object between the target and the camera
        public string dontClipTag = "Player";           // don't clip against objects with this tag (useful for not clipping against the targeted object)

        private Transform _mCam;                  // the transform of the camera
        private Transform _mPivot;                // the point at which the camera pivots around
        private float _mOriginalDist;             // the original distance to the camera before any modification are made
        private float _mMoveVelocity;             // the velocity at which the camera moved
        private float _mCurrentDist;              // the current distance from the camera to the target
        private Ray _mRay = new Ray();                        // the ray used in the lateupdate for casting between the camera and the target
        private RaycastHit[] _mHits;              // the hits between the camera and the target
        private RayHitComparer _mRayHitComparer;  // variable to compare raycast hit distances


        private void Start()
        {
            // find the camera in the object hierarchy
            _mCam = GetComponentInChildren<Camera>().transform;
            _mPivot = _mCam.parent;
            _mOriginalDist = _mCam.localPosition.magnitude;
            _mCurrentDist = _mOriginalDist;

            // create a new RayHitComparer
            _mRayHitComparer = new RayHitComparer();
        }


        private void LateUpdate()
        {
            // initially set the target distance
            float targetDist = _mOriginalDist;

            _mRay.origin = _mPivot.position + _mPivot.forward*sphereCastRadius;
            _mRay.direction = -_mPivot.forward;

            // initial check to see if start of spherecast intersects anything
            var cols = Physics.OverlapSphere(_mRay.origin, sphereCastRadius);

            bool initialIntersect = false;
            bool hitSomething = false;

            // loop through all the collisions to check if something we care about
            for (int i = 0; i < cols.Length; i++)
            {
                if ((!cols[i].isTrigger) &&
                    !(cols[i].attachedRigidbody != null && cols[i].attachedRigidbody.CompareTag(dontClipTag)))
                {
                    initialIntersect = true;
                    break;
                }
            }

            // if there is a collision
            if (initialIntersect)
            {
                _mRay.origin += _mPivot.forward*sphereCastRadius;

                // do a raycast and gather all the intersections
                _mHits = Physics.RaycastAll(_mRay, _mOriginalDist - sphereCastRadius);
            }
            else
            {
                // if there was no collision do a sphere cast to see if there were any other collisions
                _mHits = Physics.SphereCastAll(_mRay, sphereCastRadius, _mOriginalDist + sphereCastRadius);
            }

            // sort the collisions by distance
            Array.Sort(_mHits, _mRayHitComparer);

            // set the variable used for storing the closest to be as far as possible
            float nearest = Mathf.Infinity;

            // loop through all the collisions
            for (int i = 0; i < _mHits.Length; i++)
            {
                // only deal with the collision if it was closer than the previous one, not a trigger, and not attached to a rigidbody tagged with the dontClipTag
                if (_mHits[i].distance < nearest && (!_mHits[i].collider.isTrigger) &&
                    !(_mHits[i].collider.attachedRigidbody != null &&
                      _mHits[i].collider.attachedRigidbody.CompareTag(dontClipTag)))
                {
                    // change the nearest collision to latest
                    nearest = _mHits[i].distance;
                    targetDist = -_mPivot.InverseTransformPoint(_mHits[i].point).z;
                    hitSomething = true;
                }
            }

            // visualise the cam clip effect in the editor
            if (hitSomething)
            {
                Debug.DrawRay(_mRay.origin, -_mPivot.forward*(targetDist + sphereCastRadius), Color.red);
            }

            // hit something so move the camera to a better position
            Protecting = hitSomething;
            _mCurrentDist = Mathf.SmoothDamp(_mCurrentDist, targetDist, ref _mMoveVelocity,
                                           _mCurrentDist > targetDist ? clipMoveTime : returnTime);
            _mCurrentDist = Mathf.Clamp(_mCurrentDist, closestDistance, _mOriginalDist);
            _mCam.localPosition = -Vector3.forward*_mCurrentDist;
        }


        // comparer for check distances in ray cast hits
        public class RayHitComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((RaycastHit) x).distance.CompareTo(((RaycastHit) y).distance);
            }
        }
    }
}
