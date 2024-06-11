using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
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
    private MenuWindow _nextWindow;

    private void Awake()
    {
        _loginButton.onClick.AddListener(() => {
            TryAuthenticate();
        });
    }

    private void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsAuthorized)
        {
            MenuHandler.Instance.AddMenu(_menuHolder);
            MenuHandler.Instance.SetFirstMenu(_nextWindow.Get());
            _nextWindow.GoToNextWindow();
        }
        else
        {
            MenuHandler.Instance.AddMenu(_menuHolder);
        }

        if (PlayerData.Instance.GetPlayer() != null)
            _playerNameField.text = PlayerData.Instance.GetPlayer().Data["PlayerName"].Value;
    }

    private async void TryAuthenticate()
    {
        try
        {
            _loginButton.interactable = false;

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
            PlayerData.Instance.UpdatePlayerName(GetVerifiedPlayerName());
        }
        catch (LobbyServiceException e)
        {
            _loginButton.interactable = true;
            Debug.Log(e);
        }
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
