using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLobby = Unity.Services.Lobbies.Models.Lobby;

public class LobbyListBrowser : MonoBehaviour
{
    [SerializeField]
    private GameObject _lobbyInfoPrefab;

    [SerializeField]
    private Transform _contentParent;

    [SerializeField]
    private MenuWindow _menuWindow;

    private float _lastRefreshTime= - 1;

    private void Start()
    {
        LobbyDisplayInfo.OnJoinedLobby += LobbyDisplayInfo_OnJoinedLobby;
    }
    private void OnDestroy()
    {
        LobbyDisplayInfo.OnJoinedLobby -= LobbyDisplayInfo_OnJoinedLobby;
    }

    private void LobbyDisplayInfo_OnJoinedLobby()
    {
        _menuWindow.GoToNextWindow();
    }

    // Update is called once per frame
    void Update()
    {
        if(_lastRefreshTime + 5 > Time.time)
            FindLobbies();
    }

    private void OnEnable()
    {
        FindLobbies();
    }

    public async void FindLobbies()
    {
        if (_lastRefreshTime + 1.1f > Time.time)
            return;

        _lastRefreshTime = Time.time;
        List<MyLobby> foundLobbies = await Lobby.Instance.FindLobbies();
        RepopulateLobbyList(foundLobbies);
    }

    private void RepopulateLobbyList(List<MyLobby> foundLobbies)
    {
        ClearLobbyList();
        for(int i = 0; i < foundLobbies.Count; i++)
        {
            GameObject lobbyInfo = Instantiate(_lobbyInfoPrefab);
            LobbyDisplayInfo displayInfo = lobbyInfo.GetComponent<LobbyDisplayInfo>();
            displayInfo.SetLobbyInfo(foundLobbies[i].Name, foundLobbies[i].Players.Count, foundLobbies[i].MaxPlayers, foundLobbies[i].Id);

            lobbyInfo.transform.SetParent(_contentParent);
        }
    }

    private void ClearLobbyList()
    {
        Transform[] children = _contentParent.GetComponentsInChildren<Transform>();
        for(int i = 1; i < children.Length; i++)
        {
            GameObject.Destroy(children[i].gameObject);
        }
    }
}
