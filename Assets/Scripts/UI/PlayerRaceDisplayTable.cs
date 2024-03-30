using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static RaceProgressTracker;

public class PlayerRaceDisplayTable : MonoBehaviour
{
    [SerializeField]
    private GameObject _playerDisplayInfoPrefab;
    [SerializeField]
    private Transform _playersHolder;


    [SerializeField]
    private PlayerProgressDispaly _playerProgressDispaly;

    private List<PlayerRaceDisplayInfo> _playerDisplayInfoList;

    public void Initialize(NetworkList<PlayerRaceInfo> playerRaceInfoList)
    {
        if(_playerDisplayInfoList != null)
        {
            _playerDisplayInfoList.Clear();
            Transform[] children = _playersHolder.GetComponentsInChildren<Transform>();
            for(int i = 1; i < children.Length; i++) 
                Destroy(children[i]);
        }

        _playerDisplayInfoList = new List<PlayerRaceDisplayInfo>();

        for (int i = 0; i < playerRaceInfoList.Count; i++)
        {
            GameObject go = Instantiate(_playerDisplayInfoPrefab);
            var info = go.GetComponent<PlayerRaceDisplayInfo>();

            info.Initialize(i, playerRaceInfoList[i].Name);

            _playerDisplayInfoList.Add(info);

            go.transform.SetParent(_playersHolder, false);
        }
    }

    public void UpdatePlayerList(NetworkList<PlayerRaceInfo> sortedPlayerRaceInfoList, ulong localPlayerID, float trackLength)
    {
        int playerCount = sortedPlayerRaceInfoList.Count;
        if (playerCount !=  _playerDisplayInfoList.Count)
        {
            Initialize(sortedPlayerRaceInfoList);
        }

        float localProgress = 0;
        foreach(var player in sortedPlayerRaceInfoList)
        {
            if (player.ID == localPlayerID)
            {
                localProgress = player.Progress;
                break;
            }
        }

        _playerProgressDispaly.UpdateProgress(localProgress);

        for (int i = 0; i < playerCount; i++)
        {
            _playerDisplayInfoList[i].UpdatePosition(sortedPlayerRaceInfoList[i].Name);
            if (localPlayerID != sortedPlayerRaceInfoList[i].ID)
            {
                float distance = (localProgress - sortedPlayerRaceInfoList[i].Progress) * trackLength;
                _playerDisplayInfoList[i].UpdateDistance(distance);
            }
            else
                _playerDisplayInfoList[i].HideDistance();
        }
    }
}
