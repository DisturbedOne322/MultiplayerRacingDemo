using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class JoinServerHandler : MonoBehaviour
{
    private UnityTransport _unityTransport;

    private int _playersOnServer;
    public int PlayersOnServer => _playersOnServer;

    private void Awake()
    {
        _unityTransport = GameObject.FindObjectOfType<UnityTransport>();
    }

    public void SetPlayersAmount(int amount) => _playersOnServer = amount;

    public async Task<string> HostGame(int maxPlayers)
    {
        try
        {
            Allocation a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            _unityTransport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            NetworkManager.Singleton.StartHost();
            Debug.Log("successfully created server " + joinCode);
            return joinCode;
        }
        catch(RelayServiceException ex)
        {
            Debug.Log(ex);
            return "";
        }
    }

    public async Task<bool> JoinGame(string joinCode)
    {
        if (joinCode == "")
            return false;
        try
        {
            JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinCode);

            _unityTransport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch(RelayServiceException ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    public void LeaveServer()
    {
        NetworkManager.Singleton.Shutdown();

    }
}
