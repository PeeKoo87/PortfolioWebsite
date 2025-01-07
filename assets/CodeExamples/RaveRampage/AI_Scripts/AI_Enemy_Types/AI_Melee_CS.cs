using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class AI_Melee_CS : MonoBehaviour
    {
        private bool hasDashed;
        private Animator animator;
        private NavMeshAgent navAgent;
        private EnemyBaseState AIBase;
        private PlayerDetector playerDetector;
        private void Start()
        {
            animator = GetComponent<Animator>();
            navAgent = GetComponentInParent<NavMeshAgent>();
        }
        public void Dash()
        {
            if (!hasDashed)
            {
                // dash movement logic here
                
            }
            else { return; }
            hasDashed = true;
        }
        public void DashAttack(string hashInput)
        {
            
            bool attackComplete;
            if (hasDashed) 
            {

                //Dash Attack logic here
                animator.Play(hashInput); // gotta figure out the details
                //Set dash attack complete flag
                attackComplete = true;
            }
            else {
                attackComplete = false;    
                return; 
            }

            if (attackComplete) 
            {
                SpinAttack();
            }
        }

        public void SpinAttack()
        {
            // Spin attack logic here
        }
    }

}
