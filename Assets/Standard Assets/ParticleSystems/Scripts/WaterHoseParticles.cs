using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityStandardAssets.Effects
{
    public class WaterHoseParticles : MonoBehaviour
    {
        public static float LastSoundTime;
        public float force = 1;


        private List<ParticleCollisionEvent> _mCollisionEvents = new List<ParticleCollisionEvent>();
        private ParticleSystem _mParticleSystem;


        private void Start()
        {
            _mParticleSystem = GetComponent<ParticleSystem>();
        }


        private void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = _mParticleSystem.GetCollisionEvents(other, _mCollisionEvents);
            int i = 0;

            while (i < numCollisionEvents)
            {
                if (Time.time > LastSoundTime + 0.2f)
                {
                    LastSoundTime = Time.time;
                }

                var col = _mCollisionEvents[i].colliderComponent;
                var attachedRigidbody = col.GetComponent<Rigidbody>();
                if (attachedRigidbody != null)
                {
                    Vector3 vel = _mCollisionEvents[i].velocity;
                    attachedRigidbody.AddForce(vel*force, ForceMode.Impulse);
                }

                other.BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);

                i++;
            }
        }
    }
}
