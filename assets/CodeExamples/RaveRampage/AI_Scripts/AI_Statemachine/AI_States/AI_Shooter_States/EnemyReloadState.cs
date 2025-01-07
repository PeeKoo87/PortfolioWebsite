using UnityEngine;

namespace Cyber_Slicer_AI
{
    public class EnemyReloadState : EnemyBaseState
    {
        public EnemyReloadState(Enemy enemy, Animator animator) : base(enemy, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(ReloadHash, crossFadeDuration);
        }
    }
}
