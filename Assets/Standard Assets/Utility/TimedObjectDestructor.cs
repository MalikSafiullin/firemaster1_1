using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Utility
{
    public class TimedObjectDestructor : MonoBehaviour
    {
        [FormerlySerializedAs("m_TimeOut")] [SerializeField] private float mTimeOut = 1.0f;
        [FormerlySerializedAs("m_DetachChildren")] [SerializeField] private bool mDetachChildren = false;


        private void Awake()
        {
            Invoke("DestroyNow", mTimeOut);
        }


        private void DestroyNow()
        {
            if (mDetachChildren)
            {
                transform.DetachChildren();
            }
            Destroy(gameObject);
        }
    }
}
