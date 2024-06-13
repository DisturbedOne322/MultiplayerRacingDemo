using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class RaceProgressTracker : NetworkBehaviour
{
    private SplineContainer _raceLayoutSpline;
    private JoinServerHandler _joinServerHandler;

    private float _trackLength;

    private const float TIMER_UPDATE_DELAY = 0.25f;
    private WaitForSeconds _updateProgressTimer = new WaitForSeconds(TIMER_UPDATE_DELAY);


    [SerializeField]
    private PlayerRaceDisplayTable _playerRaceDisplayTable;
    private List<Transform> _playersTransformList;

    private NetworkList<PlayerRaceInfo> _playersDataNetworkList = new NetworkList<PlayerRaceInfo>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    void Start()
    {
        RaceStartHandler.OnRaceStart += RaceStartHandler_OnRaceStart;
    }
    

    private void RaceStartHandler_OnRaceStart(SplineContainer raceLayout)
    {
        _raceLayoutSpline = raceLayout;
        _trackLength = _raceLayoutSpline.CalculateLength();

        _joinServerHandler = GameObject.FindFirstObjectByType<JoinServerHandler>();

        if (NetworkManager.Singleton.IsServer)
            PopulatePlayerInfoDataScructures();

        _playerRaceDisplayTable.Initialize(_playersDataNetworkList);
        StartCoroutine(UpdatePlayersRaceProgress());
    }

    private void PopulatePlayerInfoDataScructures()
    {
        var connectedClientIDs = NetworkManager.Singleton.ConnectedClientsIds;
        _playersTransformList = new();
        _playersDataNetworkList.Clear();
        for (int i = 0; i < connectedClientIDs.Count; i++)
        {
            ulong id = connectedClientIDs[i];
            _playersDataNetworkList.Add(new PlayerRaceInfo(_joinServerHandler.ClientIdToNameDict[id], id, 0));
            _playersTransformList.Add(NetworkManager.Singleton.ConnectedClients[id].PlayerObject.transform);
        }
    }

    public override void OnDestroy()
    {
        _playersDataNetworkList?.Dispose();
        RaceStartHandler.OnRaceStart -= RaceStartHandler_OnRaceStart;
    }

    private IEnumerator UpdatePlayersRaceProgress()
    {
        while (true)
        {
            if(NetworkManager.Singleton.IsServer)
            {
                if (NetworkManager.Singleton.ConnectedClientsIds.Count != _playersTransformList.Count)
                    PopulatePlayerInfoDataScructures();

                for (int i = 0; i < _playersTransformList.Count; i++)
                {
                    float3 pos = _playersTransformList[i].position;

                    SplineUtility.GetNearestPoint(_raceLayoutSpline.Spline, _raceLayoutSpline.transform.InverseTransformPoint(pos), out float3 f3, out float t);
                    PlayerRaceInfo temp = _playersDataNetworkList[i];
                    _playersDataNetworkList[i] = new PlayerRaceInfo(temp.Name, temp.ID, t);
                }
                BubbleSort();
            }

            ulong localID = NetworkManager.Singleton.LocalClientId;
            _playerRaceDisplayTable.UpdatePlayerList(_playersDataNetworkList, localID, _trackLength);

            yield return _updateProgressTimer;
        }
    }

    public struct PlayerRaceInfo : System.IEquatable<PlayerRaceInfo>, INetworkSerializable
    {
        public ulong ID;
        public FixedString32Bytes Name;
        public float Progress;

        public PlayerRaceInfo(FixedString32Bytes name, ulong id, float progress)
        {
            Name = name;
            ID = id;
            Progress = progress;
        }

        public bool Equals(PlayerRaceInfo other)
        {
            return other.ID == ID && other.Name == Name && other.Progress == Progress;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out ID);
                reader.ReadValueSafe(out Name);
                reader.ReadValueSafe(out Progress);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(ID);
                writer.WriteValueSafe(Name);
                writer.WriteValueSafe(Progress);
            }
        }
    }
    
    private void BubbleSort()
    {
        bool sorted = false;

        int playersSize = _playersDataNetworkList.Count - 2;
        while(!sorted)
        {
            int swaps = 0;
            for (int i = 0; i <= playersSize; i++)
            {
                PlayerRaceInfo currentPlayer = _playersDataNetworkList[i];
                PlayerRaceInfo nextPlayer = _playersDataNetworkList[i + 1];
                if (currentPlayer.Progress < nextPlayer.Progress)
                {
                    Swap(i, i + 1);
                    swaps++;
                }
            }

            sorted = swaps == 0;
        }
    }

    private void Swap(int i1, int i2)
    {
        var p1 = _playersDataNetworkList[i1];
        _playersDataNetworkList[i1] = _playersDataNetworkList[i2];
        _playersDataNetworkList[i2] = p1;

        var t1 = _playersTransformList[i1];
        _playersTransformList[i1] = _playersTransformList[i2];
        _playersTransformList[i2] = t1;
    }
}
