using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinLobbyByCode : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _joinCode;

    [SerializeField]
    private MenuWindow _menuWindow;

    public void JoinByCode()
    {
        if (_joinCode.text == "")
            return;
        if (_joinCode.text.Length != 6)
            return;

        Lobby.Instance.JoinByCode(_joinCode.text);
        _menuWindow.GoToNextWindow();
    }
}
