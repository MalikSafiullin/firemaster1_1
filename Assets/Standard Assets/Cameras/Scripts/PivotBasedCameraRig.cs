using System;
using UnityEngine;


namespace UnityStandardAssets.Cameras
{
    public abstract class PivotBasedCameraRig : AbstractTargetFollower
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        protected Transform MCam; // the transform of the camera
        protected Transform MPivot; // the point at which the camera pivots around
        protected Vector3 MLastTargetPosition;


        protected virtual void Awake()
        {
            // find the camera in the object hierarchy
            MCam = GetComponentInChildren<Camera>().transform;
            MPivot = MCam.parent;
        }
    }
}
