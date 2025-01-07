using UnityEngine;
using Utilities; //testi

namespace Cyber_Slicer_AI
{
    public class ConeDetectionStrategy : IDetectionStrategy
    {
        readonly float detectionAngle;
        readonly float detectionRadius;
        readonly float innerDetectionRadius;
        public ConeDetectionStrategy(float detectionAngle,float detectionRadius,float innerDetectionRadius) 
        {
            this.detectionAngle = detectionAngle;
            this.detectionRadius = detectionRadius;
            this.innerDetectionRadius = innerDetectionRadius;
        }
        public bool Execute(Transform player, Transform detector, CountdownTimer timer)
        {
            if(timer.IsRunning) return false;

            var directionToPlayer = player.position - detector.position;
            var angleToPlayer = Vector3.Angle(directionToPlayer, detector.forward);

            // If the player is not within the detection angle + outer radius(cone in fron of enemy),
            // or is within the inner radius, return false
            if((!(angleToPlayer < detectionAngle / 2f) || !(directionToPlayer.magnitude < detectionRadius))
                && !(directionToPlayer.magnitude < innerDetectionRadius)) 
                return false;

            timer.Start();
            return true;
        }
    }
}