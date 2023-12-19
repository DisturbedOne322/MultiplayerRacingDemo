using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Visuals/Tire Trail Effect")]
    public class CarVisualsSkidMarks : MonoBehaviour
    {
        [SerializeField]
        public TrailRenderer TireTrail;
        private TrailRenderer[] _tireTrailArray;


        public void InstantiateTireTrailRenderers(Transform[] wheelMeshesArray)
        {
            if (TireTrail == null)
            {
                Debug.LogWarning("You have Skid Marks Effect, but Trail Renderer is not assigned");
                return;
            }

            int size = wheelMeshesArray.Length;
            _tireTrailArray = new TrailRenderer[size];

            for (int i = 0; i < size; i++)
            {
                _tireTrailArray[i] = Instantiate(TireTrail);

                _tireTrailArray[i].transform.forward = Vector3.up;
                _tireTrailArray[i].transform.parent = wheelMeshesArray[i].parent;
            }
        }

        public void DisplayTireTrail(bool display, int id, Vector3 position)
        {
            if (TireTrail == null)
                return;

            if (display)
            {
                position.y += 0.05f;
                _tireTrailArray[id].transform.position = position;
                _tireTrailArray[id].emitting = true;
            }
            else
            {
                _tireTrailArray[id].emitting = false;
            }
        }
    }

}
