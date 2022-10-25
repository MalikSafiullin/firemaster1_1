using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    public class BrakeLight : MonoBehaviour
    {
        public CarController car; // reference to the car controller, must be dragged in inspector

        private Renderer _mRenderer;


        private void Start()
        {
            _mRenderer = GetComponent<Renderer>();
        }


        private void Update()
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            _mRenderer.enabled = car.BrakeInput > 0f;
        }
    }
}
