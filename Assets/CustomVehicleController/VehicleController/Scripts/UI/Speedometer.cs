using TMPro;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        private CurrentCarStats _currentCarStats;
        [SerializeField]
        private TextMeshProUGUI _speedText;
        [SerializeField]
        private GameObject _rpmNeedle;

        [SerializeField]
        private TextMeshProUGUI _currentGearText;

        private const float NEEDLE_ANGLE_MAX = -225f;

        private void Update()
        {
            _currentGearText.text = _currentCarStats.CurrentGear;

            _speedText.text = ((int)Mathf.Abs(_currentCarStats.SpeedInKMperH)).ToString();
            float angle = _currentCarStats.EngineRPM / 9000 * NEEDLE_ANGLE_MAX;

            _rpmNeedle.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }

    }
}

