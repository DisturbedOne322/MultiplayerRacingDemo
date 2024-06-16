using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace Assets.VehicleController
{
    public class RaceStartHandler : NetworkBehaviour
    {
        public static event Action<SplineContainer> OnRaceStart;

        private bool _raceStarted;
        public bool RaceStarted => _raceStarted;
        private bool _raceFinished;
        public bool RaceFinished => _raceFinished;

        [SerializeField]
        private SplineContainer _raceLayout;

        [SerializeField]
        private Transform[] _racePositionArray;
        private int _playersSpawned = 0;
        private int _expectedPlayers = 0;

        [SerializeField]
        private TextMeshProUGUI _countdownText;
        private const float COUNTDOWN_TIME_MAX = 3f;

        private bool _waitingForPlayers = true;
        private NetworkVariable<float> _countdownTimeNetVar = new NetworkVariable<float>(COUNTDOWN_TIME_MAX, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private JoinServerHandler _joinServerHandler;
        [SerializeField]
        private VehicleSelectionSO _vehicleSelectionSO;

        public Vector3 SpawnPos;
        public Quaternion SpawnRot;

        private void Start()
        {
            FinishLine.LocalPlayerFinishedRace += FinishLine_LocalPlayerFinishedRace;
        }

        private void FinishLine_LocalPlayerFinishedRace()
        {
            _raceFinished = true;
        }

        private void OnDestroy()
        {
            FinishLine.LocalPlayerFinishedRace -= FinishLine_LocalPlayerFinishedRace;
        }

        public override void OnNetworkSpawn()
        {
            _joinServerHandler = GameObject.FindFirstObjectByType<JoinServerHandler>();
            NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

            if (IsServer)
            {
                _expectedPlayers = _joinServerHandler.PlayersOnServer;
                _raceStarted = false;
                _countdownTimeNetVar.Value = COUNTDOWN_TIME_MAX;
            }
        }

        private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
        {
             RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId, _joinServerHandler.VehicleSelectionIndex);       
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc(ulong id, int vehicleID)
        {
            SetTransformClientRpc(id, _racePositionArray[_playersSpawned].position, _racePositionArray[_playersSpawned].rotation);

            GameObject player = Instantiate(_vehicleSelectionSO.Vehicles[vehicleID]);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            player.GetComponent<VehicleInputNetworkProvider>().EnableInput(false);

            _playersSpawned++;
            if (_playersSpawned >= _expectedPlayers)
            {
                Invoke("EnableInputOnClients", COUNTDOWN_TIME_MAX);
                _waitingForPlayers = false;
            }
        }

        [ClientRpc(RequireOwnership = false)]
        private void SetTransformClientRpc(ulong id, Vector3 pos, Quaternion rot)
        {
            if (NetworkManager.Singleton.LocalClientId != id)
                return;

            SpawnPos = pos;
            SpawnRot = rot;
        }


        private void EnableInputOnClients()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            foreach (var cl in clients)
            {
                cl.PlayerObject.GetComponent<VehicleInputNetworkProvider>().EnableInput(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Lobby.Instance.LeaveLobbyAndServer();
                SceneManager.LoadScene("MainMenuScene");
            }


            if (_countdownTimeNetVar.Value <= 0)
            {
                if (_raceStarted)
                    return;

                _raceStarted = true;
                _countdownText.gameObject.SetActive(false);
                OnRaceStart?.Invoke(_raceLayout);
                return;
            }

            if(_countdownTimeNetVar.Value != COUNTDOWN_TIME_MAX)
                _countdownText.text = Mathf.CeilToInt(_countdownTimeNetVar.Value).ToString();

            if (!NetworkManager.Singleton.IsServer)
                return;

            if (_waitingForPlayers)
                return;

            if(_countdownTimeNetVar.Value > 0)
                _countdownTimeNetVar.Value -= Time.deltaTime;
        }
    }
}

