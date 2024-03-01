using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class DemoManager : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;
        private VehicleEngineSoundManager _engineSoundManager;

        [SerializeField]
        private CarEngineSoundSO[] _engineSoundSOArray;

        [SerializeField]
        private VehiclePartsPresetSO[] _vehiclePartsPressets;
        private int _currentPresetId;

        [SerializeField]
        private Camera[] _cameraArray;
        private int _currentCameraID = 0;
        private string[] _cameraNameArray;

        [SerializeField]
        private GameObject _staticUIParent;
        [SerializeField]
        private GameObject _dynamicUIParent;

        [SerializeField]
        private GameObject _vehicleControlsMenu;
        [SerializeField]
        private GameObject _demoControlsMenu;

        [SerializeField, Space]
        private GameObject _partsMenu;
        [SerializeField]
        private GameObject _currentPartsStaticMenu;
        [SerializeField]
        private GameObject _currentPartsDynamicMenu;


        [SerializeField, Space]
        private Text _currentEngine;
        [SerializeField]
        private Text _currentNitro;
        [SerializeField]
        private Text _currentTransmission;
        [SerializeField]
        private Text _currentTires;
        [SerializeField]
        private Text _currentSuspension;
        [SerializeField]
        private Text _currentBrakes;
        [SerializeField]
        private Text _currentBody;


        [SerializeField, Space]
        private Text _transmissionType;
        [SerializeField]
        private Text _drivetrainType;
        [SerializeField]
        private Text _presetType;
        [SerializeField]
        private Text _cameraTypeName;

        [SerializeField]
        private AudioMixer _audioMixer;
        private float _currentVolume = 0.7f;

        public Separator separator;

        [SerializeField, Space]
        private EngineSO[] _engineArray;
        [SerializeField]
        private NitrousSO[] _nitrousArray;
        [SerializeField]
        private TransmissionSO[] _transmissionArray;
        [SerializeField]
        private SuspensionSO[] _suspensionArray;
        [SerializeField]
        private TiresSO[] _tireArray;
        [SerializeField]
        private BrakesSO[] _brakesArray;
        [SerializeField]
        private VehicleBodySO[] _vehicleBodyArray;

        private int[] _partsIdArray;

        private void Start()
        {
            _engineSoundManager = _vehicleController.GetComponent<VehicleEngineSoundManager>();
            VehiclePartsSetWrapper.OnAnyPresetChanged += VehiclePartsSetWrapper_OnAnyPresetChanged;
            _cameraNameArray = new string[3];
            _cameraNameArray[0] = "Orbit";
            _cameraNameArray[1] = "Hood";
            _cameraNameArray[2] = "Top down";


            _partsIdArray = new int[7];


            _currentPresetId = -1;
            _vehicleController.UsePreset = false;
            _vehicleController.SetNewPartToCustomizableSet(_engineArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_nitrousArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_transmissionArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[0], true);
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[0], false);
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[0], true);
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[0], false);
            _vehicleController.SetNewPartToCustomizableSet(_brakesArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_vehicleBodyArray[0]);
            UpdatePartsMenu();

            _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
        }
        private void OnDestroy()
        {
            VehiclePartsSetWrapper.OnAnyPresetChanged -= VehiclePartsSetWrapper_OnAnyPresetChanged;
        }

        private void VehiclePartsSetWrapper_OnAnyPresetChanged()
        {
            UpdatePartsMenu();
        }

        private void Update()
        {
            HandleAudio();
            UpdateStatsMenu();
            ChangeEngine();
            ChangeNitrous();
            ChangeTransmission();
            ChangeSuspension();
            ChangeTires();
            ChangeBrakes();
            ChangeBody();

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("CustomVehicleControllerDemoScene");
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                _vehicleController.TransmissionType =
                    _vehicleController.TransmissionType == TransmissionType.Automatic ?
                    TransmissionType.Manual : TransmissionType.Automatic;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (_vehiclePartsPressets.Length != 0)
                {
                    _currentPresetId++;
                    if (_currentPresetId >= _vehiclePartsPressets.Length)
                    {
                        _vehicleController.UsePreset = false;
                        _currentPresetId = -1;
                        UpdateEngineSoundFromPart();
                    }
                    else
                    {
                        _vehicleController.SetVehiclePresetSO(_vehiclePartsPressets[_currentPresetId]);
                        _vehicleController.UsePreset = true;

                        UpdateEngineSoundFromPreset();
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.V))
            {
                ChangeCamera();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                _vehicleController.DrivetrainType = GetNextDrivetrainType(_vehicleController.DrivetrainType);
            }

            OpenCloseMenus(_vehicleControlsMenu, KeyCode.F1, _demoControlsMenu);
            OpenCloseMenus(_vehicleControlsMenu, KeyCode.Z, _demoControlsMenu);

            OpenCloseMenus(_demoControlsMenu, KeyCode.F2, _vehicleControlsMenu);
            OpenCloseMenus(_demoControlsMenu, KeyCode.X, _vehicleControlsMenu);

            OpenCloseMenus(_staticUIParent, KeyCode.F9);
            OpenCloseMenus(_dynamicUIParent, KeyCode.F9);
            HandlePartsMenu();

            if(Input.GetKeyDown(KeyCode.Q))
            {
                Application.Quit();
            }

        }

        private void HandleAudio()
        {
            if (Input.GetKeyDown(KeyCode.Alpha9))
                _currentVolume -= 0.1f;

            if (Input.GetKeyDown(KeyCode.Alpha0))
                _currentVolume += 0.1f;

            _currentVolume = Mathf.Clamp(_currentVolume, 0.001f, 1);
            _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
        }

        private DrivetrainType GetNextDrivetrainType(DrivetrainType current)
        {
            switch (current)
            {
                case DrivetrainType.RWD:
                    return DrivetrainType.AWD;
                case DrivetrainType.AWD:
                    return DrivetrainType.FWD;
                default:
                    return DrivetrainType.RWD;
            }
        }

        private void ChangeEngine()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha1))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[0]++;
                if (_partsIdArray[0] >= _engineArray.Length)
                    _partsIdArray[0] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_engineArray[_partsIdArray[0]]);

                UpdateEngineSoundFromPart();
            }
        }

        private void UpdateEngineSoundFromPart()
        {
            //v8
            if (_partsIdArray[0] == 3)
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[2]);
            //inline 6
            else if (_partsIdArray[0] == 4)
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[1]);
            //rotary
            else
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[0]);
        }

        private void UpdateEngineSoundFromPreset()
        {
            //rotary
            if (_currentPresetId == 0)
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[0]);
            //inline 6
            else if (_currentPresetId == 1)
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[1]);
            //v8
            else
                _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[2]);
        }

        private void ChangeNitrous()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha2))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[1]++;
                if (_partsIdArray[1] >= _nitrousArray.Length)
                    _partsIdArray[1] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_nitrousArray[_partsIdArray[1]]);
            }
        }
        private void ChangeTransmission()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha3))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[2]++;
                if (_partsIdArray[2] >= _transmissionArray.Length)
                    _partsIdArray[2] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_transmissionArray[_partsIdArray[2]]);
            }
        }
        private void ChangeTires()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha4))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[3]++;
                if (_partsIdArray[3] >= _tireArray.Length)
                    _partsIdArray[3] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_tireArray[_partsIdArray[3]], true);
                _vehicleController.SetNewPartToCustomizableSet(_tireArray[_partsIdArray[3]], false);
            }
        }
        private void ChangeSuspension()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha5))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[4]++;
                if (_partsIdArray[4] >= _suspensionArray.Length)
                    _partsIdArray[4] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[_partsIdArray[4]], true);
                _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[_partsIdArray[4]], false);
            }
        }
        private void ChangeBrakes()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha6))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[5]++;
                if (_partsIdArray[5] >= _brakesArray.Length)
                    _partsIdArray[5] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_brakesArray[_partsIdArray[5]]);
            }
        }
        private void ChangeBody()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha7))
                return;

            if (!_vehicleController.UsePreset)
            {
                _partsIdArray[6]++;
                if (_partsIdArray[6] >= _tireArray.Length)
                    _partsIdArray[6] = 0;

                _vehicleController.SetNewPartToCustomizableSet(_vehicleBodyArray[_partsIdArray[6]]);
            }
        }

        private void UpdateStatsMenu()
        {
            _transmissionType.text = _vehicleController.TransmissionType.ToString();
            if (_vehicleController.UsePreset)
                _presetType.text = _vehicleController.GetVehiclePreset().name;
            else
                _presetType.text = "Customizable Set";

            _drivetrainType.text = _vehicleController.DrivetrainType.ToString();

            _cameraTypeName.text = _cameraNameArray[_currentCameraID];
        }

        private void OpenCloseMenus(GameObject menu, KeyCode key, GameObject conflictingMenu = null)
        {
            if (Input.GetKeyDown(key))
            {
                if (conflictingMenu != null)
                {
                    if (conflictingMenu.activeSelf)
                        conflictingMenu.SetActive(false);
                }

                menu.SetActive(!menu.activeSelf);
            }
        }

        private void HandlePartsMenu()
        {
            _partsMenu.SetActive(!_vehicleController.UsePreset);

            if (_vehicleController.UsePreset)
            {
                _currentPartsStaticMenu.SetActive(false);
                _currentPartsDynamicMenu.SetActive(false);
                return;
            }

            OpenCloseMenus(_currentPartsStaticMenu, KeyCode.F3);
            OpenCloseMenus(_currentPartsStaticMenu, KeyCode.C);
            OpenCloseMenus(_currentPartsDynamicMenu, KeyCode.F3);
            OpenCloseMenus(_currentPartsDynamicMenu, KeyCode.C);
        }
        private void UpdatePartsMenu()
        {
            VehiclePartsCustomizableSet _partsCustomizableSet = _vehicleController.GetCustomizableSet();
            _currentEngine.text = _partsCustomizableSet.Engine.name;
            _currentNitro.text = _partsCustomizableSet.Nitrous.name;
            _currentTransmission.text = _partsCustomizableSet.Transmission.name;
            _currentTires.text = _partsCustomizableSet.FrontTires.name;
            _currentSuspension.text = _partsCustomizableSet.FrontSuspension.name;
            _currentBrakes.text = _partsCustomizableSet.Brakes.name;
            _currentBody.text = _partsCustomizableSet.Body.name;
        }
        private void ChangeCamera()
        {
            _cameraArray[_currentCameraID].gameObject.SetActive(false);
            _currentCameraID++;
            if (_currentCameraID >= _cameraArray.Length)
                _currentCameraID = 0;
            _cameraArray[_currentCameraID].gameObject.SetActive(true);
        }
    }
}
