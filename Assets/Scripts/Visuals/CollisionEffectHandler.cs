using Assets.VehicleController;
using System;
using Unity.Netcode;
using UnityEngine;

public class CollisionEffectHandler : NetworkBehaviour
{
    [SerializeField]
    private CustomVehicleController _vehicleController;

    [SerializeField]
    private float _minCollisionSpeed = 10;

    public event Action<Vector3, float> OnCollision;

    public event Action<Vector3, float> OnLeftCollisionStay;
    public event Action OnLeftCollisionExit;

    public event Action<Vector3, float> OnRightCollisionStay;
    public event Action OnRightCollisionExit;

    [SerializeField]
    private AudioSource _audioSource;
    private float _speedForMaxVolume = 100;
    private float _maxPitchGain = 0.6f;
    private float _minVolume = 0.3f;
    private float _maxVolumeGain = 0.3f;

    private float _collisionStayTime = 0;
    private float _collsionStayForMaxEffect = 0.3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
            return;
        OnCollision?.Invoke(collision.GetContact(0).point, collision.relativeVelocity.magnitude);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsOwner)
            return;

        float mag = collision.relativeVelocity.magnitude;
        if (mag < _minCollisionSpeed)
        {
            StopAudio();
            return;
        }
        _collisionStayTime += Time.deltaTime;
        HandleAudio(mag);

        ContactPoint[] contactPoints = new ContactPoint[4];

        int collAmount = collision.GetContacts(contactPoints);

        int rightColls = 0;
        int leftColls = 0;

        for(int i  = 0; i < collAmount; i++)
        {
            ContactPoint contactPoint = contactPoints[i];

            Vector3 direction = contactPoint.point - transform.position;
            
            if(Vector3.Dot(transform.right, direction) > 0)
            {
                OnRightCollisionStay?.Invoke(contactPoint.point, mag);
                rightColls++;
            }else
            {
                OnLeftCollisionStay?.Invoke(contactPoint.point, mag);
                leftColls++;
            }
        }

        if(rightColls == 0)
            OnRightCollisionExit?.Invoke();

        if(leftColls == 0)
            OnLeftCollisionExit?.Invoke();
    }

    private void HandleAudio(float speed)
    {
        if (!_audioSource.isPlaying)
            _audioSource.Play();

        float effectStrength = Mathf.Clamp01(_collisionStayTime / _collsionStayForMaxEffect);

        _audioSource.volume = Mathf.Clamp01(_minVolume + Mathf.Clamp01(speed / _speedForMaxVolume) * _maxVolumeGain) * effectStrength;
        _audioSource.pitch = 1 + Mathf.Clamp01(speed / _speedForMaxVolume) * _maxPitchGain;
    }

    private void StopAudio()
    {
        _collisionStayTime = 0;
        _audioSource.Stop(); 
    }

    private void OnCollisionExit(Collision collision)
    {
        StopAudio();
        OnRightCollisionExit?.Invoke();
        OnLeftCollisionExit?.Invoke();
    }
}
