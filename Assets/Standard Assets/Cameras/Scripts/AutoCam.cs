using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR

#endif

namespace UnityStandardAssets.Cameras
{
    [ExecuteInEditMode]
    public class AutoCam : PivotBasedCameraRig
    {
        [FormerlySerializedAs("m_MoveSpeed")] [SerializeField] private float mMoveSpeed = 3; // How fast the rig will move to keep up with target's position
        [FormerlySerializedAs("m_TurnSpeed")] [SerializeField] private float mTurnSpeed = 1; // How fast the rig will turn to keep up with target's rotation
        [FormerlySerializedAs("m_RollSpeed")] [SerializeField] private float mRollSpeed = 0.2f;// How fast the rig will roll (around Z axis) to match target's roll.
        [FormerlySerializedAs("m_FollowVelocity")] [SerializeField] private bool mFollowVelocity = false;// Whether the rig will rotate in the direction of the target's velocity.
        [FormerlySerializedAs("m_FollowTilt")] [SerializeField] private bool mFollowTilt = true; // Whether the rig will tilt (around X axis) with the target.
        [FormerlySerializedAs("m_SpinTurnLimit")] [SerializeField] private float mSpinTurnLimit = 90;// The threshold beyond which the camera stops following the target's rotation. (used in situations where a car spins out, for example)
        [FormerlySerializedAs("m_TargetVelocityLowerLimit")] [SerializeField] private float mTargetVelocityLowerLimit = 4f;// the minimum velocity above which the camera turns towards the object's velocity. Below this we use the object's forward direction.
        [FormerlySerializedAs("m_SmoothTurnTime")] [SerializeField] private float mSmoothTurnTime = 0.2f; // the smoothing for the camera's rotation

        private float _mLastFlatAngle; // The relative angle of the target and the rig from the previous frame.
        private float _mCurrentTurnAmount; // How much to turn the camera
        private float _mTurnSpeedVelocityChange; // The change in the turn speed velocity
        private Vector3 _mRollUp = Vector3.up;// The roll of the camera around the z axis ( generally this will always just be up )


        protected override void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || mTarget == null)
            {
                return;
            }

            // initialise some vars, we'll be modifying these in a moment
            var targetForward = mTarget.forward;
            var targetUp = mTarget.up;

            if (mFollowVelocity && Application.isPlaying)
            {
                // in follow velocity mode, the camera's rotation is aligned towards the object's velocity direction
                // but only if the object is traveling faster than a given threshold.

                if (TargetRigidbody.velocity.magnitude > mTargetVelocityLowerLimit)
                {
                    // velocity is high enough, so we'll use the target's velocty
                    targetForward = TargetRigidbody.velocity.normalized;
                    targetUp = Vector3.up;
                }
                else
                {
                    targetUp = Vector3.up;
                }
                _mCurrentTurnAmount = Mathf.SmoothDamp(_mCurrentTurnAmount, 1, ref _mTurnSpeedVelocityChange, mSmoothTurnTime);
            }
            else
            {
                // we're in 'follow rotation' mode, where the camera rig's rotation follows the object's rotation.

                // This section allows the camera to stop following the target's rotation when the target is spinning too fast.
                // eg when a car has been knocked into a spin. The camera will resume following the rotation
                // of the target when the target's angular velocity slows below the threshold.
                var currentFlatAngle = Mathf.Atan2(targetForward.x, targetForward.z)*Mathf.Rad2Deg;
                if (mSpinTurnLimit > 0)
                {
                    var targetSpinSpeed = Mathf.Abs(Mathf.DeltaAngle(_mLastFlatAngle, currentFlatAngle))/deltaTime;
                    var desiredTurnAmount = Mathf.InverseLerp(mSpinTurnLimit, mSpinTurnLimit*0.75f, targetSpinSpeed);
                    var turnReactSpeed = (_mCurrentTurnAmount > desiredTurnAmount ? .1f : 1f);
                    if (Application.isPlaying)
                    {
                        _mCurrentTurnAmount = Mathf.SmoothDamp(_mCurrentTurnAmount, desiredTurnAmount,
                                                             ref _mTurnSpeedVelocityChange, turnReactSpeed);
                    }
                    else
                    {
                        // for editor mode, smoothdamp won't work because it uses deltaTime internally
                        _mCurrentTurnAmount = desiredTurnAmount;
                    }
                }
                else
                {
                    _mCurrentTurnAmount = 1;
                }
                _mLastFlatAngle = currentFlatAngle;
            }

            // camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, mTarget.position, deltaTime*mMoveSpeed);

            // camera's rotation is split into two parts, which can have independend speed settings:
            // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
            if (!mFollowTilt)
            {
                targetForward.y = 0;
                if (targetForward.sqrMagnitude < float.Epsilon)
                {
                    targetForward = transform.forward;
                }
            }
            var rollRotation = Quaternion.LookRotation(targetForward, _mRollUp);

            // and aligning with the target object's up direction (i.e. its 'roll')
            _mRollUp = mRollSpeed > 0 ? Vector3.Slerp(_mRollUp, targetUp, mRollSpeed*deltaTime) : Vector3.up;
            transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, mTurnSpeed*_mCurrentTurnAmount*deltaTime);
        }
    }
}
