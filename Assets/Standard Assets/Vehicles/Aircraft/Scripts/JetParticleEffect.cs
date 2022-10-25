using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof (ParticleSystem))]
    public class JetParticleEffect : MonoBehaviour
    {
        // this script controls the jet's exhaust particle system, controlling the
        // size and colour based on the jet's current throttle value.
        public Color minColour; // The base colour for the effect to start at

        private AeroplaneController _mJet; // The jet that the particle effect is attached to
        private ParticleSystem _mSystem; // The particle system that is being controlled
        private float _mOriginalStartSize; // The original starting size of the particle system
        private float _mOriginalLifetime; // The original lifetime of the particle system
        private Color _mOriginalStartColor; // The original starting colout of the particle system

        // Use this for initialization
        private void Start()
        {
            // get the aeroplane from the object hierarchy
            _mJet = FindAeroplaneParent();

            // get the particle system ( it will be on the object as we have a require component set up
            _mSystem = GetComponent<ParticleSystem>();

            // set the original properties from the particle system
            _mOriginalLifetime = _mSystem.main.startLifetime.constant;
            _mOriginalStartSize = _mSystem.main.startSize.constant;
            _mOriginalStartColor = _mSystem.main.startColor.color;
        }


        // Update is called once per frame
        private void Update()
        {
			ParticleSystem.MainModule mainModule = _mSystem.main;
			// update the particle system based on the jets throttle
			mainModule.startLifetime = Mathf.Lerp(0.0f, _mOriginalLifetime, _mJet.Throttle);
			mainModule.startSize = Mathf.Lerp(_mOriginalStartSize*.3f, _mOriginalStartSize, _mJet.Throttle);
			mainModule.startColor = Color.Lerp(minColour, _mOriginalStartColor, _mJet.Throttle);
        }


        private AeroplaneController FindAeroplaneParent()
        {
            // get reference to the object transform
            var t = transform;

            // traverse the object hierarchy upwards to find the AeroplaneController
            // (since this is placed on a child object)
            while (t != null)
            {
                var aero = t.GetComponent<AeroplaneController>();
                if (aero == null)
                {
                    // try next parent
                    t = t.parent;
                }
                else
                {
                    return aero;
                }
            }

            // controller not found!
            throw new Exception(" AeroplaneContoller not found in object hierarchy");
        }
    }
}
