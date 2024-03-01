using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Input
{
    public class RacerAIBreakInput
    {
        public float GetInput(HitInfo[] hitInfo, Vector3 forward, Vector3 averageForwardPath)
        {
            (float velocityDifference, int id) = GetHighestDangerVelocityInFront(hitInfo);

            float dot = Vector3.Dot(forward, averageForwardPath);

            //if (NeedsReversing(dot))
            //    return 0;

            float aheadDanger = 1 - dot;

            if (hitInfo[0].hitDistance == 0)
            {
                return aheadDanger * 2;
            }

            if (velocityDifference < 0)
            {
                return Mathf.Clamp01(-velocityDifference * 5 / hitInfo[id].hitDistance) * aheadDanger;
            }

            return Mathf.Clamp01(velocityDifference / hitInfo[id].hitDistance) * aheadDanger;
        }

        private (float,int) GetHighestDangerVelocityInFront(HitInfo[] hitInfo)
        {
            float highestDanger = 0;
            int id = 0;
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].dotToControllerForward < 0.8f)
                    continue;
                if (!hitInfo[i].hit)
                    continue;
                if (hitInfo[i].velocityDifference > highestDanger)
                {
                    highestDanger = hitInfo[i].velocityDifference;
                    id = i;
                }
            }
            return (highestDanger, id);
        }

        private bool NeedsReversing(float dot)
        {
            Debug.Log("4 " + dot);
            return dot < 0.5f;
        }
    }
}
