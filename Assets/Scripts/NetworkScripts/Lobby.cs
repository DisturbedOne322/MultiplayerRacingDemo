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

    [SerializeField]
    private PlayerData _playerData;

    public event Action OnKicked;

    private MyLobby _hostedLobby;
    public MyLobby HostedLobby { get => _hostedLobby;}
    private MyLobby _joinedLobby;
    public MyLobby JoinedLobby { get => _joinedLobby; }

    [SerializeField]
    private JoinServerHandler _joinServerHandler;

    public bool GameStarted = false;

    private ILobbyEvents _lobbyEvents;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
            if (playersInLobby[i].Id == JoinedLobby.HostId)
                continue;

            if (playersInLobby[i].Data["Ready"].Value == "False")
            {
                Debug.Log("Not everyone ready");
                return false;
            }
        }

        return true;
    }

    public bool CheckIsPlayerReady(string playerId)
    {
        for(int i = 0; i < JoinedLobby.Players.Count; i++)
        {
            if (JoinedLobby.Players[i].Id != playerId)
                continue;

            return JoinedLobby.Players[i].Data["Ready"].Value == "True";
        }

        return false;
    }

    public async void StartGame()
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
            updateLobbyOptions.Data = new Dictionary<string, DataObject>
            {
                {"GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "True") }
            };

            _hostedLobby = await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateLobbyOptions);

            NetworkManager.Singleton.SceneManager.LoadScene("CityScene", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
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

            _playerData.ResetReadyStatus();
            string joinCode = await _joinServerHandler.HostGame(maxPlayers);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = _playerData.GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)},
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "False") },
                }
            };

            _joinedLobby = _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
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
            _playerData.ResetReadyStatus();
            string joinCode = await _joinServerHandler.HostGame(maxPlayers);

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = _playerData.GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "False") },
                },
            };
            
            _joinedLobby = _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
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
            _playerData.ResetReadyStatus();

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = _playerData.GetPlayer(),
            };

            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            bool result = await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);

            if(!result)
                return false;

            SubscribeToKickEvent(JoinedLobby.Id);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async Task<bool> JoinByCode(string code)
    {
        try
        {
            _playerData.ResetReadyStatus();

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = _playerData.GetPlayer()           
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);

            bool result = await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);
            if (!result)
                return false;

            SubscribeToKickEvent(JoinedLobby.Id);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async Task<bool> JoinByID(string id)
    {
        try
        {
            _playerData.ResetReadyStatus();

            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = _playerData.GetPlayer(),                
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByCodeOptions);
            bool result = await _joinServerHandler.JoinGame(_joinedLobby.Data["JoinCode"].Value);

            if (!result)
                return false;

            SubscribeToKickEvent(JoinedLobby.Id);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public void LeaveLobbyAndServer()
    {
        _joinServerHandler.LeaveServer();
        LeaveLobby();
    }

    public async void LeaveLobby()
    {
        if (JoinedLobby == null)
            return;

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, playerId);
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
            return queryResponse.Results;   
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
}
