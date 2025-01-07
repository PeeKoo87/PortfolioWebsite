using Cyber_Slicer_AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDistance = 90f;

    [SerializeField] LayerMask ignoreMask;
    [SerializeField] private UnityEvent OnHitTarget;

    private RaycastHit rayHit;
    private Ray ray;

    private bool canSeeTarget = false;

    private void Start()
    {
        lineRenderer.positionCount = 2;
    }
    private void Update()
    {
        ray = new(transform.position, transform.forward);
        if(Physics.Raycast(ray,out rayHit, laserDistance, ~ignoreMask))
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, rayHit.point);
            //if(rayHit.collider.TryGetComponent(out Target target))
            //{
            //    target.Hit();
            //    OnHitTarget?.Invoke();
            //}
            //Debug.Log(rayHit.collider.name);
            if (rayHit.collider.name == "Character") 
            { 
                //Debug.Log("Target Found!");
                canSeeTarget = true;
            }
            else { /*Debug.Log("Target not found");*/ canSeeTarget = false; }
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * laserDistance);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, ray.direction * laserDistance);

        Gizmos.color =Color.blue;
        Gizmos.DrawWireSphere(rayHit.point,0.23f);
    }
    public bool IsTargetInSight()
    {
        return canSeeTarget;
    }
    public void EnableLineTrace()
    {
        lineRenderer.enabled = true;
    }

    public void DisableLineTrace()
    {
        lineRenderer.enabled = false;
        
    }
}
