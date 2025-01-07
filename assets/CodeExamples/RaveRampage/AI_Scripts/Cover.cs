using Cyber_Slicer_AI;
using UnityEngine;

public class Cover : MonoBehaviour
{
    public LayerMask targetLayer;        // Layer of the targets to hit
    private Collider[] hitResults;       // Reusable array for collision results
    private const int maxTargets = 10;   // Max targets to detect
    public float checkRadius = 2f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private string occupyingEnemyID; // Stores the ID of the occupying enemy, if any

    void Awake()
    {
        hitResults = new Collider[maxTargets];
    }

    /// <summary>
    /// Overloaded version: Checks if the cover is occupied without returning the enemy ID.
    /// </summary>
    public bool IsCoverOccupied()
    {
        return IsCoverOccupied(out _); // Call the detailed version but discard the output
    }

    /// <summary>
    /// Detailed version: Checks if the cover is occupied and outputs the occupying enemy's ID.
    /// </summary>
    public bool IsCoverOccupied(out string enemyID)
    {
        enemyID = null;
        Vector3 spherePosition = transform.position + transform.TransformDirection(offset);
        int hitCount = Physics.OverlapSphereNonAlloc(
            spherePosition,
            checkRadius,
            hitResults,
            targetLayer
        );

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                Enemy enemy = hitResults[i].GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemyID = enemy.enemyID;
                    occupyingEnemyID = enemyID; // Cache the enemy ID for other methods
                    return true;
                }
            }
        }

        occupyingEnemyID = null;
        return false;
    }

    private void OnDrawGizmos()
    {
        // Visualize cover occupation status in the editor
        Gizmos.color = occupyingEnemyID != null ? Color.red : Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
    }
}
