using System;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [FormerlySerializedAs("m_MaxSpeed")] [SerializeField] private float mMaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [FormerlySerializedAs("m_JumpForce")] [SerializeField] private float mJumpForce = 400f;                  // Amount of force added when the player jumps.
        [FormerlySerializedAs("m_CrouchSpeed")] [Range(0, 1)] [SerializeField] private float mCrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [FormerlySerializedAs("m_AirControl")] [SerializeField] private bool mAirControl = false;                 // Whether or not a player can steer while jumping;
        [FormerlySerializedAs("m_WhatIsGround")] [SerializeField] private LayerMask mWhatIsGround;                  // A mask determining what is ground to the character

        private Transform _mGroundCheck;    // A position marking where to check if the player is grounded.
        const float K_GROUNDED_RADIUS = .2f; // Radius of the overlap circle to determine if grounded
        private bool _mGrounded;            // Whether or not the player is grounded.
        private Transform _mCeilingCheck;   // A position marking where to check for ceilings
        const float K_CEILING_RADIUS = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator _mAnim;            // Reference to the player's animator component.
        private Rigidbody2D _mRigidbody2D;
        private bool _mFacingRight = true;  // For determining which way the player is currently facing.

        private void Awake()
        {
            // Setting up references.
            _mGroundCheck = transform.Find("GroundCheck");
            _mCeilingCheck = transform.Find("CeilingCheck");
            _mAnim = GetComponent<Animator>();
            _mRigidbody2D = GetComponent<Rigidbody2D>();
        }


        private void FixedUpdate()
        {
            _mGrounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(_mGroundCheck.position, K_GROUNDED_RADIUS, mWhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                    _mGrounded = true;
            }
            _mAnim.SetBool("Ground", _mGrounded);

            // Set the vertical animation
            _mAnim.SetFloat("vSpeed", _mRigidbody2D.velocity.y);
        }


        public void Move(float move, bool crouch, bool jump)
        {
            // If crouching, check to see if the character can stand up
            if (!crouch && _mAnim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(_mCeilingCheck.position, K_CEILING_RADIUS, mWhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            _mAnim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (_mGrounded || mAirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move*mCrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                _mAnim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                _mRigidbody2D.velocity = new Vector2(move*mMaxSpeed, _mRigidbody2D.velocity.y);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !_mFacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && _mFacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (_mGrounded && jump && _mAnim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                _mGrounded = false;
                _mAnim.SetBool("Ground", false);
                _mRigidbody2D.AddForce(new Vector2(0f, mJumpForce));
            }
        }


        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            _mFacingRight = !_mFacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}
