using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public class MeshSocket : MonoBehaviour
    {
        public MeshSockets.SocketId socketId;
        public HumanBodyBones bone;

        public Vector3 offset;
        public Vector3 rotation;

        Transform attachPoint;

        // Start is called before the first frame update
        void Start()
        {
            /*
             attachPoint = transform.GetChild(0); 
              */
            Animator animator = GetComponentInParent<Animator>();
            attachPoint = new GameObject("Socket" + socketId).transform;
            attachPoint.SetParent(animator.GetBoneTransform(bone));
            attachPoint.localPosition = offset;
            attachPoint.localRotation = Quaternion.Euler(rotation);
        }

        public void Attach(Transform objectTransform)
        {
            objectTransform.SetParent(attachPoint, false);
        }
    }
}
