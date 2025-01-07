using AudioSystem;
using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cyber_Slicer_AI
{
    public class AI_Boss : MonoBehaviour
    {
        public Vector2Int punchDamage;
        public Vector2Int bellyFlopDamage;
        public LayerMask targetLayer;        // Layer of the targets to hit
        private Collider[] hitResults;       // Reusable array for collision results
        private int maxTargets = 10;         // Max targets to detect
        public float bellyFlopCheckRadius = 2f;
        public float punchCheckRadius = 2f;// Radius of the detection sphere
        public Vector3 offset = new Vector3(0, 0, 2); // Offset for sphere center
        public Vector3 leftFistOffset = new Vector3(0, 0, 0);
        public Vector3 rightFistOffset = new Vector3(0, 0, 0);
        public SoundData BellyFlopSound;
        public GameObject leftFist;
        public GameObject rightFist;
        public GameObject bellyTarget;

        private void Awake()
        {
            // Preallocate hitResults array
            hitResults = new Collider[maxTargets];

        }
        public void BellyFlopDamage()
        {
            int randomizedSpinDamage = Random.Range(bellyFlopDamage.x, bellyFlopDamage.y + 1);
            // Calculate the position for the sphere (in front of the player)
            Vector3 spherePosition = bellyTarget.transform.position + transform.TransformDirection(offset);

            SoundManager.Instance.CreateSound()
                .WithSoundData(BellyFlopSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition,
                bellyFlopCheckRadius,
                hitResults,
                targetLayer
            );

            // Log detected targets
            for (int i = 0; i < hitCount; i++)
            {

                Debug.Log($"Hit target: {hitResults[i].gameObject.name}");
                if (hitResults[i].gameObject.GetComponent<IDamageable>() is not PlayerUnit player) return;
                var damage = new Damage
                {
                    Amount = randomizedSpinDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);
            }
        }

        public void LeftPunchDamage()
        {
            int punchDamage = Random.Range(this.punchDamage.x, this.punchDamage.y + 1);
            // Calculate the position for the sphere (in front of the player)
            Vector3 spherePosition2 = leftFist.transform.position + transform.TransformDirection(leftFistOffset);

            SoundManager.Instance.CreateSound()
                .WithSoundData(BellyFlopSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition2,
                punchCheckRadius,
                hitResults,
                targetLayer
            );

            // Log detected targets
            for (int i = 0; i < hitCount; i++)
            {

                Debug.Log($"Hit target: {hitResults[i].gameObject.name}");
                if (hitResults[i].gameObject.GetComponent<IDamageable>() is not PlayerUnit player) return;
                var damage = new Damage
                {
                    Amount = punchDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);
            }
        }
        public void RightPunchDamage()
        {
            int punchDamage = Random.Range(this.punchDamage.x, this.punchDamage.y + 1);
            // Calculate the position for the sphere (in front of the player)
            Vector3 spherePosition3 = rightFist.transform.position + transform.TransformDirection(rightFistOffset);

            SoundManager.Instance.CreateSound()
                .WithSoundData(BellyFlopSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition3,
                punchCheckRadius,
                hitResults,
                targetLayer
            );

            // Log detected targets
            for (int i = 0; i < hitCount; i++)
            {

                Debug.Log($"Hit target: {hitResults[i].gameObject.name}");
                if (hitResults[i].gameObject.GetComponent<IDamageable>() is not PlayerUnit player) return;
                var damage = new Damage
                {
                    Amount = punchDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);
            }
        }

        //private void OnDrawGizmosSelected()
        //{
        //    // Visualize the sphere in the editor - belly
        //    Gizmos.color = Color.red;
        //    Vector3 spherePosition = transform.position + transform.TransformDirection(offset);
        //    Gizmos.DrawWireSphere(spherePosition, bellyFlopCheckRadius);

        //    // Visualize the sphere in the editor -left fist
        //    Gizmos.color = Color.cyan;
        //    Vector3 spherePosition2 = transform.position + transform.TransformDirection(leftFistOffset);
        //    Gizmos.DrawWireSphere(spherePosition2, punchCheckRadius);

        //    // Visualize the sphere in the editor - right fist
        //    Gizmos.color = Color.magenta;
        //    Vector3 spherePosition3 = transform.position + transform.TransformDirection(rightFistOffset);
        //    Gizmos.DrawWireSphere(spherePosition3, punchCheckRadius);
        //}
    }
}

