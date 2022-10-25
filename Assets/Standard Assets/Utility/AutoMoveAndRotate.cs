using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class AutoMoveAndRotate : MonoBehaviour
    {
        public Vector3AndSpace moveUnitsPerSecond;
        public Vector3AndSpace rotateDegreesPerSecond;
        public bool ignoreTimescale;
        private float _mLastRealTime;


        private void Start()
        {
            _mLastRealTime = Time.realtimeSinceStartup;
        }


        // Update is called once per frame
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (ignoreTimescale)
            {
                deltaTime = (Time.realtimeSinceStartup - _mLastRealTime);
                _mLastRealTime = Time.realtimeSinceStartup;
            }
            transform.Translate(moveUnitsPerSecond.value*deltaTime, moveUnitsPerSecond.space);
            transform.Rotate(rotateDegreesPerSecond.value*deltaTime, moveUnitsPerSecond.space);
        }


        [Serializable]
        public class Vector3AndSpace
        {
            public Vector3 value;
            public Space space = Space.Self;
        }
    }
}
