using System.Collections;
using UnityEngine;

public class AnimationSync : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component

    /// <summary>
    /// Synchronizes the animation to the distance and movement speed.
    /// </summary>
    /// <param name="animationName">The name of the animation state to play.</param>
    /// <param name="distance">The total distance to cover.</param>
    /// <param name="speed">The movement speed (units per second).</param>
    /// <param name="animator">The animator controlling the animation.</param>
    public void SyncAnimationToDistance(string animationName, float distance, float speed, Animator animator)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned.");
            return;
        }

        // Calculate the duration required to cover the distance at the given speed
        float duration = distance / speed;

        // Ensure the animator is set to the desired animation
        animator.Play(animationName, 0, 0f);

        // Wait for one frame to allow the animator state to update
        StartCoroutine(AdjustAnimationSpeed(animationName, duration, animator));
    }

    private IEnumerator AdjustAnimationSpeed(string animationName, float duration, Animator animator)
    {
        yield return null; // Wait one frame for the animator state to update

        // Get the animation's original duration
        float animationDuration = GetAnimationDuration(animationName, animator);

        if (animationDuration > 0)
        {
            // Calculate the playback speed to match the movement duration
            float playbackSpeed = animationDuration / duration;
            animator.speed = playbackSpeed;

            Debug.Log($"Animation '{animationName}' synced: Duration = {duration}s, Playback Speed = {playbackSpeed}");
        }
        else
        {
            Debug.LogWarning($"Failed to fetch duration for animation '{animationName}'.");
        }
    }

    private float GetAnimationDuration(string animationName, Animator animator)
    {
        // Ensure the animator has the correct state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(animationName))
        {
            return stateInfo.length; // Length of the animation in seconds
        }

        Debug.LogWarning($"Animation state '{animationName}' not active or name mismatch.");
        return 0f;
    }

    ///// <summary>
    ///// Synchronizes the animation to the position change across the distance traveled.
    ///// </summary>
    ///// <param name="animationName">The name of the animation state to play.</param>
    ///// <param name="startPosition">The starting position of the enemy.</param>
    ///// <param name="endPosition">The target position of the enemy.</param>
    ///// <param name="animator">The animator controlling the animation.</param>
    //public void SyncAnimationToDistance(string animationName, Vector3 startPosition, Vector3 endPosition, Animator animator)
    //{
    //    if (animator == null)
    //    {
    //        Debug.LogError("Animator is not assigned.");
    //        return;
    //    }

    //    // Calculate the total distance to cover
    //    float totalDistance = Vector3.Distance(startPosition, endPosition);

    //    // Ensure the animator is set to the desired animation
    //    animator.Play(animationName, 0, 0f);
    //    animator.speed = 0; // Pause the animation initially

    //    // Start syncing animation progress with position
    //    StartCoroutine(SyncAnimationWithMovement(animationName, startPosition, endPosition, totalDistance, animator));
    //}

    //private IEnumerator SyncAnimationWithMovement(string animationName, Vector3 startPosition, Vector3 endPosition, float totalDistance, Animator animator)
    //{
    //    Vector3 currentPosition = startPosition;

    //    while (Vector3.Distance(currentPosition, endPosition) > 0.01f)
    //    {
    //        // Calculate progress as a fraction of the distance traveled
    //        float traveledDistance = totalDistance - Vector3.Distance(currentPosition, endPosition);
    //        float progress = traveledDistance / totalDistance;

    //        // Update the animator's normalized time to match the progress
    //        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    //        if (stateInfo.IsName(animationName))
    //        {
    //            animator.Play(animationName, 0, progress);
    //        }

    //        // Update current position (assuming the object moves externally)
    //        currentPosition = transform.position;

    //        yield return null; // Wait for the next frame
    //    }

    //    // Ensure animation reaches the end
    //    animator.Play(animationName, 0, 1f);
    //    animator.speed = 1; // Reset the animator speed to normal
    //}
}
