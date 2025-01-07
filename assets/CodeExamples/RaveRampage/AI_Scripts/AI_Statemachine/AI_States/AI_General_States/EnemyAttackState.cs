using Debugging;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class EnemyAttackState : EnemyBaseState
    {
        readonly NavMeshAgent agent;
        readonly Transform player;
        readonly CountdownTimer attackTimer;
        readonly CountdownTimer spinAttackTimer;
        readonly CountdownTimer moveCooldownTimer;
        readonly AI_Enemy_Positioning positioning;

        Cover cover;
        public EnemyAttackState(Enemy enemy, Animator animator, NavMeshAgent agent, 
            Transform player, CountdownTimer attackTimer,CountdownTimer spinAttackTimer,CountdownTimer moveCooldownTimer,AI_Enemy_Positioning positioning) : base(enemy, animator)
        {
            this.agent = agent;
            this.player = player;
            this.attackTimer = attackTimer;
            this.spinAttackTimer = spinAttackTimer;
            this.positioning = positioning;
            this.moveCooldownTimer = moveCooldownTimer;
        }
        
        public override void OnEnter()
        {
            enemy.AttackStateActive = true;
        }

        public override void OnExit()
        {
            
            agent.speed = enemy.enemyConfig.Speed;
            enemy.AttackStateActive = false;
        }

        public override void Update()
        {

            //if (enemy.enemyDetected)
            //{
            //    Debug.Log("enemies collided, recalculating movement");
            //    agent.stoppingDistance = 0;
            //    agent.SetDestination(positioning.MoveTarget());

            //}
            //else if(!enemy.enemyDetected)
            //{
            //    Debug.Log("default locomotion state");
            //    agent.stoppingDistance = enemy.enemyConfig.StoppingDistance;
            //    agent.SetDestination(player.position);
            //}
            

            if (enemy.IsEnemyMelee() == false && enemy.IsEnemyBoss() == false && enemy.IsEnemySniper() == false)
            {
                agent.SetDestination(player.position);
                if (attackTimer.IsRunning) { return; }
                FireWeapon();
                animator.CrossFade(CombatMoveBlend, crossFadeDuration);
            }
            else if (enemy.IsEnemyMelee() == false && enemy.IsEnemyBoss() == false && enemy.IsEnemySniper() == true)
            {
                //if (cover.occupyingEnemyID == this.enemy.enemyID && cover.IsCoverOccupied(this.enemy.enemyID)) { agent.speed = enemy.enemyConfig.SpeedInCover; }
                //else { agent.SetDestination(player.position); }
                //if (Vector3.Distance(player.position, agent.transform.position) <15f) { agent.SetDestination(agent.transform.position + new Vector3(0, 0, -2)); };
                //Debug.Log(Vector3.Distance(player.position, agent.transform.position));
                agent.SetDestination(player.position);
                if (attackTimer.IsRunning) { return; }
                FireWeapon();
                animator.CrossFade(CombatMoveBlend, crossFadeDuration);
            }
            else if (enemy.IsEnemyMelee() == true && enemy.IsEnemyBoss() == false)
            {
                agent.SetDestination(player.position);
                SwingWeapon();
            }
            else if (enemy.IsEnemyBoss() == true && enemy.IsEnemyMelee() == false)
            {

                //Debug.Log(attackTimer.Progress);
                enemy.Punch();
                //enemy.LeftPunch();
                //attackTimer.Start();
            }
        }
        
        private void FireWeapon()
        {            
            
            
            attackTimer.Start();
            //Debug.Log("Shooting!");
            //if (animator.GetFloat("Speed") <= 0.2)
            //{
            //    float sideStepMod = Random.Range(-10, 10);
            //    Vector3 sideStep = new Vector3(sideStepMod, 0, 0);
            //    agent.SetDestination(sideStep);
            //    Debug.Log($"moving to: {sideStep}, {sideStepMod}");
            //}
            
            animator.CrossFade(AttackHash,crossFadeDuration);
        }

        private void SwingWeapon()
        {
            if (spinAttackTimer.IsRunning) { return ; }
            enemy.SpinAttack();
        }
        
        
    }
}
