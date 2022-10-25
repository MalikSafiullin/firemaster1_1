using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Effects
{
    public class FireLight : MonoBehaviour
    {
        private float _mRnd;
        private bool _mBurning = true;
        private Light _mLight;


        private void Start()
        {
            _mRnd = Random.value*100;
            _mLight = GetComponent<Light>();
        }


        private void Update()
        {
            if (_mBurning)
            {
                _mLight.intensity = 2*Mathf.PerlinNoise(_mRnd + Time.time, _mRnd + 1 + Time.time*1);
                float x = Mathf.PerlinNoise(_mRnd + 0 + Time.time*2, _mRnd + 1 + Time.time*2) - 0.5f;
                float y = Mathf.PerlinNoise(_mRnd + 2 + Time.time*2, _mRnd + 3 + Time.time*2) - 0.5f;
                float z = Mathf.PerlinNoise(_mRnd + 4 + Time.time*2, _mRnd + 5 + Time.time*2) - 0.5f;
                transform.localPosition = Vector3.up + new Vector3(x, y, z)*1;
            }
        }


        public void Extinguish()
        {
            _mBurning = false;
            _mLight.enabled = false;
        }
    }
}
