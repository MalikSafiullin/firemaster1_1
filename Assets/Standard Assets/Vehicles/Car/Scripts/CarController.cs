using System;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Car
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        Mph,
        Kph
    }

    public class CarController : MonoBehaviour
    {
        [FormerlySerializedAs("m_CarDriveType")] [SerializeField] private CarDriveType mCarDriveType = CarDriveType.FourWheelDrive;
        [FormerlySerializedAs("m_WheelColliders")] [SerializeField] private WheelCollider[] mWheelColliders = new WheelCollider[4];
        [FormerlySerializedAs("m_WheelMeshes")] [SerializeField] private GameObject[] mWheelMeshes = new GameObject[4];
        [FormerlySerializedAs("m_WheelEffects")] [SerializeField] private WheelEffects[] mWheelEffects = new WheelEffects[4];
        [FormerlySerializedAs("m_CentreOfMassOffset")] [SerializeField] private Vector3 mCentreOfMassOffset;
        [FormerlySerializedAs("m_MaximumSteerAngle")] [SerializeField] private float mMaximumSteerAngle;
        [FormerlySerializedAs("m_SteerHelper")] [Range(0, 1)] [SerializeField] private float mSteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [FormerlySerializedAs("m_TractionControl")] [Range(0, 1)] [SerializeField] private float mTractionControl; // 0 is no traction control, 1 is full interference
        [FormerlySerializedAs("m_FullTorqueOverAllWheels")] [SerializeField] private float mFullTorqueOverAllWheels;
        [FormerlySerializedAs("m_ReverseTorque")] [SerializeField] private float mReverseTorque;
        [FormerlySerializedAs("m_MaxHandbrakeTorque")] [SerializeField] private float mMaxHandbrakeTorque;
        [FormerlySerializedAs("m_Downforce")] [SerializeField] private float mDownforce = 100f;
        [FormerlySerializedAs("m_SpeedType")] [SerializeField] private SpeedType mSpeedType;
        [FormerlySerializedAs("m_Topspeed")] [SerializeField] private float mTopspeed = 200;
        [SerializeField] private static int _noOfGears = 5;
        [FormerlySerializedAs("m_RevRangeBoundary")] [SerializeField] private float mRevRangeBoundary = 1f;
        [FormerlySerializedAs("m_SlipLimit")] [SerializeField] private float mSlipLimit;
        [FormerlySerializedAs("m_BrakeTorque")] [SerializeField] private float mBrakeTorque;

        private Quaternion[] _mWheelMeshLocalRotations;
        private Vector3 _mPrevpos, _mPos;
        private float _mSteerAngle;
        private int _mGearNum;
        private float _mGearFactor;
        private float _mOldRotation;
        private float _mCurrentTorque;
        private Rigidbody _mRigidbody;
        private const float K_REVERSING_THRESHOLD = 0.01f;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle{ get { return _mSteerAngle; }}
        public float CurrentSpeed{ get { return _mRigidbody.velocity.magnitude*2.23693629f; }}
        public float MaxSpeed{get { return mTopspeed; }}
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        // Use this for initialization
        private void Start()
        {
            _mWheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                _mWheelMeshLocalRotations[i] = mWheelMeshes[i].transform.localRotation;
            }
            mWheelColliders[0].attachedRigidbody.centerOfMass = mCentreOfMassOffset;

            mMaxHandbrakeTorque = float.MaxValue;

            _mRigidbody = GetComponent<Rigidbody>();
            _mCurrentTorque = mFullTorqueOverAllWheels - (mTractionControl*mFullTorqueOverAllWheels);
        }


        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) _noOfGears)*(_mGearNum + 1);
            float downgearlimit = (1/(float) _noOfGears)*_mGearNum;

            if (_mGearNum > 0 && f < downgearlimit)
            {
                _mGearNum--;
            }

            if (f > upgearlimit && (_mGearNum < (_noOfGears - 1)))
            {
                _mGearNum++;
            }
        }


        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }


        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }


        private void CalculateGearFactor()
        {
            float f = (1/(float) _noOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f*_mGearNum, f*(_mGearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            _mGearFactor = Mathf.Lerp(_mGearFactor, targetGearFactor, Time.deltaTime*5f);
        }


        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = _mGearNum/(float) _noOfGears;
            var revsRangeMin = ULerp(0f, mRevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(mRevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, _mGearFactor);
        }


        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                mWheelColliders[i].GetWorldPose(out position, out quat);
                mWheelMeshes[i].transform.position = position;
                mWheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1*Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            _mSteerAngle = steering*mMaximumSteerAngle;
            mWheelColliders[0].steerAngle = _mSteerAngle;
            mWheelColliders[1].steerAngle = _mSteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake*mMaxHandbrakeTorque;
                mWheelColliders[2].brakeTorque = hbTorque;
                mWheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }


        private void CapSpeed()
        {
            float speed = _mRigidbody.velocity.magnitude;
            switch (mSpeedType)
            {
                case SpeedType.Mph:

                    speed *= 2.23693629f;
                    if (speed > mTopspeed)
                        _mRigidbody.velocity = (mTopspeed/2.23693629f) * _mRigidbody.velocity.normalized;
                    break;

                case SpeedType.Kph:
                    speed *= 3.6f;
                    if (speed > mTopspeed)
                        _mRigidbody.velocity = (mTopspeed/3.6f) * _mRigidbody.velocity.normalized;
                    break;
            }
        }


        private void ApplyDrive(float accel, float footbrake)
        {

            float thrustTorque;
            switch (mCarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (_mCurrentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        mWheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (_mCurrentTorque / 2f);
                    mWheelColliders[0].motorTorque = mWheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (_mCurrentTorque / 2f);
                    mWheelColliders[2].motorTorque = mWheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _mRigidbody.velocity) < 50f)
                {
                    mWheelColliders[i].brakeTorque = mBrakeTorque*footbrake;
                }
                else if (footbrake > 0)
                {
                    mWheelColliders[i].brakeTorque = 0f;
                    mWheelColliders[i].motorTorque = -mReverseTorque*footbrake;
                }
            }
        }


        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                mWheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(_mOldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - _mOldRotation) * mSteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                _mRigidbody.velocity = velRotation * _mRigidbody.velocity;
            }
            _mOldRotation = transform.eulerAngles.y;
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            mWheelColliders[0].attachedRigidbody.AddForce(-transform.up*mDownforce*
                                                         mWheelColliders[0].attachedRigidbody.velocity.magnitude);
        }


        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                mWheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= mSlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= mSlipLimit)
                {
                    mWheelEffects[i].EmitTyreSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        mWheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (mWheelEffects[i].PlayingAudio)
                {
                    mWheelEffects[i].StopAudio();
                }
                // end the trail generation
                mWheelEffects[i].EndSkidTrail();
            }
        }

        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (mCarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        mWheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    mWheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    mWheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    mWheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    mWheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= mSlipLimit && _mCurrentTorque >= 0)
            {
                _mCurrentTorque -= 10 * mTractionControl;
            }
            else
            {
                _mCurrentTorque += 10 * mTractionControl;
                if (_mCurrentTorque > mFullTorqueOverAllWheels)
                {
                    _mCurrentTorque = mFullTorqueOverAllWheels;
                }
            }
        }


        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (mWheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
