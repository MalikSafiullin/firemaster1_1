using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        [FormerlySerializedAs("SkidTrailPrefab")] public Transform skidTrailPrefab;
        public static Transform SkidTrailsDetachedParent;
        public ParticleSystem skidParticles;
        public bool Skidding { get; private set; }
        public bool PlayingAudio { get; private set; }


        private AudioSource _mAudioSource;
        private Transform _mSkidTrail;
        private WheelCollider _mWheelCollider;


        private void Start()
        {
            skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            if (skidParticles == null)
            {
                Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                skidParticles.Stop();
            }

            _mWheelCollider = GetComponent<WheelCollider>();
            _mAudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;

            if (SkidTrailsDetachedParent == null)
            {
                SkidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }


        public void EmitTyreSmoke()
        {
            skidParticles.transform.position = transform.position - transform.up*_mWheelCollider.radius;
            skidParticles.Emit(1);
            if (!Skidding)
            {
                StartCoroutine(StartSkidTrail());
            }
        }


        public void PlayAudio()
        {
            _mAudioSource.Play();
            PlayingAudio = true;
        }


        public void StopAudio()
        {
            _mAudioSource.Stop();
            PlayingAudio = false;
        }


        public IEnumerator StartSkidTrail()
        {
            Skidding = true;
            _mSkidTrail = Instantiate(skidTrailPrefab);
            while (_mSkidTrail == null)
            {
                yield return null;
            }
            _mSkidTrail.parent = transform;
            _mSkidTrail.localPosition = -Vector3.up*_mWheelCollider.radius;
        }


        public void EndSkidTrail()
        {
            if (!Skidding)
            {
                return;
            }
            Skidding = false;
            _mSkidTrail.parent = SkidTrailsDetachedParent;
            Destroy(_mSkidTrail.gameObject, 10);
        }
    }
}
