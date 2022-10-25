using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class ObjectResetter : MonoBehaviour
    {
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private List<Transform> _originalStructure;

        private Rigidbody _rigidbody;

        // Use this for initialization
        private void Start()
        {
            _originalStructure = new List<Transform>(GetComponentsInChildren<Transform>());
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;

            _rigidbody = GetComponent<Rigidbody>();
        }


        public void DelayedReset(float delay)
        {
            StartCoroutine(ResetCoroutine(delay));
        }


        public IEnumerator ResetCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            // remove any gameobjects added (fire, skid trails, etc)
            foreach (var t in GetComponentsInChildren<Transform>())
            {
                if (!_originalStructure.Contains(t))
                {
                    t.parent = null;
                }
            }

            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            if (_rigidbody)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            SendMessage("Reset");
        }
    }
}
