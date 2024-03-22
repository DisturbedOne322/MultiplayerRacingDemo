using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOffsetFromCollision : MonoBehaviour
{
    [SerializeField]
    private CameraSpeedEffects _camera;

    private float _offset = 0;
    private float _effectSpeed = 3;
    private bool _obstacleDetected = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ignore"))
            return;

        _obstacleDetected = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _obstacleDetected = false;
    }

    private void Update()
    {
        if(_obstacleDetected)
        {
            _offset += Time.deltaTime * _effectSpeed;
        }
        else
            _offset -= Time.deltaTime * _effectSpeed;

        _offset = Mathf.Clamp(_offset, 0, - _camera.Zoffset / 2);

        _camera.SetOffsetFromCollider(_offset);
    }
}
