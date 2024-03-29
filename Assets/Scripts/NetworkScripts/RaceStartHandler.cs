using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace Assets.VehicleController
{
    public class RaceStartHandler : NetworkBehaviour
    {
        public static event Action<SplineContainer> OnRaceStart;
        [SerializeField]
        private SplineContainer _raceLayout;

        private bool _raceStarted = false;

        [SerializeField]
        private GameObject _vehiclePrefab;

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

        public override void OnNetworkSpawn()
        {
            _joinServerHandler = GameObject.FindFirstObjectByType<JoinServerHandler>();

            NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

            if (IsServer)
                _expectedPlayers = _joinServerHandler.PlayersOnServer;
        }

        private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
        {
             RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId, _joinServerHandler.VehicleSelectionIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc(ulong id, int vehicleID)
        {
            SpawnClientOnLoad(id, vehicleID);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("MainMenuScene");
            }


            if (_countdownTimeNetVar.Value < 0)
            {
                if (_raceStarted)
                    return;

                _raceStarted = true;

                OnRaceStart?.Invoke(_raceLayout);

                _countdownText.gameObject.SetActive(false);

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

        private void SpawnClientOnLoad(ulong clientID, int vehicleID)
        {
            SpawnPlayerServerRpc(clientID, vehicleID);
            if (_playersSpawned >= _expectedPlayers)
            {
                Invoke("EnableInputOnClients", COUNTDOWN_TIME_MAX);
                _waitingForPlayers = false;
            }
        }

        private void EnableInputOnClients()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            foreach (var cl in clients)
            {
                cl.PlayerObject.GetComponent<VehicleInputNetworkProvider>().EnableInput(true);
            }
        }

        [ServerRpc]
        private void SpawnPlayerServerRpc(ulong playerId, int vehicleID)
        {
            GameObject player = Instantiate(_vehicleSelectionSO.Vehicles[vehicleID], _racePositionArray[_playersSpawned].position, _racePositionArray[_playersSpawned].rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerId);
            player.GetComponent<VehicleInputNetworkProvider>().EnableInput(false);
            _playersSpawned++;
        }
    }
}

