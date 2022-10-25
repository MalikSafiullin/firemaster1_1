using System;
using UnityEngine;


namespace UnityStandardAssets.Effects
{
    public class ExtinguishableParticleSystem : MonoBehaviour
    {
        public float multiplier = 1;

        private ParticleSystem[] _mSystems;


        private void Start()
        {
            _mSystems = GetComponentsInChildren<ParticleSystem>();
        }


        public void Extinguish()
        {
            foreach (var system in _mSystems)
            {
                var emission = system.emission;
                emission.enabled = false;
            }
        }
    }
}
