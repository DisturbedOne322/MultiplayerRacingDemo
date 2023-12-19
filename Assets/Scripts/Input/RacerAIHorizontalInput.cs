using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Input
{
    public class RacerAIHorizontalInput
    {
        private float[] _dangerArray;
        private float[] _interestArray;
        private float[] _velocityDifferenceArray;

        private Vector3 _result;

        private GameObject _target;

        private readonly float _minimumDistance = 2;
        public float GetHorizontalInput(HitInfo[] hitInfo, float speed,
            Vector3 localForward, Vector3 localRight, Vector3 averageForwardPath, GameObject racer)
        {
            _target = racer;
            return CalculateTurnAmount(hitInfo, speed, localForward, localRight, averageForwardPath);       
        }

        public void Initialize(int arraySize)
        {
            _result = new(0f,0f,0f);

            _dangerArray = new float[arraySize];
            _interestArray = new float[arraySize];
            _velocityDifferenceArray = new float[arraySize];
        }

        private float CalculateTurnAmount(HitInfo[] hitInfo, float speed, Vector3 localForward, Vector3 localRight, Vector3 averageForwardPath)
        {
            float rightDot = Vector3.Dot(localRight, averageForwardPath);
            bool right = rightDot < 0;
            if (Vector3.Dot(localForward, averageForwardPath) < 0.5f)
                return ReturnToTrackBehaviour(localForward, averageForwardPath, rightDot);


            return Vector3.Dot(localRight,
                GetClosestDirectionToAvoidObstacle(hitInfo, localForward, averageForwardPath, right));
        }

        public Vector3 GetClosestDirectionToAvoidObstacle(HitInfo[] hitInfo, Vector3 localForward, Vector3 averageForwardPath, bool right)
        {
            CalculateInterestArray(hitInfo, averageForwardPath);
            CalculateDangerArray(hitInfo, localForward, averageForwardPath, right);
            Vector3 result = MakeDecision(hitInfo);
            return result;
        }

        public void CalculateDangerArray(HitInfo[] hitInfo,Vector3 localForward, Vector3 directionNormalized, bool right)
        {
            //ReturnToTrackBehaviour(_controller.transform.right, directionNormalized);
            /*
              if object is moving
                if object is faster
                    danger = 0
                else
                    danger = difference in speed + distance or just distance
              else //object is still
                if object is in the path
                    maximum danger
                else
                    danger = distance to object * dot product to the object
             */
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].hit)
                    _dangerArray[i] = 1;
                else
                    _dangerArray[i] = 0;
                return;


                _velocityDifferenceArray[i] = hitInfo[i].velocityDifference;

                if (hitInfo[i].hitDistance == 0)
                {
                    _dangerArray[i] = 0;
                    continue;
                }

                if (hitInfo[i].hitDistance < _minimumDistance)
                {
                    _dangerArray[i] = 1;
                    continue;
                }

                float velocityDifference = hitInfo[i].velocityDifference;   

                if (hitInfo[i].hitVelocity == Vector3.zero)
                {
                    _dangerArray[i] = EvadeStaticObstacleBehaviour(hitInfo, localForward, i);
                }
                else
                {
                    _dangerArray[i] = EvadeVehicleBehaviour(hitInfo, localForward, i);
                }
            }

           //EvadeHeadOnCollisionBehaviour(right);
        }

        public void CalculateInterestArray(HitInfo[] hitInfo, Vector3 directionNormalized)
        {
            for (int i = 0; i < hitInfo.Length; i++)
            {
                _interestArray[i] = Vector3.Dot(hitInfo[i].direction, directionNormalized);
            }
        }
        private float EvadeStaticObstacleBehaviour(HitInfo[] hitInfo, Vector3 localForward, int id)
        {
            return Vector3.Dot(hitInfo[id].direction, localForward)
                   * (hitInfo[id].hitDistance * 2 / hitInfo[id].rayLength);
        }

        private float EvadeVehicleBehaviour(HitInfo[] hitInfo, Vector3 localForward, int id)
        {
            if (hitInfo[id].velocityDifference > 0)
                return 0;
            return Vector3.Dot(localForward, hitInfo[id].direction) * 
                Mathf.Clamp01(-hitInfo[id].velocityDifference * 10 / hitInfo[id].hitDistance);
        }

        private float ReturnToTrackBehaviour(Vector3 controllerForward, Vector3 forwardPath, float dot)
        {
            if(Vector3.Dot(controllerForward, forwardPath) > 0)
                return dot > 0 ? 1 : -1;
            return dot > 0 ? -1: 1;
        }

        private void EvadeHeadOnCollisionBehaviour(bool right)
        {

            (int rightDangers, float rightDangerSum) = CalculateHeadOnCollisionToTheRight();
            (int leftDangers, float leftDangerSum) = CalculateHeadOnCollisionToTheLeft();

            if (leftDangers == rightDangers && rightDangers == 0)
                return;

            if (leftDangers != rightDangers)
                return;

            float lowestDanger = 1;
            int lowestDangerId;
            
            if(right)
            {
                for (int i = 1; i <= _dangerArray.Length / 2; i++)
                {
                    _dangerArray[i] = 1;
                }
            }
            else
            {
                for (int i = _dangerArray.Length / 2 + 1; i < _dangerArray.Length; i++)
                {
                    _dangerArray[i] = 1;
                }
            }
            return;
            if (rightDangerSum > leftDangerSum)
            {
                for (int i = 1; i <= _dangerArray.Length / 2; i++)
                {
                    _dangerArray[i] = 1;
                }
            }
            else
            {
                for (int i = _dangerArray.Length / 2 + 1; i < _dangerArray.Length; i++)
                {
                    _dangerArray[i] = 1;
                }
            }
            
        }

        private (int,float) CalculateHeadOnCollisionToTheRight()
        {
            float sumOfDangerToTheRight = 0;
            int dangersTotal = 0;
            for (int i = 1; i <= _dangerArray.Length / 2; i++)
            {
                if (_dangerArray[i] > 0)
                    dangersTotal++;
                sumOfDangerToTheRight += _dangerArray[i];
            }

            return (dangersTotal, sumOfDangerToTheRight);
        }
        private (int, float) CalculateHeadOnCollisionToTheLeft()
        {
            float sumOfDangerToTheLeft = 0;
            int dangersTotal = 0;

            for (int i = _dangerArray.Length / 2 + 1; i < _dangerArray.Length; i++)
            {
                if (_dangerArray[i] > 0)
                    dangersTotal++;
                sumOfDangerToTheLeft += _dangerArray[i];
            }
            return (dangersTotal, sumOfDangerToTheLeft);
        }

        private float[] _resultArray;
        public Vector3 MakeDecision(HitInfo[] hitInfo)
        {
            _result = Vector3.zero;
            _resultArray = new float[hitInfo.Length];

            //works but looks like shit
            //if (_dangerArray[0] > 0)
            //{
            //    float rightDanger = CalculateHeadOnCollisionToTheRight();
            //    float leftDanger = CalculateHeadOnCollisionToTheLeft();

            //    if (rightDanger > leftDanger)
            //    {
            //        for (int i = _dangerArray.Length / 2 + 1; i < _dangerArray.Length; i++)
            //        {
            //            _result += hitInfo[i].direction * (_interestArray[i] - _dangerArray[i]);
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 1; i <= _dangerArray.Length / 2; i++)
            //        {
            //            _result += hitInfo[i].direction * (_interestArray[i] - _dangerArray[i]);
            //        }
            //    }
            //}

            //for (int i = 0; i < _dangerArray.Length; i++)
            //{
            //    _result += hitInfo[i].direction * (_interestArray[i] - _dangerArray[i]);
            //    _resultArray[i] = (_interestArray[i] - _dangerArray[i]);
            //}

            for (int i = 0; i < _dangerArray.Length; i++)
            {
                if (_dangerArray[i] == 0)
                {
                    _result += hitInfo[i].direction * (_interestArray[i]);
                }
                //_resultArray[i] = (_interestArray[i] - _dangerArray[i]);
            }
            Debug.DrawRay(_target.transform.position, _result.normalized * 100);

            return _result.normalized;
        }

        public float[] GetDangerArray()
        {
            return _dangerArray;
        }

        public float[] GetInterestArray()
        {
            return _interestArray;
        }

        public float[] GetVelocityArray()
        {
            return _velocityDifferenceArray;
        }

        public float[] GetResultArray()
        {
            return _resultArray;
        }
    }
}
