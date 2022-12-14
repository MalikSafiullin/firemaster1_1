using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarAIControl : MonoBehaviour
    {
        public enum BrakeCondition
        {
            NeverBrake,                 // the car simply accelerates at full throttle all the time.
            TargetDirectionDifference,  // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.
            TargetDistance,             // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                                        // head for a stationary target and come to rest when it arrives there.
        }

        // This script provides input to the car controller in the same way that the user control script does.
        // As such, it is really 'driving' the car, with no special physics or animation tricks to make the car behave properly.

        // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
        // in speed and direction while driving towards their target.

        [FormerlySerializedAs("m_CautiousSpeedFactor")] [SerializeField] [Range(0, 1)] private float mCautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
        [FormerlySerializedAs("m_CautiousMaxAngle")] [SerializeField] [Range(0, 180)] private float mCautiousMaxAngle = 50f;                  // angle of approaching corner to treat as warranting maximum caution
        [FormerlySerializedAs("m_CautiousMaxDistance")] [SerializeField] private float mCautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
        [FormerlySerializedAs("m_CautiousAngularVelocityFactor")] [SerializeField] private float mCautiousAngularVelocityFactor = 30f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
        [FormerlySerializedAs("m_SteerSensitivity")] [SerializeField] private float mSteerSensitivity = 0.05f;                                // how sensitively the AI uses steering input to turn to the desired direction
        [FormerlySerializedAs("m_AccelSensitivity")] [SerializeField] private float mAccelSensitivity = 0.04f;                                // How sensitively the AI uses the accelerator to reach the current desired speed
        [FormerlySerializedAs("m_BrakeSensitivity")] [SerializeField] private float mBrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
        [FormerlySerializedAs("m_LateralWanderDistance")] [SerializeField] private float mLateralWanderDistance = 3f;                              // how far the car will wander laterally towards its target
        [FormerlySerializedAs("m_LateralWanderSpeed")] [SerializeField] private float mLateralWanderSpeed = 0.1f;                               // how fast the lateral wandering will fluctuate
        [FormerlySerializedAs("m_AccelWanderAmount")] [SerializeField] [Range(0, 1)] private float mAccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander
        [FormerlySerializedAs("m_AccelWanderSpeed")] [SerializeField] private float mAccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
        [FormerlySerializedAs("m_BrakeCondition")] [SerializeField] private BrakeCondition mBrakeCondition = BrakeCondition.TargetDistance; // what should the AI consider when accelerating/braking?
        [FormerlySerializedAs("m_Driving")] [SerializeField] private bool mDriving;                                                  // whether the AI is currently actively driving or stopped.
        [FormerlySerializedAs("m_Target")] [SerializeField] private Transform mTarget;                                              // 'target' the target object to aim for.
        [FormerlySerializedAs("m_StopWhenTargetReached")] [SerializeField] private bool mStopWhenTargetReached;                                    // should we stop driving when we reach the target?
        [FormerlySerializedAs("m_ReachTargetThreshold")] [SerializeField] private float mReachTargetThreshold = 2;                                // proximity to target to consider we 'reached' it, and stop driving.

        private float _mRandomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
        private CarController _mCarController;    // Reference to actual car controller we are controlling
        private float _mAvoidOtherCarTime;        // time until which to avoid the car we recently collided with
        private float _mAvoidOtherCarSlowdown;    // how much to slow down due to colliding with another car, whilst avoiding
        private float _mAvoidPathOffset;          // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding
        private Rigidbody _mRigidbody;


        private void Awake()
        {
            // get the car controller reference
            _mCarController = GetComponent<CarController>();

            // give the random perlin a random value
            _mRandomPerlin = Random.value*100;

            _mRigidbody = GetComponent<Rigidbody>();
        }


        private void FixedUpdate()
        {
            if (mTarget == null || !mDriving)
            {
                // Car should not be moving,
                // use handbrake to stop
                _mCarController.Move(0, 0, -1f, 1f);
            }
            else
            {
                Vector3 fwd = transform.forward;
                if (_mRigidbody.velocity.magnitude > _mCarController.MaxSpeed*0.1f)
                {
                    fwd = _mRigidbody.velocity;
                }

                float desiredSpeed = _mCarController.MaxSpeed;

                // now it's time to decide if we should be slowing down...
                switch (mBrakeCondition)
                {
                    case BrakeCondition.TargetDirectionDifference:
                        {
                            // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

                            // check out the angle of our target compared to the current direction of the car
                            float approachingCornerAngle = Vector3.Angle(mTarget.forward, fwd);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle = _mRigidbody.angularVelocity.magnitude*mCautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired = Mathf.InverseLerp(0, mCautiousMaxAngle,
                                                                           Mathf.Max(spinningAngle,
                                                                                     approachingCornerAngle));
                            desiredSpeed = Mathf.Lerp(_mCarController.MaxSpeed, _mCarController.MaxSpeed*mCautiousSpeedFactor,
                                                      cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.TargetDistance:
                        {
                            // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                            // head for a stationary target and come to rest when it arrives there.

                            // check out the distance to target
                            Vector3 delta = mTarget.position - transform.position;
                            float distanceCautiousFactor = Mathf.InverseLerp(mCautiousMaxDistance, 0, delta.magnitude);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle = _mRigidbody.angularVelocity.magnitude*mCautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired = Mathf.Max(
                                Mathf.InverseLerp(0, mCautiousMaxAngle, spinningAngle), distanceCautiousFactor);
                            desiredSpeed = Mathf.Lerp(_mCarController.MaxSpeed, _mCarController.MaxSpeed*mCautiousSpeedFactor,
                                                      cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.NeverBrake:
                        break;
                }

                // Evasive action due to collision with other cars:

                // our target position starts off as the 'real' target position
                Vector3 offsetTargetPos = mTarget.position;

                // if are we currently taking evasive action to prevent being stuck against another car:
                if (Time.time < _mAvoidOtherCarTime)
                {
                    // slow down if necessary (if we were behind the other car when collision occured)
                    desiredSpeed *= _mAvoidOtherCarSlowdown;

                    // and veer towards the side of our path-to-target that is away from the other car
                    offsetTargetPos += mTarget.right*_mAvoidPathOffset;
                }
                else
                {
                    // no need for evasive action, we can just wander across the path-to-target in a random way,
                    // which can help prevent AI from seeming too uniform and robotic in their driving
                    offsetTargetPos += mTarget.right*
                                       (Mathf.PerlinNoise(Time.time*mLateralWanderSpeed, _mRandomPerlin)*2 - 1)*
                                       mLateralWanderDistance;
                }

                // use different sensitivity depending on whether accelerating or braking:
                float accelBrakeSensitivity = (desiredSpeed < _mCarController.CurrentSpeed)
                                                  ? mBrakeSensitivity
                                                  : mAccelSensitivity;

                // decide the actual amount of accel/brake input to achieve desired speed.
                float accel = Mathf.Clamp((desiredSpeed - _mCarController.CurrentSpeed)*accelBrakeSensitivity, -1, 1);

                // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
                // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
                accel *= (1 - mAccelWanderAmount) +
                         (Mathf.PerlinNoise(Time.time*mAccelWanderSpeed, _mRandomPerlin)*mAccelWanderAmount);

                // calculate the local-relative position of the target, to steer towards
                Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

                // work out the local angle towards the target
                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z)*Mathf.Rad2Deg;

                // get the amount of steering needed to aim the car towards the target
                float steer = Mathf.Clamp(targetAngle*mSteerSensitivity, -1, 1)*Mathf.Sign(_mCarController.CurrentSpeed);

                // feed input to the car controller.
                _mCarController.Move(steer, accel, accel, 0f);

                // if appropriate, stop driving when we're close enough to the target.
                if (mStopWhenTargetReached && localTarget.magnitude < mReachTargetThreshold)
                {
                    mDriving = false;
                }
            }
        }


        private void OnCollisionStay(Collision col)
        {
            // detect collision against other cars, so that we can take evasive action
            if (col.rigidbody != null)
            {
                var otherAI = col.rigidbody.GetComponent<CarAIControl>();
                if (otherAI != null)
                {
                    // we'll take evasive action for 1 second
                    _mAvoidOtherCarTime = Time.time + 1;

                    // but who's in front?...
                    if (Vector3.Angle(transform.forward, otherAI.transform.position - transform.position) < 90)
                    {
                        // the other ai is in front, so it is only good manners that we ought to brake...
                        _mAvoidOtherCarSlowdown = 0.5f;
                    }
                    else
                    {
                        // we're in front! ain't slowing down for anybody...
                        _mAvoidOtherCarSlowdown = 1;
                    }

                    // both cars should take evasive action by driving along an offset from the path centre,
                    // away from the other car
                    var otherCarLocalDelta = transform.InverseTransformPoint(otherAI.transform.position);
                    float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
                    _mAvoidPathOffset = mLateralWanderDistance*-Mathf.Sign(otherCarAngle);
                }
            }
        }


        public void SetTarget(Transform target)
        {
            mTarget = target;
            mDriving = true;
        }
    }
}
