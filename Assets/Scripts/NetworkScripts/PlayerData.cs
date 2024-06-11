using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private Player _player;

    public static PlayerData Instance;

    [SerializeField]
    private JoinServerHandler _joinServerHandler;

    private float _lastUpdateTime = 0;
    private float _updateCD = 1.05f;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public void ResetReadyStatus()
    {
        _player.Data["Ready"].Value = "False";
    }

    public void SwitchReadyStatus()
    {
        if (_lastUpdateTime + _updateCD > Time.time)
            return;

        if (_player.Data["Ready"].Value == "True")
            _player.Data["Ready"].Value = "False";
        else
            _player.Data["Ready"].Value = "True";

        Lobby.Instance.UpdatePlayerStatus(_player.Data["Ready"].Value);
        _lastUpdateTime = Time.time;
    }

    public void UpdatePlayerName(string name)
    {
        _player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name)},
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "False" )},
            }
        };
        _joinServerHandler.SetLocalPlayerName(_player.Data["PlayerName"].Value);
    }
}
