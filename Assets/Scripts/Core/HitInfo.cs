using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HitInfo
    {
        public bool hit;
        public float hitDistance;
        public float interest;
        public Vector3 direction;
        public float rayLength;
        public float velocityDifference;
        public Vector3 hitVelocity;
        public float dotToControllerForward;

        public HitInfo()
        {
            hit = false;
            hitDistance = 0f;
            direction = new Vector3(0,0,0);
            interest = 0f;
            rayLength = 0;
            velocityDifference = 0;
            hitVelocity = Vector3.zero;
            dotToControllerForward = 0;
        }
    }
}

