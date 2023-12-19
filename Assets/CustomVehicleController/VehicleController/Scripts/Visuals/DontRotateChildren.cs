using UnityEngine;

namespace Assets.VehicleController
{
    public class DontRotateChildren : MonoBehaviour
    {
        private Transform[] _childrenArray;
        private int _size;
        private void Awake()
        {
            _childrenArray = GetComponentsInChildren<Transform>();
            _size = _childrenArray.Length;
        }

        private void Update()
        {
            for (int i = 1; i < _size; i++)
            {
                _childrenArray[i].transform.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, 0, 0);
            }
        }
    }
}
