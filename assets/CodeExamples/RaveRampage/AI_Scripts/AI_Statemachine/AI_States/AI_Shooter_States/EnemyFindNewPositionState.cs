using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyFindNewPositionState : EnemyBaseState
    {
        readonly NavMeshAgent agent;

        private NavMeshPath path;

        private readonly AI_Enemy_Positioning positioning;
        private float destinationThreshold = 0.1f; // Distance threshold to consider as "arrived"

        public EnemyFindNewPositionState(Enemy enemy, Animator animator, NavMeshAgent agent, AI_Enemy_Positioning positioning)
            : base(enemy, animator)
        {
            this.agent = agent;
            this.positioning = positioning;
        }

        public override void OnEnter()
        {
            
            enemy.FindNewPositionStateActive = true;
            //Debug.Log($"{enemy} Entering Find New Position State.");
            
            //SetNewDestination();
            //positioning.anchorPos.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (!positioning.DetectEnemies()) { return; }
            if (positioning.DetectEnemies() == true) { /*Debug.Log("move to new location");*/ }

            if (!agent.pathPending && agent.remainingDistance <= destinationThreshold)
            {


                //Debug.Log($"{enemy} Reached destination. Exiting Find New Position State.");
                OnExit();
                // Trigger transition to another state
                // Example: Return to wandering or idle

            }
            SetNewDestination();
            isRelocating();
        }

        private void SetNewDestination()
        {
            
            Vector3 newTarget = positioning.MoveTarget();
            //Debug.Log($"MoveTarget returned: {newTarget}");

            // Check if agent is on the NavMesh
            if (!agent.isOnNavMesh)
            {
                //Debug.LogError($"Agent is not on the NavMesh! Warping...");
                if (NavMesh.SamplePosition(agent.transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    //Debug.LogError("Failed to warp agent to NavMesh.");
                    return;
                }
            }

            // Check if newTarget is valid on the NavMesh
            if (NavMesh.SamplePosition(newTarget, out NavMeshHit hitTarget, 1.0f, NavMesh.AllAreas))
            {
                //Debug.Log("Agent pos: " + agent.transform.position);
                //Debug.Log("NewTarget pos: " + newTarget);
                
                // Check path status
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(hitTarget.position, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hitTarget.position);
                    //Debug.Log($"Path valid. Setting destination for {enemy} to: {hitTarget.position}");
                }
                else
                {
                    //Debug.LogWarning("Path incomplete or invalid. Finding alternative target...");
                    // Optional: Try finding a new target
                }
            }
            else
            {
                //Debug.LogWarning($"Failed to find valid NavMesh position near {newTarget}");
            }
        }

        public override void OnExit()
        {
            enemy.FindNewPositionStateActive = false;
            //Debug.Log("On exit called");
            //positioning.anchorPos.gameObject.SetActive(true);
            
        }
        bool HasReachedDestination()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }

        public bool isRelocating()
        {
            if (enemy.enemyDetected)
            {
                
                agent.stoppingDistance = 0f;
                return true;
            }
            else 
            {
                
                agent.stoppingDistance = enemy.enemyConfig.StoppingDistance;
                return false;
            }
        }
    }
}
