using AudioSystem;
using Player;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace Cyber_Slicer_AI
{
    public class AI_Enemy_Positioning : MonoBehaviour
    {

        public LayerMask targetLayer;        // Layer of the targets to hit
        private Collider[] hitResults;       // Reusable array for collision results
        private int maxTargets = 10;         // Max targets to detect
        public float checkRadius = 2f;       // Radius of the detection sphere
        public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset for sphere center

        int counter = 0;
        public GameObject anchorPos;

        private List<Enemy>detectedEnemies = new List<Enemy>();
        Vector3 directionAway;
        Enemy enemy;

        
        private void Awake()
        {
            // Preallocate hitResults array
            hitResults = new Collider[maxTargets];
            enemy = GetComponent<Enemy>();
        }

        public bool DetectEnemies()
        {
            Vector3 spherePosition = transform.position + transform.TransformDirection(offset);

            // Clear previous detections
            detectedEnemies.Clear();

            // Perform the sphere overlap check
            int hitCount = Physics.OverlapSphereNonAlloc(
                spherePosition,
                checkRadius,
                hitResults,
                targetLayer
            );

            Vector3 summedDirections = Vector3.zero;
            int validEnemyCount = 0;

            for (int i = 0; i < hitCount; i++)
            {
                //Debug.Log(hitResults[i].GetComponentInParent<Enemy>().enemyID);
                // Ensure the hit object is not this anchor and has an Enemy component
                if (hitResults[i].GetComponentInParent<Enemy>().enemyID != this.enemy.enemyID /*&& hitResults[i].GetComponentInParent<Enemy>() != null*/)
                {
                    //Debug.Log(hitResults[i].name);
                    Enemy detectedEnemy = hitResults[i].GetComponentInParent<Enemy>();
                    detectedEnemies.Add(detectedEnemy);
                    
                    // Calculate direction away from this enemy
                    Vector3 temp = transform.position - detectedEnemy.transform.position;
                    summedDirections += temp.normalized; // Normalize each vector for equal weighting
                    validEnemyCount++;
                }
            }

            // Calculate the average direction away
            if (validEnemyCount > 0)
            {
                directionAway = summedDirections / validEnemyCount; // Average direction away
                directionAway.Normalize(); // Normalize to unit length
                //Debug.Log($"{enemy}, direction to move: {directionAway}");
            }
            else
            {
                directionAway = GetComponent<PlayerDetector>().Player.transform.position; // No movement if no valid enemies
            }

            counter = validEnemyCount; // Update counter for debugging
            //Debug.Log(counter);
            // Return true if enemies were detected
            return validEnemyCount >= 1;
        }

        public Vector3 MoveTarget()
        {
            //Debug.Log("move target called");
            // Ensure this returns a meaningful position
            if (directionAway == Vector3.zero)
            {
                //Debug.LogWarning("directionAway is zero, returning default position.");
                return transform.position; // Or some fallback position
            }

            Vector3 targetPosition = transform.position + directionAway.normalized * 5f; // Example offset
            //Debug.Log($"Calculated new target position: {targetPosition}");
            return targetPosition;
        }
        public void MoveToNewLocation()
        {
            //Debug.Log($"Hit target check from:{anchorPos.name}");
            for (int i = 0; i< counter; i++)
            {
                //Debug.Log(hitResults[i].name);
            }
            
            //Debug.Log($"Enemy: {enemyNames}, at location: {enemyLocations}");
            //Debug.Log("Number of enemies: " + counter);
            
            //#TODO: Relocation logic.

        }
        public void OnDrawGizmos()
        {
            if (counter > 0)
            {
                // Visualize the detection radius
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + offset, checkRadius);
            }
            else
            {
                // Visualize the detection radius
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position + offset, checkRadius);
            }

        }
    }
}


