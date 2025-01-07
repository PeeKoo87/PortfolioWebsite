using UnityEngine;
using Utilities;//testi
namespace Cyber_Slicer_AI
{
    public interface IDetectionStrategy 
    {
        bool Execute(Transform player, Transform detector, CountdownTimer timer);
    }
}