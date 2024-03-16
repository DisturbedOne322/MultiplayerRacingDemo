using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class QuickJoinLobby : MonoBehaviour
{
    [SerializeField]
    private MenuWindow _menuWindow;

    [SerializeField]
    private Button _quickJoinButton;

    private void Awake()
    {
        _quickJoinButton.onClick.AddListener(() => {
            QuickJoin();
        });
    }

    public async void QuickJoin()
    {
        try
        {
            _quickJoinButton.interactable = false;
            bool success = await Lobby.Instance.QuickJoin();
            _quickJoinButton.interactable = true;
            if (success)
                _menuWindow.GoToNextWindow();
            else
                Debug.Log("error");
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogException(ex);
            _quickJoinButton.interactable = true;
        }
    }
}
