using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsBrakeLights
    {
        private BrakeLightsParameters _parameters;
        private Material _defaultMaterial;
        private Color _defaultColor;

        public CarVisualsBrakeLights(BrakeLightsParameters parameters)
        {
            _parameters = parameters;

            if (_parameters.RearLightMeshes.Length == 0)
            {
                Debug.LogWarning("You have Brake Lights Effect, but MeshRenderer array is not assigned");
                return;
            }

            if(_parameters.MaterialAtSpecificIndex)
            {
                Material[] materials = _parameters.RearLightMeshes[0].materials;
                _defaultMaterial = new Material(materials[_parameters.MaterialIndex]);
                materials[_parameters.MaterialIndex] = _defaultMaterial;
                _defaultColor = _defaultMaterial.color;

                for (int i = 0; i < parameters.RearLightMeshes.Length; i++)
                {
                    parameters.RearLightMeshes[i].materials = materials;
                }
            }
            else
            {
                _defaultMaterial = new Material(_parameters.RearLightMeshes[0].material);
                _defaultColor = _defaultMaterial.color;

                for (int i = 0; i < parameters.RearLightMeshes.Length; i++)
                {
                    parameters.RearLightMeshes[i].material = _defaultMaterial;
                }
            }
        }

        private bool _lastState = false;

        public void HandleRearLights(bool braking)
        {
            if (_lastState == braking)
                return;

            _lastState = braking;

            _defaultMaterial.SetColor("_BaseColor", braking ? _parameters.BrakeColor : _defaultColor);
        }
    }
}

