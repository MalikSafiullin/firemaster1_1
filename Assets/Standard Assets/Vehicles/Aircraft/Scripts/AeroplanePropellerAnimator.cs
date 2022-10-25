using System;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Aeroplane
{
    public class AeroplanePropellerAnimator : MonoBehaviour
    {
        [FormerlySerializedAs("m_PropellorModel")] [SerializeField] private Transform mPropellorModel;                          // The model of the the aeroplane's propellor.
        [FormerlySerializedAs("m_PropellorBlur")] [SerializeField] private Transform mPropellorBlur;                           // The plane used for the blurred propellor textures.
        [FormerlySerializedAs("m_PropellorBlurTextures")] [SerializeField] private Texture2D[] mPropellorBlurTextures;                 // An array of increasingly blurred propellor textures.
        [FormerlySerializedAs("m_ThrottleBlurStart")] [SerializeField] [Range(0f, 1f)] private float mThrottleBlurStart = 0.25f;   // The point at which the blurred textures start.
        [FormerlySerializedAs("m_ThrottleBlurEnd")] [SerializeField] [Range(0f, 1f)] private float mThrottleBlurEnd = 0.5f;      // The point at which the blurred textures stop changing.
        [FormerlySerializedAs("m_MaxRpm")] [SerializeField] private float mMaxRpm = 2000;                               // The maximum speed the propellor can turn at.

        private AeroplaneController _mPlane;      // Reference to the aeroplane controller.
        private int _mPropellorBlurState = -1;    // To store the state of the blurred textures.
        private const float K_RPM_TO_DPS = 60f;     // For converting from revs per minute to degrees per second.
        private Renderer _mPropellorModelRenderer;
        private Renderer _mPropellorBlurRenderer;


        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            _mPlane = GetComponent<AeroplaneController>();

            _mPropellorModelRenderer = mPropellorModel.GetComponent<Renderer>();
            _mPropellorBlurRenderer = mPropellorBlur.GetComponent<Renderer>();

            // Set the propellor blur gameobject's parent to be the propellor.
            mPropellorBlur.parent = mPropellorModel;
        }


        private void Update()
        {
            // Rotate the propellor model at a rate proportional to the throttle.
            mPropellorModel.Rotate(0, mMaxRpm*_mPlane.Throttle*Time.deltaTime*K_RPM_TO_DPS, 0);

            // Create an integer for the new state of the blur textures.
            var newBlurState = 0;

            // choose between the blurred textures, if the throttle is high enough
            if (_mPlane.Throttle > mThrottleBlurStart)
            {
                var throttleBlurProportion = Mathf.InverseLerp(mThrottleBlurStart, mThrottleBlurEnd, _mPlane.Throttle);
                newBlurState = Mathf.FloorToInt(throttleBlurProportion*(mPropellorBlurTextures.Length - 1));
            }

            // If the blur state has changed
            if (newBlurState != _mPropellorBlurState)
            {
                _mPropellorBlurState = newBlurState;

                if (_mPropellorBlurState == 0)
                {
                    // switch to using the 'real' propellor model
                    _mPropellorModelRenderer.enabled = true;
                    _mPropellorBlurRenderer.enabled = false;
                }
                else
                {
                    // Otherwise turn off the propellor model and turn on the blur.
                    _mPropellorModelRenderer.enabled = false;
                    _mPropellorBlurRenderer.enabled = true;

                    // set the appropriate texture from the blur array
                    _mPropellorBlurRenderer.material.mainTexture = mPropellorBlurTextures[_mPropellorBlurState];
                }
            }
        }
    }
}
