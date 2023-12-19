using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Brake Lights Effect")]
    public class CarVisualsBrakeLights : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer[] _rearLightMeshes;

        [SerializeField, ColorUsageAttribute(true, true)]
        private Color _brakeColor;


        private Material _defaultMaterial;
        private Color _defaultColor;

        private void Awake()
        {
            _defaultMaterial = new Material(_rearLightMeshes[0].material);
            _defaultColor = _defaultMaterial.color;

            for(int i = 0; i < _rearLightMeshes.Length; i++)
            {
                _rearLightMeshes[i].material = _defaultMaterial;
            }
        }

        private bool _lastState = false;

        public void HandleRearLights(bool braking)
        {
            if (_lastState == braking)
                return;

            _lastState = braking;

            _defaultMaterial.SetColor("_BaseColor", braking ? _brakeColor : _defaultColor);
        }
    }
}

