using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;

public class QuickJoinLobby : MonoBehaviour
{
    [SerializeField]
    private MenuWindow _menuWindow;

    public void QuickJoin()
    {
        try
        {
            Lobby.Instance.QuickJoin();
            _menuWindow.GoToNextWindow();
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }
}
