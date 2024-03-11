using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private TMP_InputField _playerNameField;

    private MyLobby _createdLobby;
    private MyLobby _joinedLobby;

    private float _pingTimerMax = 15;
    private float _pingTimer = 15;

    private float _updateTimer = 1.1f;
    private float _updateTimerMax = 1.1f;

    private void Awake()
    {
        _createLobbyButton.onClick.AddListener(() => { CreateLobby(); });
        _findLobbiesButton.onClick.AddListener(() => { FindLobbies(); });
        _quickJoinButton.onClick.AddListener(() => { QuickJoin(); });
        _joinByCodeButton.onClick.AddListener(() => { JoinByCode(_lobbyCodeInputField.text); });
    }

    private void Update()
    {
        PingServer();
        UpdateLobbyData();
    }

    private async void PingServer()
    {
        if (_createdLobby == null)
            return;
        _pingTimer -= Time.unscaledDeltaTime;
        if (_pingTimer < 0)
        {
            _pingTimer = _pingTimerMax;
            Debug.Log("Ping");
            await LobbyService.Instance.SendHeartbeatPingAsync(_createdLobby.Id);
        }
    }

    private async void UpdateLobbyData()
    {
        if (_joinedLobby == null)
            return;

        _updateTimer -= Time.unscaledDeltaTime;
        if (_updateTimer < 0)
        {
            _updateTimer = _updateTimerMax;
            _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
        }
    }

    private Player CreatePlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GetVerifiedPlayerName()) }
            }
        };
    }

    private string GetVerifiedPlayerName()
    {
        string name = _playerNameField.text;
        if (name == "")
            name = "Player" + Random.Range(10, 99);

        if (char.IsNumber(name[0]))
            name = "Player" + Random.Range(10, 99);

        return name;
    }

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = CreatePlayer(),
            };
            
            _joinedLobby = _createdLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created Lobby " + _createdLobby.Name + " " + _createdLobby.LobbyCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void QuickJoin()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = CreatePlayer(),
            };

            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void JoinByCode(string code)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = CreatePlayer(),           
            };

            _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            Debug.Log("Joined by code " + code);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void FindLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found : " + queryResponse.Results.Count);
            foreach (var result in queryResponse.Results)
            {
                Debug.Log(result.Name + " " + result.LobbyCode + " " + result.Players.Count);
                PrintPlayers();
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    
    private void PrintPlayers()
    {
        if (_joinedLobby == null)
            return;

        foreach(var player in _joinedLobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
}
