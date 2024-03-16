using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyLobby = Unity.Services.Lobbies.Models.Lobby;

public class Lobby : MonoBehaviour
{
    public static Lobby Instance { get; private set; }

    public event Action OnKicked;

    private MyLobby _hostedLobby;
    public MyLobby HostedLobby { get => _hostedLobby;}
    private MyLobby _joinedLobby;
    public MyLobby JoinedLobby { get => _joinedLobby; }

    [SerializeField]
    private JoinServerHandler _joinServerHandler;

    private ILobbyEvents _lobbyEvents;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void UpdateJoinedLobby(MyLobby updatedLobby)
    {
        _joinedLobby = updatedLobby;

        if (_joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
            _hostedLobby = _joinedLobby;
        else
            _hostedLobby = null;
    }

    public bool CheckEveryoneReady()
    {
        List<Player> playersInLobby = JoinedLobby.Players;
        for (int i = 0; i < playersInLobby.Count; i++)
        {
            if (playersInLobby[i].Id == Lobby.Instance.JoinedLobby.HostId)
                continue;

            if (playersInLobby[i].Data["Ready"].Value == "False")
            {
                Debug.Log("Not everyone ready");
                return false;
            }
        }

        return true;
    }

    public async void StartGame()
    {
        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
        updateLobbyOptions.Data = new Dictionary<string, DataObject>
        {
            {"GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "True")},
        };

        await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateLobbyOptions);

        Invoke("LoadScene", 1.2f);
    }

    private void LoadScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
    }

    public async void UpdatePlayerStatus(string newStatus)
    {
        try
        {
            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions();
            updatePlayerOptions.Data = new Dictionary<string, PlayerDataObject>
            {
                {"Ready", new PlayerDataObject( PlayerDataObject.VisibilityOptions.Member, newStatus) }
            };

            await LobbyService.Instance.UpdatePlayerAsync(Lobby.Instance.JoinedLobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
        }
        catch (LobbyServiceException ex) 
        {
            Debug.Log(ex);
        }
    }

    public void KickPlayer(Player player)
    {
        try
        {
            LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, player.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task<bool> CreateLobbyDefault(int maxPlayers)
    {
        try
        {
            string lobbyName = "Lobby" + $"{UnityEngine.Random.Range(0,99999) : 00000}";
            Authenticate.Instance.ResetReadyStatus();
            string joinCode = await _joinServerHandler.HostGame(maxPlayers);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = Authenticate.Instance.GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)},
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "False") },
                }
            };

            _joinedLobby = _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created Lobby " + _hostedLobby.Name + " " + _hostedLobby.LobbyCode);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async Task<bool> CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            Authenticate.Instance.ResetReadyStatus();
            string joinCode = await _joinServerHandler.HostGame(maxPlayers);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = Authenticate.Instance.GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                },
            };
            
            _joinedLobby = _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created Lobby " + _hostedLobby.Name + " " + _hostedLobby.LobbyCode);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    private async void SubscribeToKickEvent(string lobbyID)
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.KickedFromLobby += Callbacks_KickedFromLobby;

        try
        {
            _lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobbyID, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{lobbyID}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
    }

    private void Callbacks_KickedFromLobby()
    {
        OnKicked?.Invoke();
        _lobbyEvents = null;
        _joinedLobby = _hostedLobby = null;
        _joinServerHandler.LeaveServer();
    }

    public async Task<bool> QuickJoin()
    {
        try
        {
            Authenticate.Instance.ResetReadyStatus();

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = Authenticate.Instance.GetPlayer(),
            };

            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);

            await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);
            SubscribeToKickEvent(JoinedLobby.Id);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async void JoinByCode(string code)
    {
        try
        {
            Authenticate.Instance.ResetReadyStatus();

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = Authenticate.Instance.GetPlayer()           
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);
            SubscribeToKickEvent(JoinedLobby.Id);
            Debug.Log("Joined by code " + code);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task<bool> JoinByID(string id)
    {
        try
        {
            Authenticate.Instance.ResetReadyStatus();

            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = Authenticate.Instance.GetPlayer(),                
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByCodeOptions);
            await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);
            SubscribeToKickEvent(JoinedLobby.Id);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async void LeaveLobby()
    {
        if (JoinedLobby == null)
            return;

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, playerId);
            _joinServerHandler.LeaveServer();
            _hostedLobby = _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task<List<MyLobby>> FindLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found : " + queryResponse.Results.Count);
            return queryResponse.Results;   
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
}
