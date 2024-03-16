using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNewLobby : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _lobbyName;
    [SerializeField]
    private MenuWindow _menuWindow;

    [SerializeField]
    private TMP_Dropdown _dropDown;

    [SerializeField]
    private Button _createLobbyButton;

    private Dictionary<int, int> _idToPlayerDict;

    private void Awake()
    {
        _idToPlayerDict = new Dictionary<int, int>()
        {
            {0, 2},
            {1, 4},
            {2, 8},
        };

        _createLobbyButton.onClick.AddListener(() => {
            TryCreateNewLobby();
        });
    }

    private async void TryCreateNewLobby()
    {
        _createLobbyButton.interactable = false;

        if (_lobbyName.text == "")
        {
            bool successDef = await Lobby.Instance.CreateLobbyDefault(_idToPlayerDict[_dropDown.value]);
            _createLobbyButton.interactable = true;
            if (successDef)
                _menuWindow.GoToNextWindow();
            return;
        }

        if (char.IsNumber(_lobbyName.text[0]))
            return;

        bool success = await Lobby.Instance.CreateLobby(_lobbyName.text, _idToPlayerDict[_dropDown.value]);
        _createLobbyButton.interactable = true;
        if (success)
            _menuWindow.GoToNextWindow();
    }
}
