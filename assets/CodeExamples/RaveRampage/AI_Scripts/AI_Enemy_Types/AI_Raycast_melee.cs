using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public class AI_Raycast_melee : MonoBehaviour
    {
        // Reference to the start and end points (empty GameObjects)
        public Transform startPoint;
        public Transform endPoint;

        // The layer you want the raycast to check for
        public LayerMask hitLayer;
        [SerializeField]private bool isTraceActive = false;
        [SerializeField]private bool hasHit = false;
        Enemy enemy;
        void Awake()
        {
            enemy = transform.parent.GetComponent<Enemy>();
        }
        void Update()
        {
            
            if(isTraceActive == true)
            {
                // Calculate direction from startPoint to endPoint
                Vector3 direction = endPoint.position - startPoint.position;
                float distance = direction.magnitude; // Get the distance between the two points
                direction.Normalize(); // Normalize the direction
                
                // Perform the raycast
                RaycastHit hit;
                if (Physics.Raycast(startPoint.position, direction, out hit, distance,hitLayer))
                {
                    if (hasHit) { return; }

                    

                    Debug.Log("Hit: " + hit.collider.name);
                    hasHit = true;
                    
                }
                else
                {
                    //Debug.Log("No hit detected");
                    hasHit = false;
                }
                
                // Visualize the ray in the Scene view (optional)
                Debug.DrawLine(startPoint.position, endPoint.position, Color.red);
            }
            
        }

        public void ActivateTrace()
        {
            Debug.Log("Trace active!");
            isTraceActive = true;
        }
        public void DeactivateTrace()
        {
            Debug.Log("Trace inactive!");
            isTraceActive = false;
        }
    }
}
