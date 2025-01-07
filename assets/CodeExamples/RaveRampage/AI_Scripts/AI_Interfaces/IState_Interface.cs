using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cyber_Slicer_AI
{
    public interface IState_Interface
    {
        void OnEnter();
        void Update();
        void FixedUpdate();
        void OnExit();
    }

   
}
