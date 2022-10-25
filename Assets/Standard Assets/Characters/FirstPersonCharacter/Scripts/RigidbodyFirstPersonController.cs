using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            [FormerlySerializedAs("ForwardSpeed")] public float forwardSpeed = 8.0f;   // Speed when walking forward
            [FormerlySerializedAs("BackwardSpeed")] public float backwardSpeed = 4.0f;  // Speed when walking backwards
            [FormerlySerializedAs("StrafeSpeed")] public float strafeSpeed = 4.0f;    // Speed when walking sideways
            [FormerlySerializedAs("RunMultiplier")] public float runMultiplier = 2.0f;   // Speed when sprinting
	        [FormerlySerializedAs("RunKey")] public KeyCode runKey = KeyCode.LeftShift;
            [FormerlySerializedAs("JumpForce")] public float jumpForce = 30f;
            [FormerlySerializedAs("SlopeCurveModifier")] public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [FormerlySerializedAs("CurrentTargetSpeed")] [HideInInspector] public float currentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool _mRunning;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
	            if (input == Vector2.zero) return;
				if (input.x > 0 || input.x < 0)
				{
					//strafe
					currentTargetSpeed = strafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					currentTargetSpeed = backwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
					currentTargetSpeed = forwardSpeed;
				}
#if !MOBILE_INPUT
	            if (Input.GetKey(runKey))
	            {
		            currentTargetSpeed *= runMultiplier;
		            _mRunning = true;
	            }
	            else
	            {
		            _mRunning = false;
	            }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return _mRunning; }
            }
#endif
        }


        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f; // stops the character
            public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
            public bool airControl; // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }


        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();


        private Rigidbody _mRigidBody;
        private CapsuleCollider _mCapsule;
        private float _mYRotation;
        private Vector3 _mGroundContactNormal;
        private bool _mJump, _mPreviouslyGrounded, _mJumping, _mIsGrounded;


        public Vector3 Velocity
        {
            get { return _mRigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return _mIsGrounded; }
        }

        public bool Jumping
        {
            get { return _mJumping; }
        }

        public bool Running
        {
            get
            {
 #if !MOBILE_INPUT
				return movementSettings.Running;
#else
	            return false;
#endif
            }
        }


        private void Start()
        {
            _mRigidBody = GetComponent<Rigidbody>();
            _mCapsule = GetComponent<CapsuleCollider>();
            mouseLook.Init (transform, cam.transform);
        }


        private void Update()
        {
            RotateView();

            if (CrossPlatformInputManager.GetButtonDown("Jump") && !_mJump)
            {
                _mJump = true;
            }
        }


        private void FixedUpdate()
        {
            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || _mIsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward*input.y + cam.transform.right*input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, _mGroundContactNormal).normalized;

                desiredMove.x *= movementSettings.currentTargetSpeed;
                desiredMove.z *= movementSettings.currentTargetSpeed;
                desiredMove.y *= movementSettings.currentTargetSpeed;
                if (_mRigidBody.velocity.sqrMagnitude <
                    (movementSettings.currentTargetSpeed*movementSettings.currentTargetSpeed))
                {
                    _mRigidBody.AddForce(desiredMove*SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (_mIsGrounded)
            {
                _mRigidBody.drag = 5f;

                if (_mJump)
                {
                    _mRigidBody.drag = 0f;
                    _mRigidBody.velocity = new Vector3(_mRigidBody.velocity.x, 0f, _mRigidBody.velocity.z);
                    _mRigidBody.AddForce(new Vector3(0f, movementSettings.jumpForce, 0f), ForceMode.Impulse);
                    _mJumping = true;
                }

                if (!_mJumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && _mRigidBody.velocity.magnitude < 1f)
                {
                    _mRigidBody.Sleep();
                }
            }
            else
            {
                _mRigidBody.drag = 0f;
                if (_mPreviouslyGrounded && !_mJumping)
                {
                    StickToGroundHelper();
                }
            }
            _mJump = false;
        }


        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(_mGroundContactNormal, Vector3.up);
            return movementSettings.slopeCurveModifier.Evaluate(angle);
        }


        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, _mCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((_mCapsule.height/2f) - _mCapsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    _mRigidBody.velocity = Vector3.ProjectOnPlane(_mRigidBody.velocity, hitInfo.normal);
                }
            }
        }


        private Vector2 GetInput()
        {
            
            Vector2 input = new Vector2
                {
                    x = CrossPlatformInputManager.GetAxis("Horizontal"),
                    y = CrossPlatformInputManager.GetAxis("Vertical")
                };
			movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }


        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation (transform, cam.transform);

            if (_mIsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                _mRigidBody.velocity = velRotation*_mRigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            _mPreviouslyGrounded = _mIsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, _mCapsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((_mCapsule.height/2f) - _mCapsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                _mIsGrounded = true;
                _mGroundContactNormal = hitInfo.normal;
            }
            else
            {
                _mIsGrounded = false;
                _mGroundContactNormal = Vector3.up;
            }
            if (!_mPreviouslyGrounded && _mIsGrounded && _mJumping)
            {
                _mJumping = false;
            }
        }
    }
}
