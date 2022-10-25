using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.Cameras
{
    public class HandHeldCam : LookatTarget
    {
        [FormerlySerializedAs("m_SwaySpeed")] [SerializeField] private float mSwaySpeed = .5f;
        [FormerlySerializedAs("m_BaseSwayAmount")] [SerializeField] private float mBaseSwayAmount = .5f;
        [FormerlySerializedAs("m_TrackingSwayAmount")] [SerializeField] private float mTrackingSwayAmount = .5f;
        [FormerlySerializedAs("m_TrackingBias")] [Range(-1, 1)] [SerializeField] private float mTrackingBias = 0;


        protected override void FollowTarget(float deltaTime)
        {
            base.FollowTarget(deltaTime);

            float bx = (Mathf.PerlinNoise(0, Time.time*mSwaySpeed) - 0.5f);
            float by = (Mathf.PerlinNoise(0, (Time.time*mSwaySpeed) + 100)) - 0.5f;

            bx *= mBaseSwayAmount;
            by *= mBaseSwayAmount;

            float tx = (Mathf.PerlinNoise(0, Time.time*mSwaySpeed) - 0.5f) + mTrackingBias;
            float ty = ((Mathf.PerlinNoise(0, (Time.time*mSwaySpeed) + 100)) - 0.5f) + mTrackingBias;

            tx *= -mTrackingSwayAmount*MFollowVelocity.x;
            ty *= mTrackingSwayAmount*MFollowVelocity.y;

            transform.Rotate(bx + tx, by + ty, 0);
        }
    }
}
