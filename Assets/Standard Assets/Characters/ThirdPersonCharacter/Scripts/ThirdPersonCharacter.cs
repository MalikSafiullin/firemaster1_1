using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonCharacter : MonoBehaviour
    {
        private const float K_HALF = 0.5f;
        private static readonly int _forward = Animator.StringToHash("Forward");
        private static readonly int _turn = Animator.StringToHash("Turn");
        private static readonly int _crouch = Animator.StringToHash("Crouch");
        private static readonly int _onGround = Animator.StringToHash("OnGround");
        private static readonly int _jump = Animator.StringToHash("Jump");
        private static readonly int _jumpLeg = Animator.StringToHash("JumpLeg");

        [FormerlySerializedAs("m_MovingTurnSpeed")] [SerializeField]
        private float mMovingTurnSpeed = 360;

        [FormerlySerializedAs("m_StationaryTurnSpeed")] [SerializeField]
        private float mStationaryTurnSpeed = 180;

        [FormerlySerializedAs("m_JumpPower")] [SerializeField]
        private float mJumpPower = 12f;

        [FormerlySerializedAs("m_GravityMultiplier")] [Range(1f, 4f)] [SerializeField]
        private float mGravityMultiplier = 2f;

        [FormerlySerializedAs("m_RunCycleLegOffset")] [SerializeField]
        private float mRunCycleLegOffset = 0.2f;

        [FormerlySerializedAs("m_MoveSpeedMultiplier")] [SerializeField]
        private float mMoveSpeedMultiplier = 1f;

        [FormerlySerializedAs("m_AnimSpeedMultiplier")] [SerializeField]
        private float mAnimSpeedMultiplier = 1f;

        [FormerlySerializedAs("m_GroundCheckDistance")] [SerializeField]
        private float mGroundCheckDistance = 0.1f;

        private Animator _mAnimator;
        private CapsuleCollider _mCapsule;
        private Vector3 _mCapsuleCenter;
        private float _mCapsuleHeight;
        private bool _mCrouching;
        private float _mForwardAmount;
        private Vector3 _mGroundNormal;
        private bool _mIsGrounded;
        private float _mOrigGroundCheckDistance;

        private Rigidbody _mRigidbody;
        private float _mTurnAmount;


        private void Start()
        {
            _mAnimator = GetComponent<Animator>();
            _mRigidbody = GetComponent<Rigidbody>();
            _mCapsule = GetComponent<CapsuleCollider>();
            _mCapsuleHeight = _mCapsule.height;
            _mCapsuleCenter = _mCapsule.center;

            _mRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _mOrigGroundCheckDistance = mGroundCheckDistance;
        }


        public void OnAnimatorMove()
        {
            if (!_mIsGrounded || !(Time.deltaTime > 0)) return;

            var v = _mAnimator.deltaPosition * mMoveSpeedMultiplier / Time.deltaTime;

            v.y = _mRigidbody.velocity.y;
            _mRigidbody.velocity = v;
        }


        public void Move(Vector3 move, bool crouch, bool jump)
        {
            if (move.magnitude > 1f) move.Normalize();

            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();
            move = Vector3.ProjectOnPlane(move, _mGroundNormal);
            _mTurnAmount = move.z > 0 ? Mathf.Atan2(move.x, move.z) : Mathf.Atan2(move.x, Mathf.Abs(move.z));
            _mForwardAmount = move.z;
            //print($"_mForwardAmount:{move.z}");
            print($"_mTurnAmount:{_mTurnAmount}, {Mathf.Abs(move.z)}");

            ApplyExtraTurnRotation();

            // control and velocity handling is different when grounded and airborne:
            if (_mIsGrounded)
                HandleGroundedMovement(crouch, jump);
            else
                HandleAirborneMovement();

            ScaleCapsuleForCrouching(crouch);
            PreventStandingInLowHeadroom();

            UpdateAnimator(move);
        }


        private void ScaleCapsuleForCrouching(bool crouch)
        {
            if (_mIsGrounded && crouch)
            {
                if (_mCrouching) return;
                _mCapsule.height /= 2f;
                _mCapsule.center /= 2f;
                _mCrouching = true;
            }
            else
            {
                var radius = _mCapsule.radius;
                var crouchRay = new Ray(_mRigidbody.position + Vector3.up * (radius * K_HALF), Vector3.up);
                var crouchRayLength = _mCapsuleHeight - radius * K_HALF;
                if (Physics.SphereCast(crouchRay, _mCapsule.radius * K_HALF, crouchRayLength, Physics.AllLayers,
                        QueryTriggerInteraction.Ignore))
                {
                    _mCrouching = true;
                    return;
                }

                _mCapsule.height = _mCapsuleHeight;
                _mCapsule.center = _mCapsuleCenter;
                _mCrouching = false;
            }
        }

        private void PreventStandingInLowHeadroom()
        {
            // prevent standing up in crouch-only zones
            if (!_mCrouching)
            {
                var radius = _mCapsule.radius;
                var crouchRay = new Ray(_mRigidbody.position + Vector3.up * (radius * K_HALF), Vector3.up);
                var crouchRayLength = _mCapsuleHeight - radius * K_HALF;
                if (Physics.SphereCast(crouchRay, _mCapsule.radius * K_HALF, crouchRayLength, Physics.AllLayers,
                        QueryTriggerInteraction.Ignore)) _mCrouching = true;
            }
        }


        private void UpdateAnimator(Vector3 move)
        {
            // update the animator parameters
            _mAnimator.SetFloat(_forward, _mForwardAmount, 0.1f, Time.deltaTime);
            _mAnimator.SetFloat(_turn, _mTurnAmount, 0.1f, Time.deltaTime);
            _mAnimator.SetBool(_crouch, _mCrouching);
            _mAnimator.SetBool(_onGround, _mIsGrounded);
            if (!_mIsGrounded) _mAnimator.SetFloat(_jump, _mRigidbody.velocity.y);

            var normalizedTime = _mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            var runCycle = Mathf.Repeat((normalizedTime > 0 ? normalizedTime + mRunCycleLegOffset : normalizedTime - mRunCycleLegOffset),
                1);
            var jumpLeg = (runCycle < K_HALF ? 1 : -1) * _mForwardAmount;
            if (_mIsGrounded) _mAnimator.SetFloat(_jumpLeg, jumpLeg);

            if (_mIsGrounded && move.magnitude > 0)
                _mAnimator.speed = mAnimSpeedMultiplier;
            else
                // don't use that while airborne
                _mAnimator.speed = 1;
        }


        private void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:
            var extraGravityForce = Physics.gravity * mGravityMultiplier - Physics.gravity;
            _mRigidbody.AddForce(extraGravityForce);

            mGroundCheckDistance = _mRigidbody.velocity.y < 0 ? _mOrigGroundCheckDistance : 0.01f;
        }


        private void HandleGroundedMovement(bool crouch, bool jump)
        {
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && _mAnimator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
            {
                // jump!
                var velocity = _mRigidbody.velocity;
                velocity = new Vector3(velocity.x, mJumpPower, velocity.z);
                _mRigidbody.velocity = velocity;
                _mIsGrounded = false;
                _mAnimator.applyRootMotion = false;
                mGroundCheckDistance = 0.1f;
            }
        }

        private void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            var turnSpeed = Mathf.Lerp(mStationaryTurnSpeed, mMovingTurnSpeed, _mForwardAmount);
            transform.Rotate(0, _mTurnAmount * turnSpeed * Time.deltaTime, 0);
        }


        private void CheckGroundStatus()
        {
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            var position = transform.position;
            var vector3 = position + Vector3.up * 0.1f;
            Debug.DrawLine(vector3, vector3 + Vector3.down * mGroundCheckDistance);
#endif

            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hitInfo,
                    mGroundCheckDistance))
            {
                _mGroundNormal = hitInfo.normal;
                _mIsGrounded = true;
                _mAnimator.applyRootMotion = true;
            }
            else
            {
                _mIsGrounded = false;
                _mGroundNormal = Vector3.up;
                _mAnimator.applyRootMotion = false;
            }
        }
    }
}