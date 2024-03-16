using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    private NetworkVariable<bool> _playerFinished = new NetworkVariable<bool>();

    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
            NetworkObject.Spawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;
        if (!other.gameObject.CompareTag("Player"))
            return;
        if (_playerFinished.Value)
            return;
        FixedString64Bytes playerWin = new FixedString64Bytes(other.gameObject.transform.root.name);
        WritePlayerNameServerRpc(playerWin);
    }

    [ServerRpc]
    private void WritePlayerNameServerRpc(FixedString64Bytes name)
    {
        _playerFinished.Value = true;
        DisplayWinnerClientRpc(name);
    }

    [ClientRpc]
    private void DisplayWinnerClientRpc(FixedString64Bytes name)
    {
        Debug.Log(name);    
    }
}
