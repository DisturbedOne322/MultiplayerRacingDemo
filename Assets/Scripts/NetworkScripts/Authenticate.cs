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

    public static Authenticate Instance { get; private set; }
    private bool _authenticated = false;

    private Player _player;

    public Player GetPlayer() => _player;

    [SerializeField]
    private MenuWindow _nextWindow;

    // Start is called before the first frame update
    void Start()
    {
        _loginButton.onClick.AddListener(() => {
            TryAuthenticate();
        });


        if (Instance == null)
            Instance = this;
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
            _player = CreatePlayer();

            if(_authenticated)
            {
                MenuHandler.Instance.AddMenu(_nextWindow.Get());
                return;
            }

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                _authenticated = true;
                MenuHandler.Instance.AddMenu(_nextWindow.Get());
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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
}
