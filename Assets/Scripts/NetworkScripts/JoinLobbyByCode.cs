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

    public void JoinByCode()
    {
        _joinByCodeButton.interactable = false;
        if (_joinCode.text == "")
            return;
        if (_joinCode.text.Length != 6)
            return;

        Lobby.Instance.JoinByCode(_joinCode.text);
        _joinByCodeButton.interactable = true;
        _menuWindow.GoToNextWindow();
    }
}
