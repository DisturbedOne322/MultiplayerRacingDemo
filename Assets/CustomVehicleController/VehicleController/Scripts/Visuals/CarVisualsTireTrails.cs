using UnityEngine;

namespace Assets.VehicleController
{
    public class CarVisualsTireTrails
    {
        private TireTrailParameters _parameters;
        private TrailRenderer[] _tireTrailArray;
        private float[] _radiusArray;
        private Transform[] _wheelMeshesArray;

        public CarVisualsTireTrails(Transform[] wheelMeshesArray, WheelController[] wheelControllers, TireTrailParameters parameters)
        {
            _parameters = parameters;
            if (_parameters.TrailRenderer == null)
            {
                Debug.LogWarning("You have Skid Marks Effect, but Trail Renderer is not assigned");
                return;
            }

            int size = wheelMeshesArray.Length;
            _tireTrailArray = new TrailRenderer[size];
            _radiusArray = new float[size];
            _wheelMeshesArray = wheelMeshesArray;

            for (int i = 0; i < size; i++)
            {
                _tireTrailArray[i] = GameObject.Instantiate(_parameters.TrailRenderer);

                _tireTrailArray[i].transform.forward = Vector3.up;
                _tireTrailArray[i].transform.parent = wheelMeshesArray[i].parent;
                _radiusArray[i] = wheelControllers[i].Radius;
            }
        }

        public void DisplayTireTrail(bool display, int id)
        {
            _tireTrailArray[id].emitting = display;

            if(display)
                _tireTrailArray[id].transform.position = _wheelMeshesArray[id].position - new Vector3(0, _radiusArray[id] - _parameters.VerticalOffset, 0);
        }
    }

}
