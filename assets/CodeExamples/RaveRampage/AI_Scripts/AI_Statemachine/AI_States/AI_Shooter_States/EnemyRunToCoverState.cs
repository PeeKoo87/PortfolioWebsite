using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyRunToCoverState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly CoverArea coverArea;

        private Cover currentCover;

        public EnemyRunToCoverState(Enemy enemy, Animator animator, CoverArea coverArea, NavMeshAgent agent) : base(enemy, animator)
        {
            this.agent = agent;
            this.coverArea = coverArea;
        }

        public override void OnEnter()
        {
            animator.CrossFade(MoveBlend, crossFadeDuration);
            Debug.Log("Entered Run to Cover state.");

            // Attempt to find an unoccupied cover
            currentCover = coverArea.GetUnoccupiedCover();

            if (currentCover != null)
            {
                Debug.Log($"Moving to cover at position: {currentCover.transform.position}");
                agent.SetDestination(currentCover.transform.position);
                agent.stoppingDistance = enemy.enemyConfig.CoverStoppingDistance;
            }
            else
            {
                Debug.LogWarning("No unoccupied cover available! Falling back to default behavior.");
                agent.SetDestination(enemy.transform.position); // Default fallback (e.g., hold position)
            }
        }

        public override void OnExit()
        {
            Debug.Log("Exited Run to Cover state.");
            agent.stoppingDistance = enemy.enemyConfig.StoppingDistance;
            agent.speed = enemy.enemyConfig.Speed; // Reset speed to default
        }

        public override void Update()
        {
            agent.stoppingDistance = enemy.enemyConfig.CoverStoppingDistance;
            if (currentCover == null)
            {
                Debug.Log("No cover assigned. Exiting early.");
                return;
            }

            // Check if the enemy has reached the destination
            if (HasArrivedAtDestination())
            {
                //Debug.Log("Reached cover. Adjusting speed for in-cover behavior.");
                
                agent.speed = enemy.enemyConfig.SpeedInCover;

                // Optional: Trigger additional logic when reaching cover
                // For example, enemy might start attacking from cover
            }
        }

        /// <summary>
        /// Checks if the agent has arrived at its destination.
        /// </summary>
        private bool HasArrivedAtDestination()
        {
            return agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;
        }
    }
}
