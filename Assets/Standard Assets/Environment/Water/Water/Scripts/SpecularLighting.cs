using System;
using UnityEngine;

namespace UnityStandardAssets.Water
{
    [RequireComponent(typeof(WaterBase))]
    [ExecuteInEditMode]
    public class SpecularLighting : MonoBehaviour
    {
        public Transform specularLight;
        private WaterBase _mWaterBase;


        public void Start()
        {
            _mWaterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
        }


        public void Update()
        {
            if (!_mWaterBase)
            {
                _mWaterBase = (WaterBase)gameObject.GetComponent(typeof(WaterBase));
            }

            if (specularLight && _mWaterBase.sharedMaterial)
            {
                _mWaterBase.sharedMaterial.SetVector("_WorldLightDir", specularLight.transform.forward);
            }
        }
    }
}