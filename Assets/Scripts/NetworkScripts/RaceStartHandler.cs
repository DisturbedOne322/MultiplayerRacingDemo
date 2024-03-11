using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Lobbies.Models;
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

        private float _raceTimer = 3;

        [SerializeField]
        private TextMeshProUGUI _countdownText;

        public override void OnNetworkSpawn()
        {
            if(IsServer && IsOwner)
                NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        }

        private void Singleton_OnClientConnectedCallback(ulong obj)
        {
            SpawnPlayerServerRpc(obj);
            if (_playersSpawned >= 2)
                StartRaceServerRpc();
        }

        [ServerRpc]
        private void StartRaceServerRpc()
        {
            StartTimeClientRpc();
            Invoke("EnableInputOnClients", _raceTimer);
        }

        [ClientRpc]
        private void StartTimeClientRpc()
        {
            StartCoroutine(PerformCountdown());
        }

        private IEnumerator PerformCountdown()
        {
            while (_raceTimer > 0)
            {
                _raceTimer -= Time.deltaTime;
                _countdownText.text = Mathf.CeilToInt(_raceTimer).ToString();
                yield return null;
            }
            _countdownText.gameObject.SetActive(false);
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

