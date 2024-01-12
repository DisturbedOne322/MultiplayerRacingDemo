using Assets.VehicleController;
using UnityEngine;

public class UpdateSuspensionVisuals : MonoBehaviour
{
    [SerializeField]
    private SuspensionController[] _suspensionControllers;

    [SerializeField]
    private Transform[] _suspensionVisualTransforms;

    private Vector3[] _defaultScaleArray;
    private float[] _suspensionMeshHeightArray;

    public float length;

    private void Awake()
    {
        int size = _suspensionVisualTransforms.Length;
        _defaultScaleArray = new Vector3[size];
        _suspensionMeshHeightArray = new float[size];
        for (int i = 0; i < size; i++)
        {
            _defaultScaleArray[i] = _suspensionVisualTransforms[i].localScale;
            _suspensionMeshHeightArray[i] = _suspensionVisualTransforms[i].GetComponent<MeshRenderer>().bounds.size.y;
        }
    }

    private void Update()
    {
        for(int i = 0; i < _suspensionControllers.Length; i++)
        {
            Vector3 scale = _defaultScaleArray[i];
            scale.y *= length * _suspensionControllers[i].CurrentSpringLength / _suspensionControllers[i].MaximumSpringLength;
            ScaleAround(_suspensionVisualTransforms[i].gameObject, _suspensionControllers[i].transform.position, scale);
        }
    }

    public void ScaleAround(GameObject target, Vector3 pivot, Vector3 newScale)
    {
        Vector3 A = target.transform.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.transform.localScale.x; // relative scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        target.transform.localScale = newScale;
        target.transform.localPosition = FP;
    }
}
