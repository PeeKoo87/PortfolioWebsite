using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyWanderState : EnemyBaseState
    {
        readonly NavMeshAgent agent;
        readonly Vector3 startPoint;
        readonly float wanderRadius;

        public EnemyWanderState(Enemy enemy, Animator animator,NavMeshAgent agent ,float wanderRadius) : base(enemy, animator)
        {
            this.agent = agent;
            this.startPoint = enemy.transform.position;
            this.wanderRadius = wanderRadius;

        }
        
        public override void OnEnter()
        {
            animator.CrossFade(MoveBlend, crossFadeDuration);
            enemy.WanderStateActive = true;
        }

        public override void OnExit()
        {
            enemy.WanderStateActive = false;
        }

        public override void Update()
        {
            if (HasReachedDestination()) 
            {
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
                var finalPosition = hit.position;
                
                agent.SetDestination(finalPosition);

            }
        }

        bool HasReachedDestination()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}
