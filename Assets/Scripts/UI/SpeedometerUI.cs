using Assets.VehicleController;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerUI : MonoBehaviour
{
    [SerializeField]
    private CustomVehicleController _vehicleController;

    [SerializeField]
    private TextMeshProUGUI _speedText;
    [SerializeField]
    private GameObject _rpmNeedle;
    private const float MAX_RPM_NEEDLE_ANGLE = -204f;
    [SerializeField]
    private float _speedometerMaxRPM = 9000;

    [SerializeField]
    private float _engineMaxRPM = 9000;

    [SerializeField]
    private GameObject[] _redlinePer500RPMImageArray;

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

    private void Awake()
    {
        _speedString = new StringBuilder();

        float excessRPM = _speedometerMaxRPM - _engineMaxRPM;
        int excessive500RPMImages = ((int)excessRPM) / 500;
        for(int i = 0; i <  excessive500RPMImages; i++)
        {
            _redlinePer500RPMImageArray[i].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCarStats currentCarStats = _vehicleController.GetCurrentCarStats();

        _speedString.Clear();
        _speedString.Append((int)Mathf.Abs(currentCarStats.SpeedInKMperH));
        _speedText.SetText(_speedString);

        float speedoMeterRpmMultiplier = _engineMaxRPM / _speedometerMaxRPM;

        _rpmNeedle.transform.localRotation = Quaternion.Euler(0, 0, MAX_RPM_NEEDLE_ANGLE * currentCarStats.EngineRPMPercent * speedoMeterRpmMultiplier);

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
