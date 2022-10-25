using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets.Vehicles.Car
{
    public class SkidTrail : MonoBehaviour
    {
        [FormerlySerializedAs("m_PersistTime")] [SerializeField] private float mPersistTime;


        private IEnumerator Start()
        {
			while (true)
            {
                yield return null;

                if (transform.parent.parent == null)
                {
					Destroy(gameObject, mPersistTime);
                }
            }
        }
    }
}
