using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class DynamicShadowSettings : MonoBehaviour
    {
        public Light sunLight;
        public float minHeight = 10;
        public float minShadowDistance = 80;
        public float minShadowBias = 1;
        public float maxHeight = 1000;
        public float maxShadowDistance = 10000;
        public float maxShadowBias = 0.1f;
        public float adaptTime = 1;

        private float _mSmoothHeight;
        private float _mChangeSpeed;
        private float _mOriginalStrength = 1;


        private void Start()
        {
            _mOriginalStrength = sunLight.shadowStrength;
        }


        // Update is called once per frame
        private void Update()
        {
            Ray ray = new Ray(Camera.main.transform.position, -Vector3.up);
            RaycastHit hit;
            float height = transform.position.y;
            if (Physics.Raycast(ray, out hit))
            {
                height = hit.distance;
            }

            if (Mathf.Abs(height - _mSmoothHeight) > 1)
            {
                _mSmoothHeight = Mathf.SmoothDamp(_mSmoothHeight, height, ref _mChangeSpeed, adaptTime);
            }

            float i = Mathf.InverseLerp(minHeight, maxHeight, _mSmoothHeight);

            QualitySettings.shadowDistance = Mathf.Lerp(minShadowDistance, maxShadowDistance, i);
            sunLight.shadowBias = Mathf.Lerp(minShadowBias, maxShadowBias, 1 - ((1 - i)*(1 - i)));
            sunLight.shadowStrength = Mathf.Lerp(_mOriginalStrength, 0, i);
        }
    }
}
