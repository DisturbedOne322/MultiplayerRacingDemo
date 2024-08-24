using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficCar : NetworkBehaviour
{
    private float _multiplier = 0.0001f;

    [SerializeField]
    private Transform[] _wheelsArray;
    [SerializeField]    
    private NetworkTransform _networkTransform;

    private SplineAnimate _splineAnimate;
    public SplineContainer Container;
    [SerializeField]
    private Rigidbody _rb;

    [SerializeField]    
    private LayerMask _trafficLayer;

    private bool _collided = false;
    private bool _accelerate = true;

    private float _maxSpeed;

    private NetworkVariable<float> _speedNetVar = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private const float MAX_RAY_DIST = 30;
    private WaitForSeconds _raycastDelay = new WaitForSeconds(0.25f);

    public void Initialize(SplineContainer container, float speed, float offset)
    {
        _collided = false;
        _splineAnimate = gameObject.AddComponent<SplineAnimate>();
        Container = container;
        _splineAnimate.Container = Container;

        _splineAnimate.ObjectUpAxis = SplineComponent.AlignAxis.ZAxis;
        _splineAnimate.ObjectForwardAxis = SplineComponent.AlignAxis.NegativeYAxis;

        _splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        _maxSpeed = speed;
        _splineAnimate.MaxSpeed = 1;
        _splineAnimate.StartOffset = offset;
        _splineAnimate.Play();
        _splineAnimate.Update();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(CheckForFutureCollisions(5));
    }

    private IEnumerator CheckForFutureCollisions(float delay) 
    {
        yield return new WaitForSeconds(delay);
        while (true)
        {
            if (!_collided)
                TestForFutureCollisions();

            yield return _raycastDelay;
        }
    }

    private void TestForFutureCollisions()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up, 2, -transform.up, MAX_RAY_DIST, _trafficLayer);
        if (hits.Length > 0)
            _accelerate = !IsCollisionDangerous(hits);
        else
            _accelerate = true;
    }


    private bool IsCollisionDangerous(RaycastHit[] hits)
    {
        for(int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.root == transform.root)
                continue;

            if (hits[i].collider.transform.root.TryGetComponent<TrafficCar>(out TrafficCar otherCar))
            {
                if (otherCar.Container == this.Container)
                    return true;
            }
            else
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (_collided)
            return;

        for (int i = 0; i < _wheelsArray.Length; i++)
            _wheelsArray[i].rotation *= Quaternion.Euler(_speedNetVar.Value, 0, 0);

        if (!IsServer)
            return;

        _speedNetVar.Value = _splineAnimate.MaxSpeed;
        float prevProgress = _splineAnimate.NormalizedTime;

        if (_accelerate)
        {
            if (!_splineAnimate.IsPlaying)
                _splineAnimate.Play();

            if (_splineAnimate.MaxSpeed < _maxSpeed)
            {
                _splineAnimate.MaxSpeed += _maxSpeed * Time.deltaTime / 6;
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
            {
                _splineAnimate.MaxSpeed = 0;
                _splineAnimate.Pause();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetInterpolationClientRpc(RigidbodyInterpolation value)
    {
        _rb.interpolation = value;
        _networkTransform.Interpolate = value == RigidbodyInterpolation.Interpolate;
    }

    public void ResetTrafficCar(SplineContainer newContainer, float newOffset)
    {
        _collided  = false;

        Container = newContainer;
        _splineAnimate.enabled = true;
        _splineAnimate.StartOffset = newOffset;
        _splineAnimate.Restart(true);

        _splineAnimate.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Traffic") || collision.gameObject.CompareTag("Player"))
        {
            OnAnyCollisionServerRpc();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 direction = (collision.gameObject.transform.position - transform.position).normalized;
            ResolveCollisionServerRpc(-direction * collision.relativeVelocity.magnitude * 1.5f);    
        }
    }
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void OnAnyCollisionServerRpc()
    {
        _splineAnimate.Pause();
        _splineAnimate.enabled = false;
        OnAnyCollisionClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnAnyCollisionClientRpc()
    {
        _collided = true;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ResolveCollisionServerRpc(Vector3 impulse)
    {
        ResolveCollisionClientRpc(impulse);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResolveCollisionClientRpc(Vector3 impulse)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _rb.linearVelocity = impulse;
        }
    }
}
