using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class Authenticate : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _playerNameField;
    [SerializeField]
    private Button _loginButton;

    [SerializeField] 
    private Transform _menuHolder;

    [SerializeField]
    private JoinServerHandler _joinServerHandler;

    public static Authenticate Instance { get; private set; }

    private Player _player;

    private float _lastUpdateTime = 0;
    private float _updateCD = 1.05f;

    public Player GetPlayer()
    {
        return _player;
    }

    public void ResetReadyStatus()
    {
        _player.Data["Ready"].Value = "False";
    }

    public void SwitchReadyStatus()
    {
        if (_lastUpdateTime + _updateCD > Time.time)
            return;

        if (_player.Data["Ready"].Value == "True")
            _player.Data["Ready"].Value = "False";
        else
            _player.Data["Ready"].Value = "True";

        Lobby.Instance.UpdatePlayerStatus(_player.Data["Ready"].Value);
        _lastUpdateTime = Time.time;
    }

    [SerializeField]
    private MenuWindow _nextWindow;

    private void Awake()
    {
        _loginButton.onClick.AddListener(() => {
            TryAuthenticate();
        });


        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsAuthorized)
            _nextWindow.GoToNextWindow();
        else
            MenuHandler.Instance.AddMenu(_menuHolder);
    }

    private void OnEnable()
    {
        if (_player != null)
            _playerNameField.text = _player.Data["PlayerName"].Value;
    }

    private async void TryAuthenticate()
    {
        try
        {
            _loginButton.interactable = false;
            _player = CreatePlayer();

            _joinServerHandler.SetLocalPlayerName(_player.Data["PlayerName"].Value);

            await UnityServices.InitializeAsync();

            if (AuthenticationService.Instance.IsAuthorized)
            {
                _loginButton.interactable = true;
                _nextWindow.GoToNextWindow();
                return;
            }

            AuthenticationService.Instance.SignedIn += () =>
            {
                _loginButton.interactable = true;
                _nextWindow.GoToNextWindow();
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (LobbyServiceException e)
        {
            _loginButton.interactable = true;
            Debug.Log(e);
        }
    }


    private Player CreatePlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, GetVerifiedPlayerName()) },
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "False" )},
                { "ClientID", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")},
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
}
