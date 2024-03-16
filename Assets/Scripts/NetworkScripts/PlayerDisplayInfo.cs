using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplayInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playerName;
    [SerializeField]
    private TextMeshProUGUI _readyText;
    [SerializeField]
    private Button _kickButton;


    public void SetPlayerInfo(Player player)
    {
        _playerName.text = player.Data["PlayerName"].Value;

        _kickButton.gameObject.SetActive(Lobby.Instance.HostedLobby != null && Lobby.Instance.JoinedLobby.HostId != player.Id);
        if (_kickButton.gameObject.activeSelf)
        {
            _kickButton.onClick.AddListener(() => { Lobby.Instance.KickPlayer(player); });
        }

        if (player.Id == Lobby.Instance.JoinedLobby.HostId)
        {
            _readyText.text = "Host";
            _readyText.color = Color.blue;
            return;
        }

        if (player.Data["Ready"].Value == "True")
        {
            _readyText.text = "Ready";
            _readyText.color = Color.green;
        }
        else
        {
            _readyText.text = "Not ready";
            _readyText.color = Color.red;
        }
    }

    private void OnDestroy()
    {
        _kickButton.onClick.RemoveAllListeners();
    }
}
