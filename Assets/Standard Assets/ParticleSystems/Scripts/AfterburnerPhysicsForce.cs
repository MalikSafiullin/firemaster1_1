using System;
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    [RequireComponent(typeof (SphereCollider))]
    public class AfterburnerPhysicsForce : MonoBehaviour
    {
        public float effectAngle = 15;
        public float effectWidth = 1;
        public float effectDistance = 10;
        public float force = 10;

        private Collider[] _mCols;
        private SphereCollider _mSphere;


        private void OnEnable()
        {
            _mSphere = (GetComponent<Collider>() as SphereCollider);
        }


        private void FixedUpdate()
        {
            _mCols = Physics.OverlapSphere(transform.position + _mSphere.center, _mSphere.radius);
            for (int n = 0; n < _mCols.Length; ++n)
            {
                if (_mCols[n].attachedRigidbody != null)
                {
                    Vector3 localPos = transform.InverseTransformPoint(_mCols[n].transform.position);
                    localPos = Vector3.MoveTowards(localPos, new Vector3(0, 0, localPos.z), effectWidth*0.5f);
                    float angle = Mathf.Abs(Mathf.Atan2(localPos.x, localPos.z)*Mathf.Rad2Deg);
                    float falloff = Mathf.InverseLerp(effectDistance, 0, localPos.magnitude);
                    falloff *= Mathf.InverseLerp(effectAngle, 0, angle);
                    Vector3 delta = _mCols[n].transform.position - transform.position;
                    _mCols[n].attachedRigidbody.AddForceAtPosition(delta.normalized*force*falloff,
                                                                 Vector3.Lerp(_mCols[n].transform.position,
                                                                              transform.TransformPoint(0, 0, localPos.z),
                                                                              0.1f));
                }
            }
        }


        private void OnDrawGizmosSelected()
        {
            //check for editor time simulation to avoid null ref
            if(_mSphere == null)
                _mSphere = (GetComponent<Collider>() as SphereCollider);

            _mSphere.radius = effectDistance*.5f;
            _mSphere.center = new Vector3(0, 0, effectDistance*.5f);
            var directions = new Vector3[] {Vector3.up, -Vector3.up, Vector3.right, -Vector3.right};
            var perpDirections = new Vector3[] {-Vector3.right, Vector3.right, Vector3.up, -Vector3.up};
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            for (int n = 0; n < 4; ++n)
            {
                Vector3 origin = transform.position + transform.rotation*directions[n]*effectWidth*0.5f;

                Vector3 direction =
                    transform.TransformDirection(Quaternion.AngleAxis(effectAngle, perpDirections[n])*Vector3.forward);

                Gizmos.DrawLine(origin, origin + direction*_mSphere.radius*2);
            }
        }
    }
}
