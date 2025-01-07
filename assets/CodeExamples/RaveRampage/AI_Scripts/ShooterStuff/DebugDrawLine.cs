using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public class DebugDrawLine : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Debug.DrawLine(transform.position,transform.position + transform.forward * 50);
        }
    }
}
