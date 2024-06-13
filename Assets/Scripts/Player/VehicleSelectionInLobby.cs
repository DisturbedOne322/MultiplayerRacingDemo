using System;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class VehicleSelectionInLobby : MonoBehaviour
{
    public static event Action<int> OnVehicleSelectionChanged;
    [SerializeField]
    private Transform _rotatePoint;
    [SerializeField]
    private float _rotateSpeed;

    private int _vehicleIndex = 0;

    public int SelectedVehicleIndex => _vehicleIndex;

    [SerializeField]
    private Button _nextButton;

    [SerializeField]
    private GameObject[] _vehicleModels;

    private void Awake()
    {
        _vehicleIndex = 0;
        OnVehicleSelectionChanged?.Invoke(_vehicleIndex);
        _nextButton.onClick.AddListener(() => {

            if (Lobby.Instance.CheckIsPlayerReady(AuthenticationService.Instance.PlayerId))
                return;

            _vehicleModels[_vehicleIndex].SetActive(false);
            _vehicleIndex++;

            if(_vehicleIndex >= _vehicleModels.Length)
                _vehicleIndex = 0;

            _vehicleModels[_vehicleIndex].SetActive(true);
            OnVehicleSelectionChanged?.Invoke(_vehicleIndex);
        });
    }

    // Update is called once per frame
    void Update()
    {
        _rotatePoint.Rotate(Vector3.up * _rotateSpeed * Time.deltaTime);
    }
}
