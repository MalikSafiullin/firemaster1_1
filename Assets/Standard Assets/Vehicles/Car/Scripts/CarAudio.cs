using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarAudio : MonoBehaviour
    {
        // This script reads some of the car's current properties and plays sounds accordingly.
        // The engine sound can be a simple single clip which is looped and pitched, or it
        // can be a crossfaded blend of four clips which represent the timbre of the engine
        // at different RPM and Throttle state.

        // the engine clips should all be a steady pitch, not rising or falling.

        // when using four channel engine crossfading, the four clips should be:
        // lowAccelClip : The engine at low revs, with throttle open (i.e. begining acceleration at very low speed)
        // highAccelClip : Thenengine at high revs, with throttle open (i.e. accelerating, but almost at max speed)
        // lowDecelClip : The engine at low revs, with throttle at minimum (i.e. idling or engine-braking at very low speed)
        // highDecelClip : Thenengine at high revs, with throttle at minimum (i.e. engine-braking at very high speed)

        // For proper crossfading, the clips pitches should all match, with an octave offset between low and high.


        public enum EngineAudioOptions // Options for the engine audio
        {
            Simple, // Simple style audio
            FourChannel // four Channel audio
        }

        public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;// Set the default audio options to be four channel
        public AudioClip lowAccelClip;                                              // Audio clip for low acceleration
        public AudioClip lowDecelClip;                                              // Audio clip for low deceleration
        public AudioClip highAccelClip;                                             // Audio clip for high acceleration
        public AudioClip highDecelClip;                                             // Audio clip for high deceleration
        public float pitchMultiplier = 1f;                                          // Used for altering the pitch of audio clips
        public float lowPitchMin = 1f;                                              // The lowest possible pitch for the low sounds
        public float lowPitchMax = 6f;                                              // The highest possible pitch for the low sounds
        public float highPitchMultiplier = 0.25f;                                   // Used for altering the pitch of high sounds
        public float maxRolloffDistance = 500;                                      // The maximum distance where rollof starts to take place
        public float dopplerLevel = 1;                                              // The mount of doppler effect used in the audio
        public bool useDoppler = true;                                              // Toggle for using doppler

        private AudioSource _mLowAccel; // Source for the low acceleration sounds
        private AudioSource _mLowDecel; // Source for the low deceleration sounds
        private AudioSource _mHighAccel; // Source for the high acceleration sounds
        private AudioSource _mHighDecel; // Source for the high deceleration sounds
        private bool _mStartedSound; // flag for knowing if we have started sounds
        private CarController _mCarController; // Reference to car we are controlling


        private void StartSound()
        {
            // get the carcontroller ( this will not be null as we have require component)
            _mCarController = GetComponent<CarController>();

            // setup the simple audio source
            _mHighAccel = SetUpEngineAudioSource(highAccelClip);

            // if we have four channel audio setup the four audio sources
            if (engineSoundStyle == EngineAudioOptions.FourChannel)
            {
                _mLowAccel = SetUpEngineAudioSource(lowAccelClip);
                _mLowDecel = SetUpEngineAudioSource(lowDecelClip);
                _mHighDecel = SetUpEngineAudioSource(highDecelClip);
            }

            // flag that we have started the sounds playing
            _mStartedSound = true;
        }


        private void StopSound()
        {
            //Destroy all audio sources on this object:
            foreach (var source in GetComponents<AudioSource>())
            {
                Destroy(source);
            }

            _mStartedSound = false;
        }


        // Update is called once per frame
        private void Update()
        {
            // get the distance to main camera
            float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;

            // stop sound if the object is beyond the maximum roll off distance
            if (_mStartedSound && camDist > maxRolloffDistance*maxRolloffDistance)
            {
                StopSound();
            }

            // start the sound if not playing and it is nearer than the maximum distance
            if (!_mStartedSound && camDist < maxRolloffDistance*maxRolloffDistance)
            {
                StartSound();
            }

            if (_mStartedSound)
            {
                // The pitch is interpolated between the min and max values, according to the car's revs.
                float pitch = ULerp(lowPitchMin, lowPitchMax, _mCarController.Revs);

                // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
                pitch = Mathf.Min(lowPitchMax, pitch);

                if (engineSoundStyle == EngineAudioOptions.Simple)
                {
                    // for 1 channel engine sound, it's oh so simple:
                    _mHighAccel.pitch = pitch*pitchMultiplier*highPitchMultiplier;
                    _mHighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    _mHighAccel.volume = 1;
                }
                else
                {
                    // for 4 channel engine sound, it's a little more complex:

                    // adjust the pitches based on the multipliers
                    _mLowAccel.pitch = pitch*pitchMultiplier;
                    _mLowDecel.pitch = pitch*pitchMultiplier;
                    _mHighAccel.pitch = pitch*highPitchMultiplier*pitchMultiplier;
                    _mHighDecel.pitch = pitch*highPitchMultiplier*pitchMultiplier;

                    // get values for fading the sounds based on the acceleration
                    float accFade = Mathf.Abs(_mCarController.AccelInput);
                    float decFade = 1 - accFade;

                    // get the high fade value based on the cars revs
                    float highFade = Mathf.InverseLerp(0.2f, 0.8f, _mCarController.Revs);
                    float lowFade = 1 - highFade;

                    // adjust the values to be more realistic
                    highFade = 1 - ((1 - highFade)*(1 - highFade));
                    lowFade = 1 - ((1 - lowFade)*(1 - lowFade));
                    accFade = 1 - ((1 - accFade)*(1 - accFade));
                    decFade = 1 - ((1 - decFade)*(1 - decFade));

                    // adjust the source volumes based on the fade values
                    _mLowAccel.volume = lowFade*accFade;
                    _mLowDecel.volume = lowFade*decFade;
                    _mHighAccel.volume = highFade*accFade;
                    _mHighDecel.volume = highFade*decFade;

                    // adjust the doppler levels
                    _mHighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    _mLowAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    _mHighDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    _mLowDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                }
            }
        }


        // sets up and adds new audio source to the gane object
        private AudioSource SetUpEngineAudioSource(AudioClip clip)
        {
            // create the new audio source component on the game object and set up its properties
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 0;
            source.loop = true;

            // start the clip from a random point
            source.time = Random.Range(0f, clip.length);
            source.Play();
            source.minDistance = 5;
            source.maxDistance = maxRolloffDistance;
            source.dopplerLevel = 0;
            return source;
        }


        // unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }
    }
}
