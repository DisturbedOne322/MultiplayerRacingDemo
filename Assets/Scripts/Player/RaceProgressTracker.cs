using Assets.VehicleController;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class RaceProgressTracker : MonoBehaviour
{
    private SplineContainer _raceLayoutSpline;
    private JoinServerHandler _joinServerHandler;

    private const float TIMER_UPDATE_DELAY = 0.25f;
    private WaitForSeconds _updateProgressTimer = new WaitForSeconds(TIMER_UPDATE_DELAY);

    private List<PlayerDisplayInfo> _playerDisplayInfoList;
    private List<Transform> _playersTransformList;

    // Start is called before the first frame update
    void Start()
    {
        RaceStartHandler.OnRaceStart += RaceStartHandler_OnRaceStart;
    }

    private void RaceStartHandler_OnRaceStart(SplineContainer raceLayout)
    {
        _joinServerHandler = GameObject.FindFirstObjectByType<JoinServerHandler>();

        _raceLayoutSpline = raceLayout;

        _playerDisplayInfoList = new List<PlayerDisplayInfo>();
        _playersTransformList = new List<Transform>();

        var connectedClientIDs = NetworkManager.Singleton.ConnectedClientsIds;

        for (int i = 0; i < connectedClientIDs.Count; i++)
        {
            ulong id = connectedClientIDs[i];
            _playerDisplayInfoList.Add(new PlayerDisplayInfo(_joinServerHandler.ClientIdToNameDict[id], id));
            _playersTransformList.Add(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform);
        }

        StartCoroutine(UpdateProgress());
    }

    private IEnumerator UpdateProgress()
    {
        while (true)
        {
            for(int i = 0; i < _playersTransformList.Count; i++)
            {
                float3 pos = _playersTransformList[i].position;

                SplineUtility.GetNearestPoint(_raceLayoutSpline.Spline, _raceLayoutSpline.transform.InverseTransformPoint(pos), out float3 temp, out float t);
                _playerDisplayInfoList[i].UpdateProgress(t);
            }

            yield return _updateProgressTimer;
        }
    }

    [Serializable]
    private class PlayerDisplayInfo
    {
        private string _name;
        private ulong _id;
        private float _progress = 0;

        public PlayerDisplayInfo(string name, ulong id)
        {
            _name = name;
            _id = id;
        }

        public void UpdateProgress(float progress) => _progress = progress;
    }
}
