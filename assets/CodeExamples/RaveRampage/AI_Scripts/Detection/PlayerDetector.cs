
using Player;
using UnityEngine;
using Utilities;//testi
namespace Cyber_Slicer_AI
{
    public class PlayerDetector : MonoBehaviour 
    {
        // SerializeFields instead of public originally
        //public float detectionAngle = 60f; // Cone in front of enemy
        //public float detectionRadius = 10f; // Large circle around enemy
        //public float innerDetectionRadius = 5f; // Small circle around enemy
        //public float detectionCooldown = 1f; // Time between detections
        //public float attackRange = 3f; // Distance from enemy to player to attack 
        //public float rotationSpeed = 5f;

        public float detectionAngle { get; set; }
        public float detectionRadius {  get; set; }
        public float innerDetectionRadius { get; set; }
        public float detectionCooldown { get; set; }
        public float attackRange { get; set; }
        public float rotationSpeed { get; set; }

        public Transform Player {  get; private set; }
        public Health PlayerHealth { get; private set; }

        public GameObject eyeLoc;
        public GameObject targetCheck;

        CountdownTimer detectionTimer;

        public LayerMask targetLayer;
        IDetectionStrategy detectionStrategy;
        private void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerUnit>()?.CharacterTransform;
            targetLayer = LayerMask.GetMask("PlayerCharacter", "Ground");
            //var playerLayers = GameObject.FindGameObjectsWithTag("Player");
            //foreach(var Player in playerLayers)
            //{
            //    // korjaa?
            //}
        }
        void Start()
        {
            detectionTimer = new CountdownTimer(detectionCooldown);
            
            detectionStrategy = new ConeDetectionStrategy(detectionAngle,detectionRadius,innerDetectionRadius);
        }
        void Update() => detectionTimer.Tick(Time.deltaTime);

        public bool CanDetectPlayer()
        {
            
            return detectionTimer.IsRunning || detectionStrategy.Execute(Player, eyeLoc.transform, detectionTimer);
        }

        public bool CanAttackPlayer()
        {
            if(Player != null)
            {
                var directionToPlayer = Player.position - eyeLoc.transform.position;
                return directionToPlayer.magnitude <= attackRange;
            }
            return false;
        }
        

        public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) => this.detectionStrategy = detectionStrategy;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            // Draw a spheres for the radii
            Gizmos.DrawWireSphere(eyeLoc.transform.position, detectionRadius);
            Gizmos.DrawWireSphere(eyeLoc.transform.position, innerDetectionRadius);

            // Calculate our cone directions
            Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * eyeLoc.transform.forward * detectionRadius;
            Vector3 backwardConeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * eyeLoc.transform.forward * detectionRadius;

            // Draw lines to represent the cone
            Gizmos.DrawLine(eyeLoc.transform.position, eyeLoc.transform.position + forwardConeDirection);
            Gizmos.DrawLine(eyeLoc.transform.position, eyeLoc.transform.position + backwardConeDirection);
        }
        //testi kamaa
        
        public void RotateTowardsPlayer(bool canRotate)
        { 
            if (canRotate) 
            {
                Vector3 direction = (Player.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            }
            else { return; }
            
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        public bool ConfirmLineOfSight()
        {
            if (Player == null)
            {
                Debug.LogError("Player is null. Cannot confirm line of sight.");
                return false;
            }

            Vector3 directionToPlayer = Player.position - eyeLoc.transform.position;

            // Perform raycast
            if (Physics.Raycast(eyeLoc.transform.position, directionToPlayer.normalized, out RaycastHit hit, detectionRadius))
            {
                Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}");

                // Check if the hit object is on the target layer
                //if ((1 << hit.collider.gameObject.layer & targetLayer) != 0)
                if(hit.collider.name == "Character")
                {
                    //Debug.Log("Player in sight");
                    return true;
                    //// Check if the hit object is the player
                    //if (hit.transform == Player || hit.transform.IsChildOf(Player))
                    //{
                    //    Debug.Log("Line of sight confirmed: player detected.");
                    //    return true;
                    //}
                    //else
                    //{
                    //    Debug.Log($"Line of sight blocked by: {hit.collider.name}");
                    //}
                }
                else
                {
                    //Debug.Log("Cannot see player");
                    return false;
                    //Debug.Log($"Object hit is not on the target layer: {hit.collider.name}");
                }
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }
            return false;
        }

        public bool CheckRoofCollision()
        {
            Vector3 roofCheck = eyeLoc.transform.position;
            Vector3 directionUp = Vector3.up;
            int roofLayerMask = LayerMask.GetMask("Ground");

            // Draw the upward ray for visualization
            Debug.DrawLine(roofCheck, roofCheck + directionUp * 10, Color.red, 1f);

            // Perform the upward raycast for the "Roof" layer
            if (Physics.Raycast(roofCheck, directionUp, out RaycastHit hitRoof, 10f, roofLayerMask))
            {
                Debug.Log("Hit Roof: " + hitRoof.collider.name);

                return true;
            }
            return false;
        }
    }

}