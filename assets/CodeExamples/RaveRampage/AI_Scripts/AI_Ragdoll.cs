using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Ragdoll : MonoBehaviour
{
    Animator animator;
    Rigidbody[] rigidBodies;
    // Start is called before the first frame update
    void Start()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        foreach (var rigidBody in rigidBodies)
        {
            rigidBody.isKinematic = true;
        }
        animator.enabled = true;
    }

    public void ActivateRagdoll()
    {
        foreach (var rigidBody in rigidBodies)
        {
            rigidBody.isKinematic = false;
        }
        animator.enabled = false;
    }

    public void ApplyForce(Vector3 force)
    {
        var rigidBody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        rigidBody.AddForce(force, ForceMode.VelocityChange);
    }
}

