using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

#pragma warning disable 618, 649
namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [FormerlySerializedAs("m_IsWalking")] [SerializeField] private bool mIsWalking;
        [FormerlySerializedAs("m_WalkSpeed")] [SerializeField] private float mWalkSpeed;
        [FormerlySerializedAs("m_RunSpeed")] [SerializeField] private float mRunSpeed;
        [FormerlySerializedAs("m_RunstepLenghten")] [SerializeField] [Range(0f, 1f)] private float mRunstepLenghten;
        [FormerlySerializedAs("m_JumpSpeed")] [SerializeField] private float mJumpSpeed;
        [FormerlySerializedAs("m_StickToGroundForce")] [SerializeField] private float mStickToGroundForce;
        [FormerlySerializedAs("m_GravityMultiplier")] [SerializeField] private float mGravityMultiplier;
        [FormerlySerializedAs("m_MouseLook")] [SerializeField] private MouseLook mMouseLook;
        [FormerlySerializedAs("m_UseFovKick")] [SerializeField] private bool mUseFovKick;
        [FormerlySerializedAs("m_FovKick")] [SerializeField] private FOVKick mFovKick = new FOVKick();
        [FormerlySerializedAs("m_UseHeadBob")] [SerializeField] private bool mUseHeadBob;
        [FormerlySerializedAs("m_HeadBob")] [SerializeField] private CurveControlledBob mHeadBob = new CurveControlledBob();
        [FormerlySerializedAs("m_JumpBob")] [SerializeField] private LerpControlledBob mJumpBob = new LerpControlledBob();
        [FormerlySerializedAs("m_StepInterval")] [SerializeField] private float mStepInterval;
        [FormerlySerializedAs("m_FootstepSounds")] [SerializeField] private AudioClip[] mFootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [FormerlySerializedAs("m_JumpSound")] [SerializeField] private AudioClip mJumpSound;           // the sound played when character leaves the ground.
        [FormerlySerializedAs("m_LandSound")] [SerializeField] private AudioClip mLandSound;           // the sound played when character touches back on ground.

        private Camera _mCamera;
        private bool _mJump;
        private float _mYRotation;
        private Vector2 _mInput;
        private Vector3 _mMoveDir = Vector3.zero;
        private CharacterController _mCharacterController;
        private CollisionFlags _mCollisionFlags;
        private bool _mPreviouslyGrounded;
        private Vector3 _mOriginalCameraPosition;
        private float _mStepCycle;
        private float _mNextStep;
        private bool _mJumping;
        private AudioSource _mAudioSource;

        // Use this for initialization
        private void Start()
        {
            _mCharacterController = GetComponent<CharacterController>();
            _mCamera = Camera.main;
            _mOriginalCameraPosition = _mCamera.transform.localPosition;
            mFovKick.Setup(_mCamera);
            mHeadBob.Setup(_mCamera, mStepInterval);
            _mStepCycle = 0f;
            _mNextStep = _mStepCycle/2f;
            _mJumping = false;
            _mAudioSource = GetComponent<AudioSource>();
			mMouseLook.Init(transform , _mCamera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!_mJump)
            {
                _mJump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!_mPreviouslyGrounded && _mCharacterController.isGrounded)
            {
                StartCoroutine(mJumpBob.DoBobCycle());
                PlayLandingSound();
                _mMoveDir.y = 0f;
                _mJumping = false;
            }
            if (!_mCharacterController.isGrounded && !_mJumping && _mPreviouslyGrounded)
            {
                _mMoveDir.y = 0f;
            }

            _mPreviouslyGrounded = _mCharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            _mAudioSource.clip = mLandSound;
            _mAudioSource.Play();
            _mNextStep = _mStepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*_mInput.y + transform.right*_mInput.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, _mCharacterController.radius, Vector3.down, out hitInfo,
                               _mCharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            _mMoveDir.x = desiredMove.x*speed;
            _mMoveDir.z = desiredMove.z*speed;


            if (_mCharacterController.isGrounded)
            {
                _mMoveDir.y = -mStickToGroundForce;

                if (_mJump)
                {
                    _mMoveDir.y = mJumpSpeed;
                    PlayJumpSound();
                    _mJump = false;
                    _mJumping = true;
                }
            }
            else
            {
                _mMoveDir += Physics.gravity*mGravityMultiplier*Time.fixedDeltaTime;
            }
            _mCollisionFlags = _mCharacterController.Move(_mMoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            mMouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            _mAudioSource.clip = mJumpSound;
            _mAudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (_mCharacterController.velocity.sqrMagnitude > 0 && (_mInput.x != 0 || _mInput.y != 0))
            {
                _mStepCycle += (_mCharacterController.velocity.magnitude + (speed*(mIsWalking ? 1f : mRunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(_mStepCycle > _mNextStep))
            {
                return;
            }

            _mNextStep = _mStepCycle + mStepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!_mCharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, mFootstepSounds.Length);
            _mAudioSource.clip = mFootstepSounds[n];
            _mAudioSource.PlayOneShot(_mAudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            mFootstepSounds[n] = mFootstepSounds[0];
            mFootstepSounds[0] = _mAudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!mUseHeadBob)
            {
                return;
            }
            if (_mCharacterController.velocity.magnitude > 0 && _mCharacterController.isGrounded)
            {
                _mCamera.transform.localPosition =
                    mHeadBob.DoHeadBob(_mCharacterController.velocity.magnitude +
                                      (speed*(mIsWalking ? 1f : mRunstepLenghten)));
                newCameraPosition = _mCamera.transform.localPosition;
                newCameraPosition.y = _mCamera.transform.localPosition.y - mJumpBob.Offset();
            }
            else
            {
                newCameraPosition = _mCamera.transform.localPosition;
                newCameraPosition.y = _mOriginalCameraPosition.y - mJumpBob.Offset();
            }
            _mCamera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = mIsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            mIsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = mIsWalking ? mWalkSpeed : mRunSpeed;
            _mInput = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (_mInput.sqrMagnitude > 1)
            {
                _mInput.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (mIsWalking != waswalking && mUseFovKick && _mCharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!mIsWalking ? mFovKick.FOVKickUp() : mFovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            mMouseLook.LookRotation (transform, _mCamera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (_mCollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(_mCharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
