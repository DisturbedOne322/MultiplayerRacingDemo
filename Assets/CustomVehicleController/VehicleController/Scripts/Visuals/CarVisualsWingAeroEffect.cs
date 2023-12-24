using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Wing Aero Effect")]
    public class CarVisualsWingAeroEffect : MonoBehaviour
    {
        [SerializeField]
        private CurrentCarStats _currentCarStats;

        [SerializeField]
        private TrailRenderer[] _trailRenderers;
        private int _size;

        [SerializeField]
        private int _minSpeedMStoDisplay = 20;

        [SerializeField, Range(0,1f)]
        private float _maxAlpha = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            _size = _trailRenderers.Length;
        }

        // Update is called once per frame
        void Update()
        {
            if(_currentCarStats.SpeedInMsPerS < _minSpeedMStoDisplay)
            {
                for(int i = 0; i < _size; i++)
                {
                    _trailRenderers[i].emitting = false;
                }
                return;
            }

            for (int i = 0; i < _size; i++)
            {
                if (!_trailRenderers[i].emitting)
                    _trailRenderers[i].emitting = true;

                Color currentColor = _trailRenderers[i].startColor;
                currentColor.a = _maxAlpha *  _currentCarStats.SpeedPercent;
                _trailRenderers[i].startColor = currentColor;
                
            }
        }
    }
}
