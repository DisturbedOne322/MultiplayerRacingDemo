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
    private float _minSize = 150;
    [SerializeField]
    private float _maxSize = 350;

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

        float size = _minSize + _vehicleController.GetCurrentCarStats().SpeedInMsPerS * 2;
        if(size > _maxSize)
            size = _maxSize;

        _miniMapCamera.orthographicSize = size;
    }
}
