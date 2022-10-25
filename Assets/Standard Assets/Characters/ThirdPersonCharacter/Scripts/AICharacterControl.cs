using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent Agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter Character { get; private set; } // the character we are controlling
        public Transform target;                                    // target to aim for


        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            Agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            Character = GetComponent<ThirdPersonCharacter>();

	        Agent.updateRotation = false;
	        Agent.updatePosition = true;
        }


        private void Update()
        {
            if (target != null)
                Agent.SetDestination(target.position);

            if (Agent.remainingDistance > Agent.stoppingDistance)
                Character.Move(Agent.desiredVelocity, false, false);
            else
                Character.Move(Vector3.zero, false, false);
        }


        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}
