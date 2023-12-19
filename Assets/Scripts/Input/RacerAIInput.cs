//using Dreamteck.Splines;
using UnityEngine;

namespace Assets.VehicleController
{
    //[RequireComponent(typeof(RaycastSensor)),
    //    RequireComponent(typeof(CustomVehicleController))]
    //public class RacerAIInput : MonoBehaviour
    //{
    //    [SerializeField]
    //    private TrackManager _trackManager;
    //    private CustomVehicleController _customVehicleController;
    //    private RaycastSensor _raycastSensor;
    //    private SplineProjector _splineProjector;
    //    private Rigidbody _rb;

    //    private RacerAIBreakInput _breakInput;
    //    private RacerAIGasInput _gasInput;
    //    private RacerAIHorizontalInput _horizontalInput;

    //    private void Awake()
    //    {
    //        _breakInput = new RacerAIBreakInput();
    //        _horizontalInput = new RacerAIHorizontalInput();
    //        _gasInput = new RacerAIGasInput();

    //        _customVehicleController = GetComponent<CustomVehicleController>();
    //        _raycastSensor = GetComponent<RaycastSensor>();
    //        _rb = GetComponent<Rigidbody>();


    //        _splineProjector = GetComponent<SplineProjector>();
    //        _horizontalInput.Initialize(_raycastSensor.RayCastAmount);
    //    }

    //    private void FixedUpdate()
    //    {
    //        HitInfo[] hitInfo = _raycastSensor.GetRaycastHitInfoArray();
    //        _trackManager.GetFutureForwardPaths(_splineProjector,
    //            _customVehicleController.CurrentCarStats.SpeedInMsPerS, out Vector3 averageForwardPath);
            
    //        float gasInput = _gasInput.GetInput(hitInfo, transform.forward, averageForwardPath);
    //        float breakInput = _breakInput.GetInput(hitInfo, transform.forward, averageForwardPath);
    //        float horizontalInput = _horizontalInput.GetHorizontalInput(hitInfo,
    //            _customVehicleController.CurrentCarStats.SpeedInMsPerS, transform.forward, transform.right,
    //            averageForwardPath, this.gameObject);

    //        _customVehicleController.OnGasInput(gasInput);
    //        _customVehicleController.OnBreakInput(breakInput);
    //        _customVehicleController.OnHorizontalInput(horizontalInput);
    //    }

    //    //private void TestOutout()
    //    //{
    //    //    string d = "";
    //    //    string i = "";
    //    //    string v = "";
    //    //    string r = "";
    //    //    float[] danger = _horizontalInput.GetDangerArray();
    //    //    float[] interest = _horizontalInput.GetInterestArray();
    //    //    float[] vel = _horizontalInput.GetVelocityArray();
    //    //    float[] re = _horizontalInput.GetResultArray();
    //    //    for (int n = 0; n < danger.Length; n++)
    //    //    {
    //    //        d += danger[n] + "\n";
    //    //        i += interest[n] + "\n";
    //    //        v += vel[n] + "\n";
    //    //        r += re[n] + "\n";
    //    //    }
    //    //    dangerText.text = d;
    //    //    interestText.text = i;
    //    //    velocityText.text = v;
    //    //    resultText.text = r;
    //    //}

    //}
}
