using UnityEngine;
using UnityEngine.AI;
namespace Cyber_Slicer_AI
{
    public class EnemyDeathState : EnemyBaseState
    {
        
        readonly NavMeshAgent agent;
        readonly CountdownTimer despawnTimer;
        readonly AI_Enemy_Positioning AiPositioning;
        public EnemyDeathState(Enemy enemy,NavMeshAgent agent, Animator animator,AI_Enemy_Positioning AiPositioning ,CountdownTimer despawnTimer) : base(enemy, animator)
        {
            this.agent = agent;
            this.AiPositioning = AiPositioning;
        }
        public override void OnEnter()
        { 
            //despawnTimer.Start();
            //animator.SetTrigger("IsDead");
            //enemy.collider.enabled = false; // testi collider
            //enemy.isAlive = false;
            //enemy.enabled = false;

            //if (!despawnTimer.IsRunning)
            //{
            //    GameObject.Destroy(enemy);
            //}
            //agent.enabled = false;

            
        }
    }
}

