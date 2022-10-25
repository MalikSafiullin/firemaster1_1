using System;
using UnityEngine;


namespace UnityStandardAssets.Effects
{
    public class Hose : MonoBehaviour
    {
        public float maxPower = 20;
        public float minPower = 5;
        public float changeSpeed = 5;
        public ParticleSystem[] hoseWaterSystems;
        public Renderer systemRenderer;

        private float _mPower;


        // Update is called once per frame
        private void Update()
        {
            _mPower = Mathf.Lerp(_mPower, Input.GetMouseButton(0) ? maxPower : minPower, Time.deltaTime*changeSpeed);

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                systemRenderer.enabled = !systemRenderer.enabled;
            }

            foreach (var system in hoseWaterSystems)
            {
				ParticleSystem.MainModule mainModule = system.main;
                mainModule.startSpeed = _mPower;
                var emission = system.emission;
                emission.enabled = (_mPower > minPower*1.1f);
            }
        }
    }
}
