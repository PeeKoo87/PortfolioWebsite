using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyChargeState : EnemyBaseState
    {
        private readonly Transform player;
        private readonly NavMeshAgent agent;
        private readonly PlayerDetector detector;
        private readonly CountdownTimer chargeTimer;
        
        public EnemyChargeState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player, PlayerDetector detector, CountdownTimer chargeTimer) : base(enemy, animator)
        {
            this.player = player;
            this.agent = agent;
            this.detector = detector;
            this.chargeTimer = chargeTimer;
        }

        public override void OnEnter()
        {
            enemy.ChargeStateActive = true;
            if (chargeTimer.IsRunning) { return; }
            enemy.StartCharge();
        }

        public override void OnExit()
        {           
            enemy.ChargeStateActive = false;            
        }

        public override void Update() 
        {
            
        }
    }
}
