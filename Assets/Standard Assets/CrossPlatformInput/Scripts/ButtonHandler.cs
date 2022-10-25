using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {

        [FormerlySerializedAs("Name")] public string name;

        void OnEnable()
        {

        }

        public void SetDownState()
        {
            CrossPlatformInputManager.SetButtonDown(name);
        }


        public void SetUpState()
        {
            CrossPlatformInputManager.SetButtonUp(name);
        }


        public void SetAxisPositiveState()
        {
            CrossPlatformInputManager.SetAxisPositive(name);
        }


        public void SetAxisNeutralState()
        {
            CrossPlatformInputManager.SetAxisZero(name);
        }


        public void SetAxisNegativeState()
        {
            CrossPlatformInputManager.SetAxisNegative(name);
        }

        public void Update()
        {

        }
    }
}
