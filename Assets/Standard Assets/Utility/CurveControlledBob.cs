using System;
using UnityEngine;
using UnityEngine.Serialization;


namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class CurveControlledBob
    {
        [FormerlySerializedAs("HorizontalBobRange")] public float horizontalBobRange = 0.33f;
        [FormerlySerializedAs("VerticalBobRange")] public float verticalBobRange = 0.33f;
        [FormerlySerializedAs("Bobcurve")] public AnimationCurve bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob
        [FormerlySerializedAs("VerticaltoHorizontalRatio")] public float verticaltoHorizontalRatio = 1f;

        private float _mCyclePositionX;
        private float _mCyclePositionY;
        private float _mBobBaseInterval;
        private Vector3 _mOriginalCameraPosition;
        private float _mTime;


        public void Setup(Camera camera, float bobBaseInterval)
        {
            _mBobBaseInterval = bobBaseInterval;
            _mOriginalCameraPosition = camera.transform.localPosition;

            // get the length of the curve in time
            _mTime = bobcurve[bobcurve.length - 1].time;
        }


        public Vector3 DoHeadBob(float speed)
        {
            float xPos = _mOriginalCameraPosition.x + (bobcurve.Evaluate(_mCyclePositionX)*horizontalBobRange);
            float yPos = _mOriginalCameraPosition.y + (bobcurve.Evaluate(_mCyclePositionY)*verticalBobRange);

            _mCyclePositionX += (speed*Time.deltaTime)/_mBobBaseInterval;
            _mCyclePositionY += ((speed*Time.deltaTime)/_mBobBaseInterval)*verticaltoHorizontalRatio;

            if (_mCyclePositionX > _mTime)
            {
                _mCyclePositionX -= _mTime;
            }
            if (_mCyclePositionY > _mTime)
            {
                _mCyclePositionY -= _mTime;
            }

            return new Vector3(xPos, yPos, 0f);
        }
    }
}
