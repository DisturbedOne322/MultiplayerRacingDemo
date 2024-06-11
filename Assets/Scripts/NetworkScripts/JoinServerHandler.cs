using AYellowpaper.SerializedCollections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinServerHandler : NetworkBehaviour
{
    private UnityTransport _unityTransport;

    private int _playersOnServer;
    public int PlayersOnServer => _playersOnServer;

    private int _vehicleSelectionIndex = 0;
    public int VehicleSelectionIndex => _vehicleSelectionIndex;

    private string _localPlayerName = "";
    public string LocalPlayerName => _localPlayerName;

    public SerializedDictionary<ulong, string> ClientIdToNameDict;

    public void SetLocalPlayerName(string localPlayerName) => _localPlayerName = localPlayerName;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        _unityTransport = GameObject.FindFirstObjectByType<UnityTransport>();
        if (IsServer)
            ClientIdToNameDict = new SerializedDictionary<ulong, string>();
    }

    public override void OnNetworkSpawn()
    {
        SendDataToServerRpc(NetworkManager.Singleton.LocalClientId, PlayerData.Instance.GetPlayer().Data["PlayerName"].Value);
        NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
    }

    private void Start()
    {
        NetworkManager.Singleton.RunInBackground = true;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        VehicleSelectionInLobby.OnVehicleSelectionChanged += UpdateSelection;
    }

    private void Singleton_OnServerStopped(bool obj)
    {
        if (Lobby.Instance.JoinedLobby == null)
            return;

        if (Lobby.Instance.JoinedLobby.Data["GameStarted"].Value == "False")
            MenuHandler.Instance.RemoveLastMenu();
    }

    [ServerRpc(RequireOwnership = false)] 
    private void SendDataToServerRpc(ulong clientID, string lobbyUserName)
    {
        ClientIdToNameDict.Add(clientID, lobbyUserName);
    }

    private void UpdateSelection(int index)
    {
        _vehicleSelectionIndex = index;
    }

    private void Singleton_OnClientDisconnectCallback(ulong id)
    {
        if (IsServer)
            ClientIdToNameDict.Remove(id);

        if (NetworkManager.Singleton.LocalClientId == id)
        {
            if (Lobby.Instance.JoinedLobby.Data["GameStarted"].Value == "False")
                MenuHandler.Instance.RemoveLastMenu();
            else
                SceneManager.LoadScene("MainMenuScene");
            Lobby.Instance.LeaveLobby();
        }
    }

    public void SetPlayersAmount(int amount) => _playersOnServer = amount;

    public async Task<string> HostGame(int maxPlayers)
    {
        try
        {
            ClientIdToNameDict.Clear();

            Allocation a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            _unityTransport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            NetworkManager.Singleton.StartHost();
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
        if (IsServer)
            ClientIdToNameDict.Clear();

        NetworkManager.Singleton.Shutdown();
    }
}
