using Cyber_Slicer_AI;
using Debugging;
using Player;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VisualEffects;

public class ImpactDamageEffect : MonoBehaviour
{
    Enemy enemy;
    public float sizeTarget = 4f; // Maximum radius of the effect
    public float growthDuration = 0.5f; // Time to grow
    public float shrinkDuration = 0.2f; // Time to shrink
    public LayerMask detectionLayer; // Layer to detect with the OverlapSphere
    public int maxColliders = 10; // Maximum number of colliders the sphere can detect

    //public int punchDamageAmount { get; set; }
    //public int slamDamageAmount {  get; set; }
    //public int bellyFlopDamageAmount {  get; set; }

    public int punchDamageAmount = 10;
    public int slamDamageAmount = 15;
    public int bellyFlopDamageAmount = 30;
    private Collider[] detectedColliders; // Array to hold detected colliders
    private float currentRadius = 0f; // Current radius of the effect
    private bool isActive = false; // To control when to draw the gizmos
    private bool playerDamaged = false; // Tracks if the player has already taken damage
    public AnimationCurve sizeoverLifeCurve;


    public GameObject impactPrefab;
    public GameObject leftHandTarget;
    public GameObject rightHandTarget;
    public GameObject slamTarget;

    private void Awake()
    {
        // Initialize the collider array
        detectedColliders = new Collider[maxColliders];
        
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("key pressed");
            BellyFlopImpactEffect();
        }
    }

    public void StartNewAttack()
    {
        // Allow damage detection again for a new attack
        playerDamaged = false;
    }

    public void SpawnImpactEffect()
    {
        TriggerImpactEffect(transform.position, 0, sizeoverLifeCurve); // Default damage amount for generic impacts
    }

    public void LeftPunchImpactEffect()
    {
        if (leftHandTarget != null)
        {
            TriggerImpactEffect(leftHandTarget.transform.position, punchDamageAmount, sizeoverLifeCurve);
        }
    }

    public void RightPunchImpactEffect()
    {
        if (rightHandTarget != null)
        {
            TriggerImpactEffect(rightHandTarget.transform.position, punchDamageAmount, sizeoverLifeCurve);
        }
    }

    public void BellyFlopImpactEffect()
    {
        TriggerImpactEffect(transform.position, bellyFlopDamageAmount, sizeoverLifeCurve);
    }

    public void SlamImpactEffect()
    {
        if (slamTarget != null)
        {
            TriggerImpactEffect(slamTarget.transform.position, slamDamageAmount, sizeoverLifeCurve);
        }
    }

    private void TriggerImpactEffect(Vector3 position, int damageAmount, AnimationCurve sizeOverLifeCurve)
    {
        if (impactPrefab == null)
        {
            Debug.LogError("Impact Prefab is not assigned!");
            return;
        }

        // Instantiate the impact effect at the specified position
        GameObject impactInstance = Instantiate(impactPrefab, position, Quaternion.identity);

        //float elapsedTime = 0;
        //float targetDuration = 4; 
        //while(elapsedTime < targetDuration)
        //{
        //    currentRadius = Mathf.Lerp(0f, 5, elapsedTime / 5);
        //    VFX.PlayDamageAreaEffect(position, currentRadius * 5, elapsedTime, sizeOverLifeCurve);
        //    elapsedTime += Time.deltaTime;
        //}

        
        //VFX.PlaySmokeEffect(position);
        VFX.PlayRubbleEffect(position,100, new Vector2(1,3),1,10,15,-24,1);
        // Trigger the animator function (if the prefab has an Animator component)
        Animator animator = impactInstance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("PlayEffect");
        }

        // Start the grow-shrink coroutine on the spawned instance
        ImpactDamageEffect effectScript = impactInstance.GetComponent<ImpactDamageEffect>();
        if (effectScript != null)
        {
            StartCoroutine(effectScript.ScaleSphereOnce(sizeTarget, growthDuration, shrinkDuration, damageAmount,position, sizeOverLifeCurve));
        }

        // Optionally destroy the impact instance after the effect completes
        Destroy(impactInstance, growthDuration + shrinkDuration + 0.5f); // Add a buffer time
    }
    
    private IEnumerator ScaleSphereOnce(float targetRadius, float growDuration, float shrinkDuration, int damageAmount, 
        Vector3 position, AnimationCurve sizeOverLifeCurve)
    {
        isActive = true; // Enable gizmo drawing

        // Phase 1: Grow the sphere
        float elapsedTime = 0f;
        //var areaEffect = VFX.PlayDamageAreaEffect(position + new Vector3(0,-0.2f,0), 1, growDuration + shrinkDuration, sizeOverLifeCurve);
        var smokeEffect = VFX.PlaySmokeEffect(position, 1f);
        while (elapsedTime < growDuration)
        {
            currentRadius = Mathf.Lerp(0f, targetRadius, elapsedTime / growDuration);
            DetectObjects(currentRadius, damageAmount); // Detect objects within the current radius
            //areaEffect.transform.localScale = currentRadius * 9 * Vector3.one;
            smokeEffect.transform.localScale = currentRadius * 0.2f * Vector3.one;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentRadius = targetRadius; // Ensure the radius is exact at the end of the grow phase
        DetectObjects(currentRadius, damageAmount);

        // Phase 2: Shrink the sphere
        elapsedTime = 0f;
        while (elapsedTime < shrinkDuration)
        {
            currentRadius = Mathf.Lerp(targetRadius, 0f, elapsedTime / shrinkDuration);
            DetectObjects(currentRadius, damageAmount); // Detect objects during the shrink phase
            //areaEffect.transform.localScale = currentRadius * 9 * Vector3.one; // Test
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentRadius = 0f; // Reset radius
        isActive = false; // Disable gizmo drawing
    }

    private void DetectObjects(float radius, int impactDamage)
    {
        // Skip if the player has already taken damage
        if (playerDamaged)
            return;

        // Detect objects within the sphere
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, detectedColliders, detectionLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider collider = detectedColliders[i];

            // Check if the object is the player
            if (collider.gameObject.GetComponent<IDamageable>() is PlayerUnit player)
            {
                // Apply the correct damage amount
                var damage = new Damage
                {
                    Amount = impactDamage,
                    Type = DamageType.Slash
                };
                player.Damage(ref damage);

                // Mark the player as damaged
                playerDamaged = true;
                Debug.Log($"Player hit with {impactDamage} damage.");
                return; // Exit the loop as the player has been damaged
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!isActive) return;

        // Draw the current radius as a wireframe sphere
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}
