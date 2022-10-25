using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    public class LandingGear : MonoBehaviour
    {

        private enum GearState
        {
            Raised = -1,
            Lowered = 1
        }

        // The landing gear can be raised and lowered at differing altitudes.
        // The gear is only lowered when descending, and only raised when climbing.

        // this script detects the raise/lower condition and sets a parameter on
        // the animator to actually play the animation to raise or lower the gear.

        public float raiseAtAltitude = 40;
        public float lowerAtAltitude = 40;

        private GearState _mState = GearState.Lowered;
        private Animator _mAnimator;
        private Rigidbody _mRigidbody;
        private AeroplaneController _mPlane;

        // Use this for initialization
        private void Start()
        {
            _mPlane = GetComponent<AeroplaneController>();
            _mAnimator = GetComponent<Animator>();
            _mRigidbody = GetComponent<Rigidbody>();
        }


        // Update is called once per frame
        private void Update()
        {
            if (_mState == GearState.Lowered && _mPlane.Altitude > raiseAtAltitude && _mRigidbody.velocity.y > 0)
            {
                _mState = GearState.Raised;
            }

            if (_mState == GearState.Raised && _mPlane.Altitude < lowerAtAltitude && _mRigidbody.velocity.y < 0)
            {
                _mState = GearState.Lowered;
            }

            // set the parameter on the animator controller to trigger the appropriate animation
            _mAnimator.SetInteger("GearState", (int) _mState);
        }
    }
}
