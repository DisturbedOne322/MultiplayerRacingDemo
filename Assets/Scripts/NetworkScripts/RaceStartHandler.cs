using TMPro;
using Unity.Netcode;
using UnityEngine;
 
namespace Assets.VehicleController
{
    public class RaceStartHandler : NetworkBehaviour
    {
        [SerializeField]
        private GameObject _vehiclePrefab;

        [SerializeField]
        private Transform[] _racePositionArray;
        private int _playersSpawned = 0;
        private int _expectedPlayers = 0;

        [SerializeField]
        private TextMeshProUGUI _countdownText;
        private const float COUNTDOWN_TIME_MAX = 3.49f;

        private bool _waitingForPlayers = true;
        private NetworkVariable<float> _countdownTimeNetVar = new NetworkVariable<float>(COUNTDOWN_TIME_MAX, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public override void OnNetworkSpawn()
        {
            RequestSpawnServerRpc(NetworkManager.Singleton.LocalClientId);
            if (IsServer)
                _expectedPlayers = GameObject.FindObjectOfType<JoinServerHandler>().PlayersOnServer;
        }


        [ServerRpc(RequireOwnership = false)]
        private void RequestSpawnServerRpc(ulong id)
        {
            SpawnClientOnLoad(id);
        }

        private void Update()
        {
            if (_countdownTimeNetVar.Value < 0)
            {
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

        private void SpawnClientOnLoad(ulong clientID)
        {
            SpawnPlayerServerRpc(clientID);
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
        private void SpawnPlayerServerRpc(ulong playerId)
        {
            GameObject player = Instantiate(_vehiclePrefab, _racePositionArray[_playersSpawned].position, Quaternion.Euler(_racePositionArray[_playersSpawned].forward));
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerId);
            player.GetComponent<VehicleInputNetworkProvider>().EnableInput(false);
            _playersSpawned++;
        }
    }
}

