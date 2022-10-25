using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class DragRigidbody : MonoBehaviour
    {
        const float K_SPRING = 50.0f;
        const float K_DAMPER = 5.0f;
        const float K_DRAG = 10.0f;
        const float K_ANGULAR_DRAG = 5.0f;
        const float K_DISTANCE = 0.2f;
        const bool K_ATTACH_TO_CENTER_OF_MASS = false;

        private SpringJoint _mSpringJoint;


        private void Update()
        {
            // Make sure the user pressed the mouse down
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            var mainCamera = FindCamera();

            // We need to actually hit an object
            RaycastHit hit = new RaycastHit();
            if (
                !Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition).origin,
                                 mainCamera.ScreenPointToRay(Input.mousePosition).direction, out hit, 100,
                                 Physics.DefaultRaycastLayers))
            {
                return;
            }
            // We need to hit a rigidbody that is not kinematic
            if (!hit.rigidbody || hit.rigidbody.isKinematic)
            {
                return;
            }

            if (!_mSpringJoint)
            {
                var go = new GameObject("Rigidbody dragger");
                Rigidbody body = go.AddComponent<Rigidbody>();
                _mSpringJoint = go.AddComponent<SpringJoint>();
                body.isKinematic = true;
            }

            _mSpringJoint.transform.position = hit.point;
            _mSpringJoint.anchor = Vector3.zero;

            _mSpringJoint.spring = K_SPRING;
            _mSpringJoint.damper = K_DAMPER;
            _mSpringJoint.maxDistance = K_DISTANCE;
            _mSpringJoint.connectedBody = hit.rigidbody;

            StartCoroutine("DragObject", hit.distance);
        }


        private IEnumerator DragObject(float distance)
        {
            var oldDrag = _mSpringJoint.connectedBody.drag;
            var oldAngularDrag = _mSpringJoint.connectedBody.angularDrag;
            _mSpringJoint.connectedBody.drag = K_DRAG;
            _mSpringJoint.connectedBody.angularDrag = K_ANGULAR_DRAG;
            var mainCamera = FindCamera();
            while (Input.GetMouseButton(0))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                _mSpringJoint.transform.position = ray.GetPoint(distance);
                yield return null;
            }
            if (_mSpringJoint.connectedBody)
            {
                _mSpringJoint.connectedBody.drag = oldDrag;
                _mSpringJoint.connectedBody.angularDrag = oldAngularDrag;
                _mSpringJoint.connectedBody = null;
            }
        }


        private Camera FindCamera()
        {
            if (GetComponent<Camera>())
            {
                return GetComponent<Camera>();
            }

            return Camera.main;
        }
    }
}
