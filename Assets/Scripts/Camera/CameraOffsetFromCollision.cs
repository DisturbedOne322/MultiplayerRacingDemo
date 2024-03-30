using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOffsetFromCollision : MonoBehaviour
{
    [SerializeField]
    private CameraSpeedEffects _camera;

    private float _offset = 0;
    private float _effectSpeed = 3;
    private float _lastCollTime = 0;
    private float _delay = 0.33f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ignore"))
            return;

        _lastCollTime = Time.time;
    }


    private void Update()
    {
        if(_lastCollTime + _delay > Time.time)
        {
            _offset += Time.deltaTime * _effectSpeed;
        }
        else
            _offset -= Time.deltaTime * _effectSpeed;

        _offset = Mathf.Clamp(_offset, 0, - _camera.Zoffset / 2);

        _camera.SetOffsetFromCollider(_offset);
    }
}
