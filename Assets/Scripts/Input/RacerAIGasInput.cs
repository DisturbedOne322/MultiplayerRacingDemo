using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Input
{
    public class RacerAIGasInput
    {
        public float GetInput(HitInfo[] hitInfo, Vector3 forward, Vector3 averageForwardPath)
        {
            float velocityDifference = hitInfo[0].velocityDifference;

            float dot = Vector3.Dot(forward, averageForwardPath);

            //if (NeedReversing(hitInfo, dot))
            //    return -1;

            if (NoDangersAhead(hitInfo))
                return dot;

            if (hitInfo[0].hitDistance == 0)
                return 1;

            if (velocityDifference < 0)
                return 1 - Mathf.Clamp01(-velocityDifference * 5 / hitInfo[0].hitDistance);

            return Mathf.Clamp01(1 - (velocityDifference / hitInfo[0].hitDistance)) * dot;
        }

        private bool NeedReversing(HitInfo[] hitInfo, float dot)
        {
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].dotToControllerForward < 0.7f)
                    continue;
                if (!hitInfo[i].hit)
                    continue;

                if (hitInfo[i].hitDistance < 1 || dot < 0.1f)
                    return true;
            }
            return false;
        }

        private bool NoDangersAhead(HitInfo[] hitInfo)
        {
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].dotToControllerForward < 0.8f)
                    continue;
                if (hitInfo[i].hit)
                    return false;
            }
            return true;
        }
    }
}
