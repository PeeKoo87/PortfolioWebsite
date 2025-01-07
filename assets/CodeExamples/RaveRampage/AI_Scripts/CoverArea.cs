using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverArea : MonoBehaviour
{
    private Cover[] covers;

    void Awake()
    {
        covers = GetComponentsInChildren<Cover>();
    }

    /// <summary>
    /// Returns a random cover within the area, optionally considering agent location.
    /// </summary>
    public Cover GetRandomCover(Vector3 agentLocation)
    {
        if (covers == null || covers.Length == 0)
        {
            Debug.LogWarning("No covers found in CoverArea.");
            return null;
        }
        return covers[Random.Range(0, covers.Length)];
    }

    /// <summary>
    /// Searches all covers and returns the first unoccupied one, or null if all are occupied.
    /// </summary>
    public Cover GetUnoccupiedCover()
    {
        foreach (var cover in covers)
        {
            if (!cover.IsCoverOccupied()) // Uses the no-argument version
            {
                return cover;
            }
        }

        Debug.Log("All covers are occupied.");
        return null; // All covers are occupied
    }

    /// <summary>
    /// Checks if all covers are occupied.
    /// </summary>
    public bool AreAllCoversOccupied()
    {
        foreach (var cover in covers)
        {
            if (!cover.IsCoverOccupied()) // Uses the no-argument version
            {
                return false; // Found at least one free cover
            }
        }

        return true; // All covers are occupied
    }
}
