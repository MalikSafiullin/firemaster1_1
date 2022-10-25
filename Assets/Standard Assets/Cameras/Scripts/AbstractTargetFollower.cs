using System;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable 649
namespace UnityStandardAssets.Cameras
{
    public abstract class AbstractTargetFollower : MonoBehaviour
    {
        public enum UpdateType // The available methods of updating are:
        {
            FixedUpdate, // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate, // Update in LateUpdate. (for tracking objects that are moved in Update)
            ManualUpdate, // user must call to update camera
        }

        [FormerlySerializedAs("m_Target")] [SerializeField] protected Transform mTarget;            // The target object to follow
        [FormerlySerializedAs("m_AutoTargetPlayer")] [SerializeField] private bool mAutoTargetPlayer = true;  // Whether the rig should automatically target the player.
        [FormerlySerializedAs("m_UpdateType")] [SerializeField] private UpdateType mUpdateType;         // stores the selected update type

        protected Rigidbody TargetRigidbody;


        protected virtual void Start()
        {
            // if auto targeting is used, find the object tagged "Player"
            // any class inheriting from this should call base.Start() to perform this action!
            if (mAutoTargetPlayer)
            {
                FindAndTargetPlayer();
            }
            if (mTarget == null) return;
            TargetRigidbody = mTarget.GetComponent<Rigidbody>();
        }


        private void FixedUpdate()
        {
            // we update from here if updatetype is set to Fixed, or in auto mode,
            // if the target has a rigidbody, and isn't kinematic.
            if (mAutoTargetPlayer && (mTarget == null || !mTarget.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }
            if (mUpdateType == UpdateType.FixedUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }


        private void LateUpdate()
        {
            // we update from here if updatetype is set to Late, or in auto mode,
            // if the target does not have a rigidbody, or - does have a rigidbody but is set to kinematic.
            if (mAutoTargetPlayer && (mTarget == null || !mTarget.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }
            if (mUpdateType == UpdateType.LateUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }


        public void ManualUpdate()
        {
            // we update from here if updatetype is set to Late, or in auto mode,
            // if the target does not have a rigidbody, or - does have a rigidbody but is set to kinematic.
            if (mAutoTargetPlayer && (mTarget == null || !mTarget.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }
            if (mUpdateType == UpdateType.ManualUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        protected abstract void FollowTarget(float deltaTime);


        public void FindAndTargetPlayer()
        {
            // auto target an object tagged player, if no target has been assigned
            var targetObj = GameObject.FindGameObjectWithTag("Player");
            if (targetObj)
            {
                SetTarget(targetObj.transform);
            }
        }


        public virtual void SetTarget(Transform newTransform)
        {
            mTarget = newTransform;
        }


        public Transform Target
        {
            get { return mTarget; }
        }
    }
}
