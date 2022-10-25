using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneAiControl : MonoBehaviour
    {
        // This script represents an AI 'pilot' capable of flying the plane towards a designated target.
        // It sends the equivalent of the inputs that a user would send to the Aeroplane controller.
        [FormerlySerializedAs("m_RollSensitivity")] [SerializeField] private float mRollSensitivity = .2f;         // How sensitively the AI applies the roll controls
        [FormerlySerializedAs("m_PitchSensitivity")] [SerializeField] private float mPitchSensitivity = .5f;        // How sensitively the AI applies the pitch controls
        [FormerlySerializedAs("m_LateralWanderDistance")] [SerializeField] private float mLateralWanderDistance = 5;     // The amount that the plane can wander by when heading for a target
        [FormerlySerializedAs("m_LateralWanderSpeed")] [SerializeField] private float mLateralWanderSpeed = 0.11f;    // The speed at which the plane will wander laterally
        [FormerlySerializedAs("m_MaxClimbAngle")] [SerializeField] private float mMaxClimbAngle = 45;            // The maximum angle that the AI will attempt to make plane can climb at
        [FormerlySerializedAs("m_MaxRollAngle")] [SerializeField] private float mMaxRollAngle = 45;             // The maximum angle that the AI will attempt to u
        [FormerlySerializedAs("m_SpeedEffect")] [SerializeField] private float mSpeedEffect = 0.01f;           // This increases the effect of the controls based on the plane's speed.
        [FormerlySerializedAs("m_TakeoffHeight")] [SerializeField] private float mTakeoffHeight = 20;            // the AI will fly straight and only pitch upwards until reaching this height
        [FormerlySerializedAs("m_Target")] [SerializeField] private Transform mTarget;                    // the target to fly towards

        private AeroplaneController _mAeroplaneController;  // The aeroplane controller that is used to move the plane
        private float _mRandomPerlin;                       // Used for generating random point on perlin noise so that the plane will wander off path slightly
        private bool _mTakenOff;                            // Has the plane taken off yet


        // setup script properties
        private void Awake()
        {
            // get the reference to the aeroplane controller, so we can send move input to it and read its current state.
            _mAeroplaneController = GetComponent<AeroplaneController>();

            // pick a random perlin starting point for lateral wandering
            _mRandomPerlin = Random.Range(0f, 100f);
        }


        // reset the object to sensible values
        public void Reset()
        {
            _mTakenOff = false;
        }


        // fixed update is called in time with the physics system update
        private void FixedUpdate()
        {
            if (mTarget != null)
            {
                // make the plane wander from the path, useful for making the AI seem more human, less robotic.
                Vector3 targetPos = mTarget.position +
                                    transform.right*
                                    (Mathf.PerlinNoise(Time.time*mLateralWanderSpeed, _mRandomPerlin)*2 - 1)*
                                    mLateralWanderDistance;

                // adjust the yaw and pitch towards the target
                Vector3 localTarget = transform.InverseTransformPoint(targetPos);
                float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
                float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);


                // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
                targetAnglePitch = Mathf.Clamp(targetAnglePitch, -mMaxClimbAngle*Mathf.Deg2Rad,
                                               mMaxClimbAngle*Mathf.Deg2Rad);

                // calculate the difference between current pitch and desired pitch
                float changePitch = targetAnglePitch - _mAeroplaneController.PitchAngle;

                // AI always applies gentle forward throttle
                const float throttleInput = 0.5f;

                // AI applies elevator control (pitch, rotation around x) to reach the target angle
                float pitchInput = changePitch*mPitchSensitivity;

                // clamp the planes roll
                float desiredRoll = Mathf.Clamp(targetAngleYaw, -mMaxRollAngle*Mathf.Deg2Rad, mMaxRollAngle*Mathf.Deg2Rad);
                float yawInput = 0;
                float rollInput = 0;
                if (!_mTakenOff)
                {
                    // If the planes altitude is above m_TakeoffHeight we class this as taken off
                    if (_mAeroplaneController.Altitude > mTakeoffHeight)
                    {
                        _mTakenOff = true;
                    }
                }
                else
                {
                    // now we have taken off to a safe height, we can use the rudder and ailerons to yaw and roll
                    yawInput = targetAngleYaw;
                    rollInput = -(_mAeroplaneController.RollAngle - desiredRoll)*mRollSensitivity;
                }

                // adjust how fast the AI is changing the controls based on the speed. Faster speed = faster on the controls.
                float currentSpeedEffect = 1 + (_mAeroplaneController.ForwardSpeed*mSpeedEffect);
                rollInput *= currentSpeedEffect;
                pitchInput *= currentSpeedEffect;
                yawInput *= currentSpeedEffect;

                // pass the current input to the plane (false = because AI never uses air brakes!)
                _mAeroplaneController.Move(rollInput, pitchInput, yawInput, throttleInput, false);
            }
            else
            {
                // no target set, send zeroed input to the planeW
                _mAeroplaneController.Move(0, 0, 0, 0, false);
            }
        }


        // allows other scripts to set the plane's target
        public void SetTarget(Transform target)
        {
            mTarget = target;
        }
    }
}
