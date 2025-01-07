using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class BossDashState : EnemyBaseState
    {
        readonly CountdownTimer dashAttackTimer;
        readonly NavMeshAgent agent;
        float velocityModEnter = 0;
        float velocityModExit = 1;
        AI_Boss boss;
        public BossDashState(Enemy enemy, Animator animator, NavMeshAgent agent, CountdownTimer dashAttackTimer) : base(enemy, animator)
        {

            this.agent = agent;
            this.dashAttackTimer = dashAttackTimer;
        }

        public override void OnEnter()
        {
            agent.velocity = agent.velocity * velocityModEnter;
            if (dashAttackTimer.IsRunning) { return; }
            enemy.StartBellyDash();
        }
        public override void OnExit()
        {
            agent.velocity = agent.velocity * velocityModExit;
        }
    }
}

