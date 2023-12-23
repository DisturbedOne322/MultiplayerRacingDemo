using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Tire Trail Effect")]
    public class CarVisualsSkidMarks : MonoBehaviour
    {
        [SerializeField]
        public TrailRenderer TireTrail;
        private TrailRenderer[] _tireTrailArray;

        private float[] _radiusArray;
        private Transform[] _wheelMeshesArray;
        private int _size;

        [SerializeField]
        private float _verticalOffset;


        public void InstantiateTireTrailRenderers(Transform[] wheelMeshesArray, WheelController[] wheelControllers)
        {
            if (TireTrail == null)
            {
                Debug.LogWarning("You have Skid Marks Effect, but Trail Renderer is not assigned");
                return;
            }

            _size = wheelMeshesArray.Length;
            _tireTrailArray = new TrailRenderer[_size];
            _radiusArray = new float[_size];
            _wheelMeshesArray = wheelMeshesArray;

            for (int i = 0; i < _size; i++)
            {
                _tireTrailArray[i] = Instantiate(TireTrail);

                _tireTrailArray[i].transform.forward = Vector3.up;
                _tireTrailArray[i].transform.parent = wheelMeshesArray[i].parent;
                _radiusArray[i] = wheelControllers[i].Radius;
            }
        }

        public void DisplayTireTrail(bool display, int id)
        {
            if (TireTrail == null)
                return;

            _tireTrailArray[id].emitting = display;

            if(display)
                _tireTrailArray[id].transform.position = _wheelMeshesArray[id].position - new Vector3(0, _radiusArray[id] - _verticalOffset, 0);
        }
    }

}
