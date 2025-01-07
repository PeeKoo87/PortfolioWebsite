using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class BossBellyFlopState : EnemyBaseState
    {
        readonly CountdownTimer bellyFlopAttackTimer;
        readonly NavMeshAgent agent;
        float velocityModEnter = 0;
        float velocityModExit = 1;
        AI_Boss boss;
        public BossBellyFlopState(Enemy enemy, Animator animator,NavMeshAgent agent, CountdownTimer bellyFlopAttackTimer) : base(enemy, animator)
        {
            
            this.agent = agent;
            this.bellyFlopAttackTimer = bellyFlopAttackTimer;
        }

        public override void OnEnter()
        {
            agent.velocity = agent.velocity * velocityModEnter;
            if (bellyFlopAttackTimer.IsRunning) { return; }
            enemy.StartBellyFlop();
        }
        public override void OnExit()
        {
            agent.velocity = agent.velocity * velocityModExit;
        }
    }
}

