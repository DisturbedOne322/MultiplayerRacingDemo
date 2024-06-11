using Assets.VehicleController;
using Unity.Netcode;
using UnityEngine;

public class Minimap : NetworkBehaviour
{
    [SerializeField]
    private Camera _miniMapCamera; 

    [SerializeField]
    private CustomVehicleController _vehicleController;
    private Transform _followTransform;

    [SerializeField]
    private Transform _playerIcon;

    [SerializeField]
    private float _minSize = 200;
    [SerializeField]
    private float _maxSize = 600;

    [SerializeField]
    private float _verticalOffset = 100;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
         
        transform.parent = null;
        _followTransform = _vehicleController.transform;
    }

    void Update()
    {
        if (!IsSpawned)
            return;

        transform.position = _followTransform.position + new Vector3(0, _verticalOffset, 0);
        _playerIcon.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(_followTransform.forward, Vector3.forward, Vector3.up) + 90);

        float size = _minSize + Mathf.Abs(_vehicleController.GetCurrentCarStats().SpeedInMsPerS) * 4;
        if(size > _maxSize)
            size = _maxSize;

        _miniMapCamera.orthographicSize = size;
    }
}
