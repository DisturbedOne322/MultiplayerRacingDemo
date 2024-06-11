using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyByCode : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _joinCode;

    [SerializeField]
    private MenuWindow _menuWindow;

    [SerializeField]
    private Button _joinByCodeButton;

    private void Awake()
    {
        _joinByCodeButton.onClick.AddListener(() => {
            JoinByCode();
        });
    }

    public async void JoinByCode()
    {
        if (_joinCode.text == "" || _joinCode.text.Length != 6)
        {
            Debug.Log("Code must consist of 6 characters");
            return;
        }

        _joinByCodeButton.interactable = false;
        bool success = await Lobby.Instance.JoinByCode(_joinCode.text);
        _joinByCodeButton.interactable = true;
        if(success)
            _menuWindow.GoToNextWindow();
    }
}
