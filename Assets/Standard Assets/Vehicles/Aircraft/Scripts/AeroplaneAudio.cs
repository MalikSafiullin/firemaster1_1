using System;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Aeroplane
{
    public class AeroplaneAudio : MonoBehaviour
    {

        [Serializable]
        public class AdvancedSetttings // A class for storing the advanced options.
        {
            public float engineMinDistance = 50f;                   // The min distance of the engine audio source.
            public float engineMaxDistance = 1000f;                 // The max distance of the engine audio source.
            public float engineDopplerLevel = 1f;                   // The doppler level of the engine audio source.
            [Range(0f, 1f)] public float engineMasterVolume = 0.5f; // An overall control of the engine sound volume.
            public float windMinDistance = 10f;                     // The min distance of the wind audio source.
            public float windMaxDistance = 100f;                    // The max distance of the wind audio source.
            public float windDopplerLevel = 1f;                     // The doppler level of the wind audio source.
            [Range(0f, 1f)] public float windMasterVolume = 0.5f;   // An overall control of the wind sound volume.
        }

        [FormerlySerializedAs("m_EngineSound")] [SerializeField] private AudioClip mEngineSound;                     // Looped engine sound, whose pitch and volume are affected by the plane's throttle setting.
        [FormerlySerializedAs("m_EngineMinThrottlePitch")] [SerializeField] private float mEngineMinThrottlePitch = 0.4f;       // Pitch of the engine sound when at minimum throttle.
        [FormerlySerializedAs("m_EngineMaxThrottlePitch")] [SerializeField] private float mEngineMaxThrottlePitch = 2f;         // Pitch of the engine sound when at maximum throttle.
        [FormerlySerializedAs("m_EngineFwdSpeedMultiplier")] [SerializeField] private float mEngineFwdSpeedMultiplier = 0.002f;   // Additional multiplier for an increase in pitch of the engine from the plane's speed.
        [FormerlySerializedAs("m_WindSound")] [SerializeField] private AudioClip mWindSound;                       // Looped wind sound, whose pitch and volume are affected by the plane's velocity.
        [FormerlySerializedAs("m_WindBasePitch")] [SerializeField] private float mWindBasePitch = 0.2f;                // starting pitch for wind (when plane is at zero speed)
        [FormerlySerializedAs("m_WindSpeedPitchFactor")] [SerializeField] private float mWindSpeedPitchFactor = 0.004f;       // Relative increase in pitch of the wind from the plane's speed.
        [FormerlySerializedAs("m_WindMaxSpeedVolume")] [SerializeField] private float mWindMaxSpeedVolume = 100;            // the speed the aircraft much reach before the wind sound reaches maximum volume.
        [FormerlySerializedAs("m_AdvancedSetttings")] [SerializeField] private AdvancedSetttings mAdvancedSetttings = new AdvancedSetttings();// container to make advanced settings appear as rollout in inspector

        private AudioSource _mEngineSoundSource;  // Reference to the AudioSource for the engine.
        private AudioSource _mWindSoundSource;    // Reference to the AudioSource for the wind.
        private AeroplaneController _mPlane;      // Reference to the aeroplane controller.
        private Rigidbody _mRigidbody;


        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            _mPlane = GetComponent<AeroplaneController>();
            _mRigidbody = GetComponent<Rigidbody>();


            // Add the audiosources and get the references.
            _mEngineSoundSource = gameObject.AddComponent<AudioSource>();
            _mEngineSoundSource.playOnAwake = false;
            _mWindSoundSource = gameObject.AddComponent<AudioSource>();
            _mWindSoundSource.playOnAwake = false;

            // Assign clips to the audiosources.
            _mEngineSoundSource.clip = mEngineSound;
            _mWindSoundSource.clip = mWindSound;

            // Set the parameters of the audiosources.
            _mEngineSoundSource.minDistance = mAdvancedSetttings.engineMinDistance;
            _mEngineSoundSource.maxDistance = mAdvancedSetttings.engineMaxDistance;
            _mEngineSoundSource.loop = true;
            _mEngineSoundSource.dopplerLevel = mAdvancedSetttings.engineDopplerLevel;

            _mWindSoundSource.minDistance = mAdvancedSetttings.windMinDistance;
            _mWindSoundSource.maxDistance = mAdvancedSetttings.windMaxDistance;
            _mWindSoundSource.loop = true;
            _mWindSoundSource.dopplerLevel = mAdvancedSetttings.windDopplerLevel;

            // call update here to set the sounds pitch and volumes before they actually play
            Update();

            // Start the sounds playing.
            _mEngineSoundSource.Play();
            _mWindSoundSource.Play();
        }


        private void Update()
        {
            // Find what proportion of the engine's power is being used.
            var enginePowerProportion = Mathf.InverseLerp(0, _mPlane.MaxEnginePower, _mPlane.EnginePower);

            // Set the engine's pitch to be proportional to the engine's current power.
            _mEngineSoundSource.pitch = Mathf.Lerp(mEngineMinThrottlePitch, mEngineMaxThrottlePitch, enginePowerProportion);

            // Increase the engine's pitch by an amount proportional to the aeroplane's forward speed.
            // (this makes the pitch increase when going into a dive!)
            _mEngineSoundSource.pitch += _mPlane.ForwardSpeed*mEngineFwdSpeedMultiplier;

            // Set the engine's volume to be proportional to the engine's current power.
            _mEngineSoundSource.volume = Mathf.InverseLerp(0, _mPlane.MaxEnginePower*mAdvancedSetttings.engineMasterVolume,
                                                         _mPlane.EnginePower);

            // Set the wind's pitch and volume to be proportional to the aeroplane's forward speed.
            float planeSpeed = _mRigidbody.velocity.magnitude;
            _mWindSoundSource.pitch = mWindBasePitch + planeSpeed*mWindSpeedPitchFactor;
            _mWindSoundSource.volume = Mathf.InverseLerp(0, mWindMaxSpeedVolume, planeSpeed)*mAdvancedSetttings.windMasterVolume;
        }
    }
}
