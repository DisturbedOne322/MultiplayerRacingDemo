using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsWingAeroEffect
    {
        private CurrentCarStats _currentCarStats;
        private WingAeroParameters _parameters;

        private int _size;

        public CarVisualsWingAeroEffect(WingAeroParameters parameters, CurrentCarStats currentCarStats)
        {
            _parameters = parameters;
            _size = _parameters.TrailRendererArray.Length;
            _currentCarStats = currentCarStats;

            if (parameters.TrailRendererArray.Length == 0)
                Debug.LogWarning("You have Wing Wind Effect, but TrailRenderer is not assigned");
        }
        
        public void HandleWingAeroEffect()
        {
            if (_currentCarStats.SpeedInMsPerS < _parameters.MinSpeedMStoDisplay)
            {
                for (int i = 0; i < _size; i++)
                {
                    _parameters.TrailRendererArray[i].emitting = false;
                }
                return;
            }

            for (int i = 0; i < _size; i++)
            {
                if (!_parameters.TrailRendererArray[i].emitting)
                    _parameters.TrailRendererArray[i].emitting = true;

                Color currentColor = _parameters.TrailRendererArray[i].startColor;
                currentColor.a = _parameters.MaxAlpha * _currentCarStats.SpeedPercent;
                _parameters.TrailRendererArray[i].startColor = currentColor;
            }
        }
    }
}
