using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public abstract class EnemyBaseState : IState_Interface
    {
        protected readonly Enemy enemy;
        protected readonly Animator animator;

        ////Get animation hashes
        //protected static readonly int IdleHash = Animator.StringToHash("Idle_Guard_AR");
        //protected static readonly int RunHash = Animator.StringToHash("Run_guard_AR");
        //// walk hash
        //protected static readonly int WalkHash = Animator.StringToHash("WalkFront_Shoot_AR");
        //// attack hash
        //protected static readonly int AttackHash = Animator.StringToHash("Shoot_BurstShot_AR");
        //// death hash
        //protected static readonly int DieHash = Animator.StringToHash("Die");

        //Get animation hashes
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int RunHash = Animator.StringToHash("Run");
        protected static readonly int WalkHash = Animator.StringToHash("Walk");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");
        protected static readonly int DieHash = Animator.StringToHash("Death");
        protected static readonly int ReloadHash = Animator.StringToHash("Reload");

        protected static readonly int MoveBlend = Animator.StringToHash("Movement");//testi
        protected static readonly int CombatMoveBlend = Animator.StringToHash("Movement/Combat");//testi

        protected const float crossFadeDuration = 0.1f;
        protected EnemyBaseState(Enemy enemy, Animator animator)
        {
            this.enemy = enemy;
            this.animator = animator;
        }

        public virtual void FixedUpdate()
        {
            //no-op
        }

        public virtual void OnEnter()
        {
            //no-op
        }

        public virtual void OnExit()
        {
            //no-op
        }

        public virtual void Update()
        {
            //no-op
        }

    }
}
