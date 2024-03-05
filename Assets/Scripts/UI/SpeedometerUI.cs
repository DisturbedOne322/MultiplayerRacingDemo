using Assets.VehicleController;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerUI : NetworkBehaviour
{
    [SerializeField]
    private CustomVehicleController _vehicleController;

    [SerializeField]
    private TextMeshProUGUI _speedText;
    [SerializeField]
    private GameObject _rpmNeedle;
    private const float MAX_RPM_NEEDLE_ANGLE = -225f;

    [SerializeField]
    private GameObject _boostNeedle;
    private const float MAX_BOOST_NEEDLE_ANGLE = -270f;

    [SerializeField]
    private Slider _nitroSlider;
    [SerializeField]
    private TextMeshProUGUI _bootleLeftText;
    int _bottlesLeftLast = -1;

    [SerializeField]
    private TextMeshProUGUI _gearText;

    private StringBuilder _speedString;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _speedString = new StringBuilder();
            return;
        }

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCarStats currentCarStats = _vehicleController.GetCurrentCarStats();

        _speedString.Clear();
        _speedString.Append((int)Mathf.Abs(currentCarStats.SpeedInKMperH));
        _speedText.SetText(_speedString);

        _rpmNeedle.transform.localRotation = Quaternion.Euler(0, 0, MAX_RPM_NEEDLE_ANGLE * currentCarStats.EngineRPMPercent);

        _boostNeedle.transform.localRotation = Quaternion.Euler(0, 0, MAX_BOOST_NEEDLE_ANGLE * currentCarStats.ForcedInductionBoostPercent);

        _nitroSlider.value = currentCarStats.NitroPercentLeft;

        if(currentCarStats.NitroBottlesLeft != _bottlesLeftLast)
        {
            _bootleLeftText.text = currentCarStats.NitroBottlesLeft.ToString();
            _bottlesLeftLast = currentCarStats.NitroBottlesLeft;
        }

        _gearText.text = currentCarStats.CurrentGear;
    }
}
