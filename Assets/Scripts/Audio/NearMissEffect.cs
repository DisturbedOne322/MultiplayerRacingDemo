using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearMissEffect : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField]
    private AudioSource _audioSource;

    float _maxDistance;

    [SerializeField]
    private float _minSpeedDifference = 50;
    private float _minSpeedSqr;
    [SerializeField]
    private float _maxSpeedDifference = 200;
    private float _maxSpeedSqr;

    private float _pitchRandomOffset = 0.3f;

    private void Start()
    {
        _rb = transform.root.GetComponent<Rigidbody>();
        _maxDistance = GetComponent<BoxCollider>().size.x / 2;
        _minSpeedSqr = _minSpeedDifference * _minSpeedDifference;
        _maxSpeedSqr = _maxSpeedDifference * _maxSpeedDifference;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody otherRB = other.transform.root.GetComponent<Rigidbody>();

        if (otherRB == null)
            return;

        Vector3 velocityDifference = _rb.linearVelocity - otherRB.linearVelocity;
        float sqrMagDifference = velocityDifference.sqrMagnitude;

        if (sqrMagDifference < _minSpeedSqr)
            return;

        if(sqrMagDifference > _maxSpeedSqr)
            sqrMagDifference = _maxSpeedSqr;

        float volume = (sqrMagDifference - _minSpeedSqr) / (_maxSpeedSqr - _minSpeedSqr);

        _audioSource.volume = volume;
        _audioSource.pitch = volume + 1 + Random.Range(-_pitchRandomOffset, _pitchRandomOffset);

        float pan = Vector3.Dot(transform.right, other.transform.position - transform.position);

        float volumeMultiplier = 1 - ((Vector3.Distance(other.transform.position, transform.position) / _maxDistance) / 2);

        if (pan > 0)
            pan = 1;
        else
            pan = -1;

        _audioSource.panStereo = pan;

        _audioSource.PlayOneShot(_audioSource.clip, volumeMultiplier);
    }
}
