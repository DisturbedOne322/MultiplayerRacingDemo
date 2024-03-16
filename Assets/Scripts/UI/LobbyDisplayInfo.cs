using System;
using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDisplayInfo : MonoBehaviour
{
    public static event Action OnJoinedLobby;
    [SerializeField]
    private TextMeshProUGUI _lobbyNameText;
    [SerializeField]
    private TextMeshProUGUI _playersInLobbyText;
    

    [SerializeField]
    private Button _joinLobbyButton;

    private string _joinID;

    public void SetLobbyInfo(string lobbyName, int playersInLobby, int playersMax, string joinID)
    {
        _lobbyNameText.text = lobbyName;
        _playersInLobbyText.text = playersInLobby + " / " + playersMax;
        _joinID = joinID;

        _joinLobbyButton.onClick.AddListener(() => { JoinLobbyByID(_joinID); });
    }

    
    private async void JoinLobbyByID(string id)
    {
        try
        {
            bool success = await Lobby.Instance.JoinByID(id);
             if(success)
                OnJoinedLobby?.Invoke();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
