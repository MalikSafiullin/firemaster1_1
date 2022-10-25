using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Utility
{
    public class SimpleMouseRotator : MonoBehaviour
    {
        // A mouselook behaviour with constraints which operate relative to
        // this gameobject's initial rotation.
        // Only rotates around local X and Y.
        // Works in local coordinates, so if this object is parented
        // to another moving gameobject, its local constraints will
        // operate correctly
        // (Think: looking out the side window of a car, or a gun turret
        // on a moving spaceship with a limited angular range)
        // to have no constraints on an axis, set the rotationRange to 360 or greater.
        public Vector2 rotationRange = new Vector3(70, 70);
        public float rotationSpeed = 10;
        public float dampingTime = 0.2f;
        public bool autoZeroVerticalOnMobile = true;
        public bool autoZeroHorizontalOnMobile = false;
        public bool relative = true;
        
        
        private Vector3 _mTargetAngles;
        private Vector3 _mFollowAngles;
        private Vector3 _mFollowVelocity;
        private Quaternion _mOriginalRotation;


        private void Start()
        {
            _mOriginalRotation = transform.localRotation;
        }


        private void Update()
        {
            // we make initial calculations from the original local rotation
            transform.localRotation = _mOriginalRotation;

            // read input from mouse or mobile controls
            float inputH;
            float inputV;
            if (relative)
            {
                inputH = CrossPlatformInputManager.GetAxis("Mouse X");
                inputV = CrossPlatformInputManager.GetAxis("Mouse Y");

                // wrap values to avoid springing quickly the wrong way from positive to negative
                if (_mTargetAngles.y > 180)
                {
                    _mTargetAngles.y -= 360;
                    _mFollowAngles.y -= 360;
                }
                if (_mTargetAngles.x > 180)
                {
                    _mTargetAngles.x -= 360;
                    _mFollowAngles.x -= 360;
                }
                if (_mTargetAngles.y < -180)
                {
                    _mTargetAngles.y += 360;
                    _mFollowAngles.y += 360;
                }
                if (_mTargetAngles.x < -180)
                {
                    _mTargetAngles.x += 360;
                    _mFollowAngles.x += 360;
                }

#if MOBILE_INPUT
            // on mobile, sometimes we want input mapped directly to tilt value,
            // so it springs back automatically when the look input is released.
			if (autoZeroHorizontalOnMobile) {
				m_TargetAngles.y = Mathf.Lerp (-rotationRange.y * 0.5f, rotationRange.y * 0.5f, inputH * .5f + .5f);
			} else {
				m_TargetAngles.y += inputH * rotationSpeed;
			}
			if (autoZeroVerticalOnMobile) {
				m_TargetAngles.x = Mathf.Lerp (-rotationRange.x * 0.5f, rotationRange.x * 0.5f, inputV * .5f + .5f);
			} else {
				m_TargetAngles.x += inputV * rotationSpeed;
			}
#else
                // with mouse input, we have direct control with no springback required.
                _mTargetAngles.y += inputH*rotationSpeed;
                _mTargetAngles.x += inputV*rotationSpeed;
#endif

                // clamp values to allowed range
                _mTargetAngles.y = Mathf.Clamp(_mTargetAngles.y, -rotationRange.y*0.5f, rotationRange.y*0.5f);
                _mTargetAngles.x = Mathf.Clamp(_mTargetAngles.x, -rotationRange.x*0.5f, rotationRange.x*0.5f);
            }
            else
            {
                inputH = Input.mousePosition.x;
                inputV = Input.mousePosition.y;

                // set values to allowed range
                _mTargetAngles.y = Mathf.Lerp(-rotationRange.y*0.5f, rotationRange.y*0.5f, inputH/Screen.width);
                _mTargetAngles.x = Mathf.Lerp(-rotationRange.x*0.5f, rotationRange.x*0.5f, inputV/Screen.height);
            }

            // smoothly interpolate current values to target angles
            _mFollowAngles = Vector3.SmoothDamp(_mFollowAngles, _mTargetAngles, ref _mFollowVelocity, dampingTime);

            // update the actual gameobject's rotation
            transform.localRotation = _mOriginalRotation*Quaternion.Euler(-_mFollowAngles.x, _mFollowAngles.y, 0);
        }
    }
}
