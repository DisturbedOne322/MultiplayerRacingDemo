using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficCar : NetworkBehaviour
{
    private float _multiplier = 0.0001f;

    [SerializeField]
    private SplineAnimate _splineAnimate;
    [SerializeField]
    private Rigidbody _rb;

    [SerializeField]    
    private LayerMask _trafficLayer;

    private bool _collided = false;
    private bool _accelerate = true;

    private float _maxSpeed;

    private const float MAX_RAY_DIST = 20;
    private WaitForSeconds _raycastDelay = new WaitForSeconds(0.33f);

    public void Initialize(SplineContainer container, float speed, float offset)
    {
        _splineAnimate.Container = container;

        _splineAnimate.ObjectUpAxis = SplineComponent.AlignAxis.ZAxis;
        _splineAnimate.ObjectForwardAxis = SplineComponent.AlignAxis.NegativeYAxis;

        _splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        _maxSpeed = speed;
        _splineAnimate.MaxSpeed = 1;

        _splineAnimate.StartOffset = offset;
        _rb.Sleep();

        StartCoroutine(CheckForFutureCollisions(offset));
    }

    private IEnumerator CheckForFutureCollisions(float delay) 
    {
        yield return new WaitForSeconds(delay);
        while (true)
        {
            if (!_collided)
            {
                if (Physics.Raycast(transform.position + Vector3.up, -transform.up, MAX_RAY_DIST, _trafficLayer))
                    _accelerate = false;
                else
                    _accelerate = true;
            }

            yield return _raycastDelay;
        }
    }

    private void Update()
    {
        if (_collided)
            return;

        float prevProgress = _splineAnimate.NormalizedTime;

        if (_accelerate)
        {
            if (!_splineAnimate.IsPlaying)
                _splineAnimate.Play();

            if (_splineAnimate.MaxSpeed < _maxSpeed)
            {
                _splineAnimate.MaxSpeed += _maxSpeed * Time.deltaTime / 5;
                _splineAnimate.NormalizedTime = prevProgress;
            }
        }   
        else
        {
            if (_splineAnimate.MaxSpeed > 1)
            {
                _splineAnimate.MaxSpeed -= _splineAnimate.MaxSpeed * Time.deltaTime * 4;
                _splineAnimate.NormalizedTime = prevProgress;
            }
            else
                _splineAnimate.Pause();
        }


    }

    public void ResetTrafficCar(SplineContainer newContainer, float newOffset)
    {
        _rb.Sleep();

        _splineAnimate.Container = newContainer;

        _splineAnimate.StartOffset = newOffset;
        _splineAnimate.Restart(true);

        _splineAnimate.Play();
        _collided = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.CompareTag("Traffic"))
                OnCollisionServerRPC(collision.relativeVelocity);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            OnCollisionServerRPC(collision.relativeVelocity);
            OnCollisionWithPlayerServerRPC(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, collision.relativeVelocity);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnCollisionServerRPC(Vector3 relativeVel)
    {
        _collided = true;
        _splineAnimate.Pause();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnCollisionWithPlayerServerRPC(ulong collidedClientID, Vector3 relativeVel)
    {
        ReactToCollisionClientRPC(collidedClientID, relativeVel);
    }

    [ClientRpc]
    private void ReactToCollisionClientRPC(ulong testID, Vector3 relativeVel)
    {
        if(NetworkManager.Singleton.LocalClientId == testID)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Rigidbody>().AddForce(relativeVel * _multiplier, ForceMode.Impulse);
        }
    }
}
