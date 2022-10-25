using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Vehicles.Ball
{
    public class Ball : MonoBehaviour
    {
        [FormerlySerializedAs("m_MovePower")] [SerializeField] private float mMovePower = 5; // The force added to the ball to move it.
        [FormerlySerializedAs("m_UseTorque")] [SerializeField] private bool mUseTorque = true; // Whether or not to use torque to move the ball.
        [FormerlySerializedAs("m_MaxAngularVelocity")] [SerializeField] private float mMaxAngularVelocity = 25; // The maximum velocity the ball can rotate at.
        [FormerlySerializedAs("m_JumpPower")] [SerializeField] private float mJumpPower = 2; // The force added to the ball when it jumps.

        private const float K_GROUND_RAY_LENGTH = 1f; // The length of the ray to check if the ball is grounded.
        private Rigidbody _mRigidbody;


        private void Start()
        {
            _mRigidbody = GetComponent<Rigidbody>();
            // Set the maximum angular velocity.
            GetComponent<Rigidbody>().maxAngularVelocity = mMaxAngularVelocity;
        }


        public void Move(Vector3 moveDirection, bool jump)
        {
            // If using torque to rotate the ball...
            if (mUseTorque)
            {
                // ... add torque around the axis defined by the move direction.
                _mRigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x)*mMovePower);
            }
            else
            {
                // Otherwise add force in the move direction.
                _mRigidbody.AddForce(moveDirection*mMovePower);
            }

            // If on the ground and jump is pressed...
            if (Physics.Raycast(transform.position, -Vector3.up, K_GROUND_RAY_LENGTH) && jump)
            {
                // ... add force in upwards.
                _mRigidbody.AddForce(Vector3.up*mJumpPower, ForceMode.Impulse);
            }
        }
    }
}
