using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof (Text))]
    public class FPSCounter : MonoBehaviour
    {
        const float FPS_MEASURE_PERIOD = 0.5f;
        private int _mFpsAccumulator = 0;
        private float _mFpsNextPeriod = 0;
        private int _mCurrentFps;
        const string DISPLAY = "{0} FPS";
        private Text _mText;


        private void Start()
        {
            _mFpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
            _mText = GetComponent<Text>();
        }


        private void Update()
        {
            // measure average frames per second
            _mFpsAccumulator++;
            if (Time.realtimeSinceStartup > _mFpsNextPeriod)
            {
                _mCurrentFps = (int) (_mFpsAccumulator/FPS_MEASURE_PERIOD);
                _mFpsAccumulator = 0;
                _mFpsNextPeriod += FPS_MEASURE_PERIOD;
                _mText.text = string.Format(DISPLAY, _mCurrentFps);
            }
        }
    }
}
