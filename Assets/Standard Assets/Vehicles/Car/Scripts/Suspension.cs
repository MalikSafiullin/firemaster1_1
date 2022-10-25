using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    // this script is specific to the car supplied in the the assets
    // it controls the suspension hub to make it move with the wheel are it goes over bumps
    public class Suspension : MonoBehaviour
    {
        public GameObject wheel; // The wheel that the script needs to referencing to get the postion for the suspension


        private Vector3 _mTargetOriginalPosition;
        private Vector3 _mOrigin;


        private void Start()
        {
            _mTargetOriginalPosition = wheel.transform.localPosition;
            _mOrigin = transform.localPosition;
        }


        private void Update()
        {
            transform.localPosition = _mOrigin + (wheel.transform.localPosition - _mTargetOriginalPosition);
        }
    }
}
