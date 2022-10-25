using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneUserControl4Axis : MonoBehaviour
    {
        // these max angles are only used on mobile, due to the way pitch and roll input are handled
        public float maxRollAngle = 80;
        public float maxPitchAngle = 80;

        // reference to the aeroplane that we're controlling
        private AeroplaneController _mAeroplane;
        private float _mThrottle;
        private bool _mAirBrakes;
        private float _mYaw;


        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            _mAeroplane = GetComponent<AeroplaneController>();
        }


        private void FixedUpdate()
        {
            // Read input for the pitch, yaw, roll and throttle of the aeroplane.
            float roll = CrossPlatformInputManager.GetAxis("Mouse X");
            float pitch = CrossPlatformInputManager.GetAxis("Mouse Y");
            _mAirBrakes = CrossPlatformInputManager.GetButton("Fire1");
            _mYaw = CrossPlatformInputManager.GetAxis("Horizontal");
            _mThrottle = CrossPlatformInputManager.GetAxis("Vertical");
#if MOBILE_INPUT
        AdjustInputForMobileControls(ref roll, ref pitch, ref m_Throttle);
#endif
            // Pass the input to the aeroplane
            _mAeroplane.Move(roll, pitch, _mYaw, _mThrottle, _mAirBrakes);
        }


        private void AdjustInputForMobileControls(ref float roll, ref float pitch, ref float throttle)
        {
            // because mobile tilt is used for roll and pitch, we help out by
            // assuming that a centered level device means the user
            // wants to fly straight and level!

            // this means on mobile, the input represents the *desired* roll angle of the aeroplane,
            // and the roll input is calculated to achieve that.
            // whereas on non-mobile, the input directly controls the roll of the aeroplane.

            float intendedRollAngle = roll*maxRollAngle*Mathf.Deg2Rad;
            float intendedPitchAngle = pitch*maxPitchAngle*Mathf.Deg2Rad;
            roll = Mathf.Clamp((intendedRollAngle - _mAeroplane.RollAngle), -1, 1);
            pitch = Mathf.Clamp((intendedPitchAngle - _mAeroplane.PitchAngle), -1, 1);
        }
    }
}
