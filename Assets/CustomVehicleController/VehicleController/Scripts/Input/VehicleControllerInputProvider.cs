using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Input Provider")]
    public class VehicleControllerInputProviderOld : MonoBehaviour, IVehicleControllerInputProvider
    {
        #region Control field
        private float _gasInput;
        private float _brakeInput;
        private bool _handbrakeInput;
        private float _horizontalInput;

        private bool _nitroBoostInput;

        private float _pitchInput;
        private float _yawInput;
        private float _rollInput;

        private bool _shiftedUp;
        private bool _shiftedDown;
        #endregion

        private void Update()
        {
            _gasInput = Input.GetKey(KeyCode.W) ? 1 : 0;
            _brakeInput = Input.GetKey(KeyCode.S) ? 1 : 0;
            _handbrakeInput = Input.GetKey(KeyCode.Space);
            _horizontalInput = Input.GetAxis("Horizontal");

            _nitroBoostInput = Input.GetKey(KeyCode.N);

            _pitchInput = Input.GetAxis("Vertical");

            float yawLeftInput = Input.GetKey(KeyCode.Q) ? 1 : 0;
            float yawRightInput = Input.GetKey(KeyCode.E) ? 1 : 0;
            _yawInput = yawRightInput - yawLeftInput;

            _rollInput = Input.GetAxis("Horizontal");

            _shiftedUp = Input.GetKeyDown(KeyCode.LeftShift);
            _shiftedDown = Input.GetKeyDown(KeyCode.LeftControl);
        }

        public float GetGasInput() => _gasInput;

        public float GetBrakeInput() => _brakeInput;

        public bool GetHandbrakeInput() => _handbrakeInput;

        public float GetHorizontalInput() => _horizontalInput;

        public bool GetGearUpInput() => _shiftedUp;

        public bool GetGearDownInput() => _shiftedDown;

        public float GetPitchInput() => _pitchInput;

        public float GetYawInput() => _yawInput;

        public float GetRollInput() => _rollInput;

        public bool GetNitroBoostInput() => _nitroBoostInput;
    }
}

