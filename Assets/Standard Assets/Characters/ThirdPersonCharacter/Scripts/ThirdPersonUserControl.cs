using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        [SerializeField] private KeyCode crouchKey = KeyCode.C;
        [SerializeField] private float speed;

        private Transform _cam;
        private Vector3 _camForward;
        private ThirdPersonCharacter _character;
        private Vector3 _move;


        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
            {
                _cam = Camera.main.transform;
            }
            else
            {
                const string message =
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.";
                Debug.LogWarning(message, gameObject);
            }

            _character = GetComponent<ThirdPersonCharacter>();
        }


        private void Update()
        {
            var jump = Input.GetButtonDown("Jump");
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            var crouch = Input.GetKey(crouchKey);


            _camForward = _cam.forward * (speed * Time.deltaTime);
            _move = vertical * _camForward + horizontal * _cam.right;

            // print($"v:{vertical}; h:{horizontal}; m:{_move}");
            _character.Move(_move, crouch, jump);
        }
    }
}