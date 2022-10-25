using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class CameraRefocus
    {
        public Camera Camera;
        public Vector3 Lookatpoint;
        public Transform Parent;

        private Vector3 _mOrigCameraPos;
        private bool _mRefocus;


        public CameraRefocus(Camera camera, Transform parent, Vector3 origCameraPos)
        {
            _mOrigCameraPos = origCameraPos;
            Camera = camera;
            Parent = parent;
        }


        public void ChangeCamera(Camera camera)
        {
            Camera = camera;
        }


        public void ChangeParent(Transform parent)
        {
            Parent = parent;
        }


        public void GetFocusPoint()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Parent.transform.position + _mOrigCameraPos, Parent.transform.forward, out hitInfo,
                                100f))
            {
                Lookatpoint = hitInfo.point;
                _mRefocus = true;
                return;
            }
            _mRefocus = false;
        }


        public void SetFocusPoint()
        {
            if (_mRefocus)
            {
                Camera.transform.LookAt(Lookatpoint);
            }
        }
    }
}
