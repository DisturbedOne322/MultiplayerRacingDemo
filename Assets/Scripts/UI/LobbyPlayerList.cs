using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyPlayerList : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerInfoPrefab;
    [SerializeField]
    private Transform _contentParent;

    [SerializeField]
    private TextMeshProUGUI _joinCode;
    [SerializeField]
    private TextMeshProUGUI _playersNumText;

    private float _pingTimerMax = 15;
    private float _pingTimer = 15;

    private float _updateTimer = 0f;
    private float _updateTimerMax = 2f;

    private void OnEnable()
    {
        UpdateLobbyData();
    }

    private void Update()
    {
        PingServer();
        UpdateLobbyData();
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

            displayInfo.SetPlayerInfo(playersInLobby[i].Data["PlayerName"].Value, false);

            displayInfo.transform.SetParent(_contentParent);
        }
    }

    private void ClearLobbyList()
    {
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
            FindPlayers();
            UpdateCode();
            UpdatePlayersNum();
        }
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
