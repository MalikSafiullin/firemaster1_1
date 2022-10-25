using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    // this script is specific to the supplied Sample Assets car, which has mudguards over the front wheels
    // which have to turn with the wheels when steering is applied.

    public class Mudguard : MonoBehaviour
    {
        public CarController carController; // car controller to get the steering angle

        private Quaternion _mOriginalRotation;


        private void Start()
        {
            _mOriginalRotation = transform.localRotation;
        }


        private void Update()
        {
            transform.localRotation = _mOriginalRotation*Quaternion.Euler(0, carController.CurrentSteerAngle, 0);
        }
    }
}
