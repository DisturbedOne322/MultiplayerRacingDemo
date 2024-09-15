using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEditor;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode()]
    public class RaycastSensor : MonoBehaviour
    {
        private Rigidbody _rb;

        [SerializeField] private int _raycastAmount = 5;

        public int RayCastAmount
        {
            get { return _raycastAmount; }
        }

        [SerializeField] private float _maxAngleBetweenRays = 20f;
        [SerializeField] private float _rayDistance;
        [SerializeField] private Vector3 _offset;


        [SerializeField] private LayerMask _collisionLayer;

        [SerializeField] private Color _hitColor;

        [SerializeField] private float _radius;
        [SerializeField] private float _closeRadius = 4.5f;

        private HitInfo[] _hitInfoArray;
        private float[] _rayAngleArray;

        [SerializeField] private float _maxHorizontalDistance = 10;

        private RaycastHit[] raycastHitResults;
        private Collider[] _closeColliderArray;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            InitializeHitInfoArray();
        }

        private void InitializeHitInfoArray()
        {
            _hitInfoArray = new HitInfo[_raycastAmount];
            _rayAngleArray = new float[_raycastAmount];
            raycastHitResults = new RaycastHit[_raycastAmount];
            _closeColliderArray = new Collider[_raycastAmount];

            for (int i = 0; i < _hitInfoArray.Length; i++)
            {
                _hitInfoArray[i] = new HitInfo();
                if (i <= _raycastAmount / 2)
                {
                    _rayAngleArray[i] = i * _maxAngleBetweenRays;
                }
                else
                {
                    _rayAngleArray[i] = (i - _raycastAmount / 2) * -_maxAngleBetweenRays;
                }

                float cLen = _rayDistance;
                float aLen = cLen * Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                if (aLen > _maxHorizontalDistance)
                {
                    aLen = _maxHorizontalDistance;
                    cLen = aLen / Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                }

                _hitInfoArray[i].rayLength = cLen;
                _hitInfoArray[i].dotToControllerForward = Vector3.Dot(transform.forward, Quaternion.Euler(0, _rayAngleArray[i], 0) * transform.forward);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //if(EditorApplication.isPlaying) 
            //    return;
            //InitializeHitInfoArray();

            //for (int i = 0; i < _hitInfoArray.Length; i++)
            //{
            //    _hitInfoArray[i].direction = Quaternion.Euler(0, _rayAngleArray[i], 0) * transform.forward;
            //    Gizmos.DrawWireSphere(transform.position + _offset + _hitInfoArray[i].direction * _hitInfoArray[i].rayLength, _radius);
            //    Debug.DrawRay(transform.position + _offset, _hitInfoArray[i].direction * _hitInfoArray[i].rayLength);
            //}
        }

#endif
        public HitInfo[] GetRaycastHitInfoArray()
        {
            if(_hitInfoArray == null)
                InitializeHitInfoArray();
            for (int i = 0; i < _raycastAmount; i++)
            {
                _hitInfoArray[i].direction = Quaternion.Euler(0, _rayAngleArray[i], 0) * transform.forward;

                _hitInfoArray[i].hit = Physics.SphereCast(transform.position + _offset, _radius,
                     _hitInfoArray[i].direction, out raycastHitResults[i], _hitInfoArray[i].rayLength, 
                    _collisionLayer);
                

                _hitInfoArray[i].hitDistance = raycastHitResults[i].distance;
                if (_hitInfoArray[i].hit)
                {
                    if (raycastHitResults[i].collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody hitRB))
                    {
                        Vector3 velocityDif = hitRB.velocity - _rb.velocity;
                        _hitInfoArray[i].velocityDifference = velocityDif.x * transform.forward.x
                                                              + velocityDif.z *
                                                              transform.forward.z;
                        
                        _hitInfoArray[i].hitVelocity = hitRB.velocity;
                    }
                    else
                    {
                        _hitInfoArray[i].velocityDifference = _rb.velocity.x * transform.forward.x + _rb.velocity.z * transform.forward.z;
                        _hitInfoArray[i].hitVelocity = Vector3.zero;
                    }
                }
                else
                {
                    _hitInfoArray[i].velocityDifference = _rb.velocity.x * transform.forward.x + _rb.velocity.z * transform.forward.z;
                    _hitInfoArray[i].hitVelocity = Vector3.zero;
                }
            }
            for (int i = 0; i < _raycastAmount; i++)
            {

                Debug.DrawRay(transform.position + _offset,
                    _hitInfoArray[i].direction * _hitInfoArray[i].rayLength,
                    _hitInfoArray[i].hit ? Color.red : Color.blue);
            }
            // GetCloseCollisions();
            return _hitInfoArray;
        }

        private void GetCloseCollisions()
        {
            int collisions = Physics.OverlapSphereNonAlloc(transform.position + _offset, _closeRadius,
                _closeColliderArray, _collisionLayer);

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;    
            Vector3 pos = transform.position;

            for (int i = 0; i < collisions; i++)
            {
                if (_closeColliderArray[i].transform.position == pos)
                    continue;

                Vector3 dir = (_closeColliderArray[i].transform.position - pos).normalized;
                float dot = Vector3.Dot(dir, forward);
                float side = Vector3.Dot(dir, right);

                int closestRayID = FindRayWithClosestDot(dot);

                if (side >= 0)
                {
                    FillClosedCollisions(closestRayID, i);
                }
                else
                {
                    FillClosedCollisions(closestRayID + _raycastAmount / 2 + 1, i);
                }
            }
        }

        private int FindRayWithClosestDot(float dot)
        {
            int id = 0;
            float closest = _hitInfoArray[0].dotToControllerForward;
            float difference = Mathf.Abs(dot - closest);
            for (int i = 1; i < _hitInfoArray.Length / 2; i++)
            {
                float currentDifference = Mathf.Abs(dot - _hitInfoArray[i].dotToControllerForward);
                if (currentDifference < difference)
                {
                    id = i;
                    difference = currentDifference;
                }
            }

            return id;
        }

        private void FillClosedCollisions(int rayID, int colID)
        {
            _hitInfoArray[rayID].hit = true;

            _hitInfoArray[rayID].hitDistance = Vector3.Distance(transform.position, _closeColliderArray[colID].transform.position);

            if (_closeColliderArray[colID].gameObject.TryGetComponent<Rigidbody>(out Rigidbody hitRB))
            {
                Vector3 velocityDif = hitRB.velocity - _rb.velocity;
                _hitInfoArray[rayID].velocityDifference = velocityDif.x * transform.forward.x
                                                      + velocityDif.z *
                                                      transform.forward.z;
                _hitInfoArray[rayID].hitVelocity = hitRB.velocity;
            }
            else
            {
                _hitInfoArray[rayID].velocityDifference = _rb.velocity.x * transform.forward.x + _rb.velocity.z * transform.forward.z;
                _hitInfoArray[rayID].hitVelocity = Vector3.zero;
            }
        }
    }
}
