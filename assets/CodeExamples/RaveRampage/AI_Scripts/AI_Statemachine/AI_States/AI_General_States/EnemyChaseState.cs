using Player.Abilities;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyChaseState : EnemyBaseState
    {
        readonly NavMeshAgent agent;
        readonly Transform player;
        public EnemyChaseState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
        {
            this.agent = agent;
            this.player = player;
        }

        public override void OnEnter()
        {
            animator.CrossFade(CombatMoveBlend, crossFadeDuration);
            enemy.ChaseStateActive = true;
        }

        public override void OnExit()
        {
            enemy.ChaseStateActive = false;
        }

        public override void Update()
        {
            if (!player) return;

            if (!agent.isOnNavMesh)
            {
                Debug.LogError("Agent is not on navmesh: " + agent.gameObject.name);

                // Get closest navmesh point 
                if (NavMesh.SamplePosition(agent.transform.position, out var hit, float.MaxValue, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
            
            agent.SetDestination(player.position);
        }

        //public void GetDistance()
        //{
        //    if (enemy.gameObject.name == "HyenaDude")
        //    {
        //        Debug.Log("Calculating distance");
        //        float dashdistance = Vector3.Distance(agent.transform.position, player.transform.position);
        //        Debug.Log("Distance between player and enemy" + dashdistance);

        //        if(dashdistance < 15)
        //        {
        //            agent.speed = 20;
        //        }
        //        else if(dashdistance > 10)
        //        {
        //            agent.speed = 5;
        //        }
        //    }
        //}
    }
}
