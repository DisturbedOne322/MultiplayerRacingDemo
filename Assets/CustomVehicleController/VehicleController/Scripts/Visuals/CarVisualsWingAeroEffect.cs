using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsWingAeroEffect
    {
        //private CurrentCarStats _currentCarStats;
        private WingAeroParameters _parameters;

        private int _size;

        public CarVisualsWingAeroEffect(WingAeroParameters parameters)
        {
            _parameters = parameters;
            _size = _parameters.TrailRendererArray.Length;

            if (parameters.TrailRendererArray.Length == 0)
                Debug.LogWarning("You have Wing Wind Effect, but TrailRenderer is not assigned");
        }

        public void HandleWingAeroEffect(float speed, float speedPercent)
        {
            if (speed < _parameters.MinSpeedToDisplay)
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
                currentColor.a = _parameters.MaxAlpha * ((speed - _parameters.MinSpeedToDisplay) / _parameters.MinSpeedToDisplay);
                _parameters.TrailRendererArray[i].startColor = currentColor;
            }
        }
    }
}