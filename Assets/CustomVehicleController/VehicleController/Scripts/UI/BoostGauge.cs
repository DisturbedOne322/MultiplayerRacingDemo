using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class BoostGauge : MonoBehaviour
    {
        [SerializeField]
        private CurrentCarStats _currentCarStats;

        [SerializeField]
        private GameObject _boostNeedle;

        [SerializeField]
        private Slider _nitroPercentSlider;


        private float _maxRotation = -267f;

        // Update is called once per frame
        void Update()
        {
            _boostNeedle.transform.rotation = Quaternion.Euler(0, 0, _maxRotation * _currentCarStats.ForcedInductionBoostPercent);
            _nitroPercentSlider.value = _currentCarStats.NitroPercentLeft;
        }
    }
}

