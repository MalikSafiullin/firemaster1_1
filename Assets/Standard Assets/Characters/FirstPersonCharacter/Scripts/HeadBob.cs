using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.Utility;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class HeadBob : MonoBehaviour
    {
        [FormerlySerializedAs("Camera")] public Camera camera;
        public CurveControlledBob motionBob = new CurveControlledBob();
        public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
        public RigidbodyFirstPersonController rigidbodyFirstPersonController;
        [FormerlySerializedAs("StrideInterval")] public float strideInterval;
        [FormerlySerializedAs("RunningStrideLengthen")] [Range(0f, 1f)] public float runningStrideLengthen;

       // private CameraRefocus m_CameraRefocus;
        private bool _mPreviouslyGrounded;
        private Vector3 _mOriginalCameraPosition;


        private void Start()
        {
            motionBob.Setup(camera, strideInterval);
            _mOriginalCameraPosition = camera.transform.localPosition;
       //     m_CameraRefocus = new CameraRefocus(Camera, transform.root.transform, Camera.transform.localPosition);
        }


        private void Update()
        {
          //  m_CameraRefocus.GetFocusPoint();
            Vector3 newCameraPosition;
            if (rigidbodyFirstPersonController.Velocity.magnitude > 0 && rigidbodyFirstPersonController.Grounded)
            {
                camera.transform.localPosition = motionBob.DoHeadBob(rigidbodyFirstPersonController.Velocity.magnitude*(rigidbodyFirstPersonController.Running ? runningStrideLengthen : 1f));
                newCameraPosition = camera.transform.localPosition;
                newCameraPosition.y = camera.transform.localPosition.y - jumpAndLandingBob.Offset();
            }
            else
            {
                newCameraPosition = camera.transform.localPosition;
                newCameraPosition.y = _mOriginalCameraPosition.y - jumpAndLandingBob.Offset();
            }
            camera.transform.localPosition = newCameraPosition;

            if (!_mPreviouslyGrounded && rigidbodyFirstPersonController.Grounded)
            {
                StartCoroutine(jumpAndLandingBob.DoBobCycle());
            }

            _mPreviouslyGrounded = rigidbodyFirstPersonController.Grounded;
          //  m_CameraRefocus.SetFocusPoint();
        }
    }
}
