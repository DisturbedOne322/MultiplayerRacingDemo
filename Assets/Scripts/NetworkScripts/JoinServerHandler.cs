using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinServerHandler : MonoBehaviour
{
    private UnityTransport _unityTransport;

    private int _playersOnServer;
    public int PlayersOnServer => _playersOnServer;

    private int _vehicleSelectionIndex = 0;
    public int VehicleSelectionIndex => _vehicleSelectionIndex;

    private string _localPlayerName = "";
    public string LocalPlayerName => _localPlayerName;

    public void SetLocalPlayerName(string localPlayerName) => _localPlayerName = localPlayerName;

    private void Awake()
    {
        _unityTransport = GameObject.FindObjectOfType<UnityTransport>();
    }

    private void Start()
    {
        NetworkManager.Singleton.RunInBackground = true;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        VehicleSelectionInLobby.OnVehicleSelectionChanged += UpdateSelection;
    }


    private void UpdateSelection(int index)
    {
        _vehicleSelectionIndex = index;
    }

    private void Singleton_OnClientDisconnectCallback(ulong id)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            //host
            if (id == 0)
            {
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("MainMenuScene");
            }
        }
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
