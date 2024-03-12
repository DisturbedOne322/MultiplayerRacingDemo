using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using MyLobby = Unity.Services.Lobbies.Models.Lobby;

public class Lobby : MonoBehaviour
{
    public static Lobby Instance { get; private set; }

    [SerializeField]
    private Button _createLobbyButton;

    [SerializeField]
    private Button _quickJoinButton;

    [SerializeField]    
    private Button _findLobbiesButton;

    [SerializeField]
    private Button _joinByCodeButton;

    [SerializeField]
    private TMP_InputField _lobbyCodeInputField;

    private MyLobby _hostedLobby;
    public MyLobby HostedLobby { get => _hostedLobby;}
    private MyLobby _joinedLobby;
    public MyLobby JoinedLobby { get => _joinedLobby; }

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

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = Authenticate.Instance.GetPlayer(),
            };
            
            _joinedLobby = _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created Lobby " + _hostedLobby.Name + " " + _hostedLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = Authenticate.Instance.GetPlayer(),
            };

            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinByCode(string code)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = Authenticate.Instance.GetPlayer()           
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            Debug.Log("Joined by code " + code);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinByID(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions
            {
                Player = Authenticate.Instance.GetPlayer(),                
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(id, joinLobbyByCodeOptions);
            Debug.Log("Joined by id " + id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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
