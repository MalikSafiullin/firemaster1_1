using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class FOVKick
    {
        [FormerlySerializedAs("Camera")] public Camera camera;                           // optional camera setup, if null the main camera will be used
        [HideInInspector] public float originalFov;     // the original fov
        [FormerlySerializedAs("FOVIncrease")] public float fovIncrease = 3f;                  // the amount the field of view increases when going into a run
        [FormerlySerializedAs("TimeToIncrease")] public float timeToIncrease = 1f;               // the amount of time the field of view will increase over
        [FormerlySerializedAs("TimeToDecrease")] public float timeToDecrease = 1f;               // the amount of time the field of view will take to return to its original size
        [FormerlySerializedAs("IncreaseCurve")] public AnimationCurve increaseCurve;


        public void Setup(Camera camera)
        {
            CheckStatus(camera);

            this.camera = camera;
            originalFov = camera.fieldOfView;
        }


        private void CheckStatus(Camera camera)
        {
            if (camera == null)
            {
                throw new Exception("FOVKick camera is null, please supply the camera to the constructor");
            }

            if (increaseCurve == null)
            {
                throw new Exception(
                    "FOVKick Increase curve is null, please define the curve for the field of view kicks");
            }
        }


        public void ChangeCamera(Camera camera)
        {
            this.camera = camera;
        }


        public IEnumerator FOVKickUp()
        {
            float t = Mathf.Abs((camera.fieldOfView - originalFov)/fovIncrease);
            while (t < timeToIncrease)
            {
                camera.fieldOfView = originalFov + (increaseCurve.Evaluate(t/timeToIncrease)*fovIncrease);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }


        public IEnumerator FOVKickDown()
        {
            float t = Mathf.Abs((camera.fieldOfView - originalFov)/fovIncrease);
            while (t > 0)
            {
                camera.fieldOfView = originalFov + (increaseCurve.Evaluate(t/timeToDecrease)*fovIncrease);
                t -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            //make sure that fov returns to the original size
            camera.fieldOfView = originalFov;
        }
    }
}
