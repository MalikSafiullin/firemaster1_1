using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets._2D
{
    [RequireComponent(typeof (PlatformerCharacter2D))]
    public class Platformer2DUserControl : MonoBehaviour
    {
        private PlatformerCharacter2D _mCharacter;
        private bool _mJump;


        private void Awake()
        {
            _mCharacter = GetComponent<PlatformerCharacter2D>();
        }


        private void Update()
        {
            if (!_mJump)
            {
                // Read the jump input in Update so button presses aren't missed.
                _mJump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
        }


        private void FixedUpdate()
        {
            // Read the inputs.
            bool crouch = Input.GetKey(KeyCode.LeftControl);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            // Pass all parameters to the character control script.
            _mCharacter.Move(h, crouch, _mJump);
            _mJump = false;
        }
    }
}
