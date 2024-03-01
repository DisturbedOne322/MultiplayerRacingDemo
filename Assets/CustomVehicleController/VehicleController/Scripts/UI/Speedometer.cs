using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        private Text _speedText;
        [SerializeField]
        private Slider _rpmSlider;

        [SerializeField]
        private Text _currentGearText;
        [SerializeField]
        private Text _nitroBottlesLeft;

        [SerializeField]
        private Slider _nitroSlider;
        [SerializeField]
        private Slider _boostSlider;


        private void Update()
        {
            _currentGearText.text = _vehicleController.GetCurrentCarStats().CurrentGear;
            _nitroBottlesLeft.text = _vehicleController.GetCurrentCarStats().NitroBottlesLeft.ToString();

            _speedText.text = ((int)Mathf.Abs(_vehicleController.GetCurrentCarStats().SpeedInKMperH)).ToString();
            _nitroSlider.value = _vehicleController.GetCurrentCarStats().NitroPercentLeft;
            _rpmSlider.value = _vehicleController.GetCurrentCarStats().EngineRPMPercent;
            _boostSlider.value = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent;
        }

    }
}