using AudioSystem;
using Debugging;
using Player;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public class AI_Melee_Weapon : MonoBehaviour
    {
        //public int damage { get; set; }      // Damage dealt by the weapon
        //public int chargeDamage { get; set; } //ChargeAttack damage
        public Vector2Int spinDamageRange;
        public Vector2Int chargeDamageRange;
        public int damage = 2;
        public int chargeDamage = 15;
        public LayerMask targetLayer;        // Layer of the targets to hit
        private Collider[] hitResults;       // Reusable array for collision results
        private int maxTargets = 10;         // Max targets to detect
        public float checkRadius = 2f;       // Radius of the detection sphere
        public Vector3 offset = new Vector3(0, 0, 2); // Offset for sphere center
        public SoundData swordSpinSound;
        public SoundData FootStepSound;
        Enemy enemy;
        private void Awake()
        {
            // Preallocate hitResults array
            hitResults = new Collider[maxTargets];
            enemy = GetComponentInParent<Enemy>();
        }
        
        public void SetMeleeParameters()
        {
            damage = enemy.enemyConfig.MeleeDamage;
            chargeDamage = enemy.enemyConfig.ChargeAttackDamage;
        }
        public void PerformAttack()
        {
            int randomizedSpinDamage = Random.Range(spinDamageRange.x, spinDamageRange.y + 1); // Spin damage
            randomizedSpinDamage = Mathf.RoundToInt(randomizedSpinDamage * (1 + enemy.Stats.Damage * 0.01f));
            
            // Calculate the position for the sphere (in front of the player)
            Vector3 spherePosition = transform.position + transform.TransformDirection(offset);

            SoundManager.Instance.CreateSound()
                .WithSoundData(swordSpinSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition,
                checkRadius,
                hitResults,
                targetLayer
            );

            // Log detected targets
            for (int i = 0; i < hitCount; i++)
            {
                
               // Debug.Log($"Hit target: {hitResults[i].gameObject.name}");
                if (hitResults[i].gameObject.GetComponent<IDamageable>() is not PlayerUnit player) return;
                var damage = new Damage
                {
                    Amount = randomizedSpinDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);
            }
        }

        public void PerformChargeAttack()
        {
            DBug.Assert(enemy != null, "enemy is null"); ;
            DBug.Assert(enemy.Stats != null, "enemy.Stats is null");
            int randomizedChargeDamage = Random.Range(chargeDamageRange.x, chargeDamageRange.y + 1); // charge damage
            randomizedChargeDamage = Mathf.RoundToInt(randomizedChargeDamage * (1 + enemy.Stats.Damage * 0.01f));
            // Calculate the position for the sphere (in front of the player)
            Vector3 spherePosition = transform.position + transform.TransformDirection(offset);

            SoundManager.Instance.CreateSound()
                .WithSoundData(swordSpinSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition,
                checkRadius,
                hitResults,
                targetLayer
            );

            // Log detected targets
            for (int i = 0; i < hitCount; i++)
            {

               // Debug.Log($"Hit target: {hitResults[i].gameObject.name}");
                if (hitResults[i].gameObject.GetComponent<IDamageable>() is not PlayerUnit player) return;

                var damage = new Damage
                {
                    Amount = randomizedChargeDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);
            }
        }

        public void Footstep()
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(FootStepSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the sphere in the editor
            Gizmos.color = Color.red;
            Vector3 spherePosition = transform.position + transform.TransformDirection(offset);
            Gizmos.DrawWireSphere(spherePosition, checkRadius);
        }
    }
}
