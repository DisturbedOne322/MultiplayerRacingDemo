using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    private List<ulong> _finishedClientIdList;
    private JoinServerHandler _joinServerHandler;


    public override void OnNetworkSpawn()
    {
        if(IsServer)
            _finishedClientIdList = new List<ulong>();
        
        _joinServerHandler = GameObject.FindFirstObjectByType<JoinServerHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.transform.root.CompareTag("Player"))
            return;

        if (other.gameObject.transform.root.GetComponent<NetworkObject>().OwnerClientId != NetworkManager.Singleton.LocalClientId)
            return;

        WritePlayerNameServerRpc(NetworkManager.Singleton.LocalClientId, new FixedString64Bytes(_joinServerHandler.LocalPlayerName));
    }


    [ServerRpc(RequireOwnership = false)]
    private void WritePlayerNameServerRpc(ulong clientID, FixedString64Bytes playerName)
    {
        if (_finishedClientIdList.Contains(clientID))
            return;

        _finishedClientIdList.Add(clientID);
        DisplayWinnerClientRpc(playerName, _finishedClientIdList.Count);
    }

    [ClientRpc]
    private void DisplayWinnerClientRpc(FixedString64Bytes name, int position)
    {
        Debug.Log($"Player {name} finished at position {position}");    
    }
}
