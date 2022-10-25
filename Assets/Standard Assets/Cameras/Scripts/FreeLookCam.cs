using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Cameras
{
    public class FreeLookCam : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [FormerlySerializedAs("m_MoveSpeed")] [SerializeField] private float mMoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [FormerlySerializedAs("m_TurnSpeed")] [Range(0f, 10f)] [SerializeField] private float mTurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [FormerlySerializedAs("m_TurnSmoothing")] [SerializeField] private float mTurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [FormerlySerializedAs("m_TiltMax")] [SerializeField] private float mTiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [FormerlySerializedAs("m_TiltMin")] [SerializeField] private float mTiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [FormerlySerializedAs("m_LockCursor")] [SerializeField] private bool mLockCursor = false;                   // Whether the cursor should be hidden and locked.
        [FormerlySerializedAs("m_VerticalAutoReturn")] [SerializeField] private bool mVerticalAutoReturn = false;           // set wether or not the vertical axis should auto return

        private float _mLookAngle;                    // The rig's y axis rotation.
        private float _mTiltAngle;                    // The pivot's x axis rotation.
        private const float K_LOOK_DISTANCE = 100f;    // How far in front of the pivot the character's look target is.
		private Vector3 _mPivotEulers;
		private Quaternion _mPivotTargetRot;
		private Quaternion _mTransformTargetRot;

        protected override void Awake()
        {
            base.Awake();
            // Lock or unlock the cursor.
            Cursor.lockState = mLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !mLockCursor;
			_mPivotEulers = MPivot.rotation.eulerAngles;

	        _mPivotTargetRot = MPivot.transform.localRotation;
			_mTransformTargetRot = transform.localRotation;
        }


        protected void Update()
        {
            HandleRotationMovement();
            if (mLockCursor && Input.GetMouseButtonUp(0))
            {
                Cursor.lockState = mLockCursor ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !mLockCursor;
            }
        }


        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        protected override void FollowTarget(float deltaTime)
        {
            if (mTarget == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, mTarget.position, deltaTime*mMoveSpeed);
        }


        private void HandleRotationMovement()
        {
			if(Time.timeScale < float.Epsilon)
			return;

            // Read the user input
            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");

            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            _mLookAngle += x*mTurnSpeed;

            // Rotate the rig (the root object) around Y axis only:
            _mTransformTargetRot = Quaternion.Euler(0f, _mLookAngle, 0f);

            if (mVerticalAutoReturn)
            {
                // For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
                // on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
                // we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
                _mTiltAngle = y > 0 ? Mathf.Lerp(0, -mTiltMin, y) : Mathf.Lerp(0, mTiltMax, -y);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                _mTiltAngle -= y*mTurnSpeed;
                // and make sure the new value is within the tilt range
                _mTiltAngle = Mathf.Clamp(_mTiltAngle, -mTiltMin, mTiltMax);
            }

            // Tilt input around X is applied to the pivot (the child of this object)
			_mPivotTargetRot = Quaternion.Euler(_mTiltAngle, _mPivotEulers.y , _mPivotEulers.z);

			if (mTurnSmoothing > 0)
			{
				MPivot.localRotation = Quaternion.Slerp(MPivot.localRotation, _mPivotTargetRot, mTurnSmoothing * Time.deltaTime);
				transform.localRotation = Quaternion.Slerp(transform.localRotation, _mTransformTargetRot, mTurnSmoothing * Time.deltaTime);
			}
			else
			{
				MPivot.localRotation = _mPivotTargetRot;
				transform.localRotation = _mTransformTargetRot;
			}
        }
    }
}
