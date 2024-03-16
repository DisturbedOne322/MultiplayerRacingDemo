using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerInfoPrefab;
    [SerializeField]
    private Transform _contentParent;

    [SerializeField]
    private MenuWindow _menuWindow;

    [SerializeField]
    private TextMeshProUGUI _lobbyName;
    [SerializeField]
    private TextMeshProUGUI _joinCode;
    [SerializeField]
    private TextMeshProUGUI _playersNumText;

    [SerializeField]
    private Button _startGameButton;

    [SerializeField]
    private Button _readyButton;

    private float _pingTimerMax = 15;
    private float _pingTimer = 15;

    private float _updateTimer = 0f;
    private float _updateTimerMax = 2f;

    private void Awake()
    {
        _startGameButton.onClick.AddListener(() => {
            if (!Lobby.Instance.CheckEveryoneReady())
                return;

            _startGameButton.interactable = false;
            Lobby.Instance.StartGame();
        });

        Lobby.Instance.OnKicked += Instance_OnKicked;
    }

    private void Instance_OnKicked()
    {
        _updateTimer = _updateTimerMax;
        _menuWindow.GoToPrevWindow();
    }

    private void OnEnable()
    {
        _updateTimer = 0;
        UpdateLobbyData();
    }


    private void Update()
    {
        PingServer();
        UpdateLobbyData();
    }

    private void DisplayStartGameButtonForHost()
    {
        if (Lobby.Instance.JoinedLobby == null)
            return;

        _startGameButton.gameObject.SetActive(Lobby.Instance.JoinedLobby.HostId == AuthenticationService.Instance.PlayerId);
        _readyButton.gameObject.SetActive(!_startGameButton.gameObject.activeSelf);
    }

    public void FindPlayers()
    {
        List<Player> playersInLobby = Lobby.Instance.JoinedLobby.Players;
        RepopulatePlayerList(playersInLobby);
    }

    private void RepopulatePlayerList(List<Player> playersInLobby)
    {
        ClearLobbyList();
        for (int i = 0; i < playersInLobby.Count; i++)
        {
            GameObject playerInfo = Instantiate(_playerInfoPrefab);
            PlayerDisplayInfo displayInfo = playerInfo.GetComponent<PlayerDisplayInfo>();

            displayInfo.SetPlayerInfo(playersInLobby[i]);

            displayInfo.transform.SetParent(_contentParent, false);
        }
    }

    private void ClearLobbyList()
    {
        if (_contentParent == null)
            return;

        Transform[] children = _contentParent.GetComponentsInChildren<Transform>();
        for (int i = 1; i < children.Length; i++)
        {
            GameObject.Destroy(children[i].gameObject);
        }
    }

    private async void PingServer()
    {
        if (Lobby.Instance.HostedLobby == null)
            return;
        _pingTimer -= Time.unscaledDeltaTime;
        if (_pingTimer < 0)
        {
            _pingTimer = _pingTimerMax;
            Debug.Log("Ping");
            await LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Instance.HostedLobby.Id);
        }
    }

    private async void UpdateLobbyData()
    {
        if (Lobby.Instance.JoinedLobby == null)
            return;

        _updateTimer -= Time.unscaledDeltaTime;
        if (_updateTimer < 0)
        {
            _updateTimer = _updateTimerMax;
            Lobby.Instance.UpdateJoinedLobby(await LobbyService.Instance.GetLobbyAsync(Lobby.Instance.JoinedLobby.Id));

            if (Lobby.Instance.JoinedLobby.Data["GameStarted"].Value == "True")
                return;

            FindPlayers();
            UpdateCode();
            UpdatePlayersNum();
            DisplayStartGameButtonForHost();
            UpdateLobbyName();
        }
    }

    private void UpdateLobbyName()
    {
        _lobbyName.text = Lobby.Instance.JoinedLobby.Name;
    }

    private void UpdateCode()
    {
        _joinCode.text = Lobby.Instance.JoinedLobby.LobbyCode;
    }

    private void UpdatePlayersNum()
    {
        _playersNumText.text = Lobby.Instance.JoinedLobby.Players.Count + " / " + Lobby.Instance.JoinedLobby.MaxPlayers;
    }
}
