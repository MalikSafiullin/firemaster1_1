using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class LerpControlledBob
    {
        [FormerlySerializedAs("BobDuration")] public float bobDuration;
        [FormerlySerializedAs("BobAmount")] public float bobAmount;

        private float _mOffset = 0f;


        // provides the offset that can be used
        public float Offset()
        {
            return _mOffset;
        }


        public IEnumerator DoBobCycle()
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < bobDuration)
            {
                _mOffset = Mathf.Lerp(0f, bobAmount, t/bobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // make it move back to neutral
            t = 0f;
            while (t < bobDuration)
            {
                _mOffset = Mathf.Lerp(bobAmount, 0f, t/bobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            _mOffset = 0f;
        }
    }
}
