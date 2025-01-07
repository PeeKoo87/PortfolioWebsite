using System.Collections;
using UnityEngine;

public class SectionedAnimationSync : MonoBehaviour
{
    [Tooltip("The Animator component controlling animations.")]
    public Animator animator; // Reference to the Animator component

    [Tooltip("Array of animation configurations for different animations.")]
    public AnimationConfig[] animations; // Configuration for each animation

    private AnimationConfig currentAnimation; // Currently playing animation
    private int currentSectionIndex = 0; // Index of the current section

    [System.Serializable]
    public class Section
    {
        public string sectionName; // Name of the section (optional, for debugging)
        [Range(0f, 1f)] public float startNormalizedTime; // Start time of the section (0 to 1)
        [Range(0f, 1f)] public float endNormalizedTime; // End time of the section (0 to 1)
        public float targetDuration; // Target duration for this section in seconds
    }

    [System.Serializable]
    public class AnimationConfig
    {
        public string animationName; // Name of the animation state
        public Section[] animationSections; // Sections for this animation
    }

    /// <summary>
    /// Plays the specified animation by name and starts its first section.
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        currentAnimation = GetAnimationConfig(animationName);
        if (currentAnimation == null)
        {
            Debug.LogError($"Animation '{animationName}' not found in the configuration.");
            return;
        }

        if (currentAnimation.animationSections.Length == 0)
        {
            Debug.LogError($"No sections defined for animation '{animationName}'.");
            return;
        }

        animator.Play(animationName, 0, currentAnimation.animationSections[0].startNormalizedTime);
        StartSection(0);
    }

    /// <summary>
    /// Stops the current animation.
    /// </summary>
    public void StopAnimation()
    {
        animator.speed = 0;
        currentSectionIndex = 0;
        Debug.Log("Animation stopped.");
    }

    /// <summary>
    /// Jumps to a specific section of the current animation.
    /// </summary>
    /// <param name="sectionIndex">The index of the section to jump to.</param>
    public void JumpToSection(int sectionIndex)
    {
        if (currentAnimation == null)
        {
            //Debug.LogError("No animation is currently playing.");
            return;
        }

        if (sectionIndex < 0 || sectionIndex >= currentAnimation.animationSections.Length)
        {
            //Debug.LogError("Invalid section index.");
            return;
        }

        StartSection(sectionIndex);
    }

    private void Update()
    {
        if (currentAnimation == null || currentAnimation.animationSections.Length == 0) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(currentAnimation.animationName))
        {
            //Debug.LogWarning("Animator is not playing the correct animation.");
            return;
        }

        float normalizedProgress = stateInfo.normalizedTime % 1; // Clamp to [0, 1]

        if (normalizedProgress >= currentAnimation.animationSections[currentSectionIndex].endNormalizedTime)
        {
            currentSectionIndex++;
            if (currentSectionIndex < currentAnimation.animationSections.Length)
            {
                StartSection(currentSectionIndex);
            }
            else
            {
                //Debug.Log($"Animation '{currentAnimation.animationName}' completed all sections.");
            }
        }
    }

    private void StartSection(int index)
    {
        currentSectionIndex = index;
        Section section = currentAnimation.animationSections[index];

        float normalizedDuration = section.endNormalizedTime - section.startNormalizedTime;
        float playbackSpeed = normalizedDuration / section.targetDuration;

        animator.speed = playbackSpeed;
        animator.Play(currentAnimation.animationName, 0, section.startNormalizedTime);

        //Debug.Log($"Started section {index} of animation '{currentAnimation.animationName}': Speed = {playbackSpeed}, Start = {section.startNormalizedTime}, End = {section.endNormalizedTime}");
    }

    private AnimationConfig GetAnimationConfig(string animationName)
    {
        foreach (var animation in animations)
        {
            if (animation.animationName == animationName)
                return animation;
        }
        return null;
    }

    /// <summary>
    /// Synchronizes progress within a section of the current animation.
    /// </summary>
    public void SyncSectionProgress(int sectionIndex, float progress)
    {
        if (currentAnimation == null)
        {
            //Debug.LogError("No animation is currently playing.");
            return;
        }

        if (sectionIndex < 0 || sectionIndex >= currentAnimation.animationSections.Length)
        {
            //Debug.LogError("Invalid section index.");
            return;
        }

        Section section = currentAnimation.animationSections[sectionIndex];
        progress = Mathf.Clamp01(progress);
        float normalizedTime = Mathf.Lerp(section.startNormalizedTime, section.endNormalizedTime, progress);

        animator.Play(currentAnimation.animationName, 0, normalizedTime);
        //Debug.Log($"Section {sectionIndex} Progress: {progress} -> Normalized Time: {normalizedTime}");
    }
}
