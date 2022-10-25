using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Vehicles.Car
{
    public class CarSelfRighting : MonoBehaviour
    {
        // Automatically put the car the right way up, if it has come to rest upside-down.
        [FormerlySerializedAs("m_WaitTime")] [SerializeField] private float mWaitTime = 3f;           // time to wait before self righting
        [FormerlySerializedAs("m_VelocityThreshold")] [SerializeField] private float mVelocityThreshold = 1f;  // the velocity below which the car is considered stationary for self-righting

        private float _mLastOkTime; // the last time that the car was in an OK state
        private Rigidbody _mRigidbody;


        private void Start()
        {
            _mRigidbody = GetComponent<Rigidbody>();
        }


        private void Update()
        {
            // is the car is the right way up
            if (transform.up.y > 0f || _mRigidbody.velocity.magnitude > mVelocityThreshold)
            {
                _mLastOkTime = Time.time;
            }

            if (Time.time > _mLastOkTime + mWaitTime)
            {
                RightCar();
            }
        }


        // put the car back the right way up:
        private void RightCar()
        {
            // set the correct orientation for the car, and lift it off the ground a little
            transform.position += Vector3.up;
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
    }
}
