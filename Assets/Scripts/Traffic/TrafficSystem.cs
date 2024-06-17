using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficSystem : NetworkBehaviour
{
    [SerializeField]
    private SplineContainer[] _roadSplines;
    
    [SerializeField]
    private GameObject[] _trafficVehiclePrefabArray;

    private int _maxDistanceToReset = 600;
    private int _resetDistanceMin = 700;
    private int _resetDistanceMax = 1200;

    private float _trafficSpeed = 25;

    private List<TrafficCar> _trafficVehiclePool;
    private int _vehiclesPerType = 2;

    private float _minDistanceBetweenCars = 50;
    private float _maxDistanceBetweenCars = 200;

    private float _currentStartDistance = 0;

    private WaitForSeconds _updateTrafficTime = new WaitForSeconds(5);

    private int _playersNum = 0;
    private Transform[] _playersTransformArray;
    private float[] _playersTimeArray;
    private bool[] _foundRequiredTimeForPlayersArray;


    private float _roadLength;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Initialize();
        StartCoroutine(UpdateTraffic());
    }

    private void FindPlayers()
    {
        var clients = NetworkManager.Singleton.ConnectedClients;

        _playersNum = clients.Count;

        if (_playersTransformArray != null && _playersNum == _playersTransformArray.Length)
        {
            for(int i = 0; i < _playersNum; i++)
                _foundRequiredTimeForPlayersArray[i] = false;

            return;
        }

        _playersTransformArray = new Transform[_playersNum];
        _foundRequiredTimeForPlayersArray = new bool[_playersNum];

        _playersTimeArray = new float[_playersNum];

        int c = 0;
        foreach (var client in clients)
        {
            _playersTransformArray[c] = client.Value.PlayerObject.GetComponent<Transform>();
            c++;
        }
    }

    private void Initialize()
    {
        _trafficVehiclePool = new List<TrafficCar>();
        _roadLength = _roadSplines[1].CalculateLength();

        for (int i = 0; i < _trafficVehiclePrefabArray.Length; i++)
        {
            for (int v = 0; v < _vehiclesPerType; v++)
            {
                GameObject gameObject = Instantiate(_trafficVehiclePrefabArray[i]);
                TrafficCar trafficCar = gameObject.GetComponent<TrafficCar>();

                _currentStartDistance += UnityEngine.Random.Range(_minDistanceBetweenCars, _maxDistanceBetweenCars) * UnityEngine.Random.Range(1, 3);
                _currentStartDistance %= _roadLength;

                trafficCar.Initialize(_roadSplines[UnityEngine.Random.Range(0, _roadSplines.Length)], _trafficSpeed, _currentStartDistance / _roadLength);
                _trafficVehiclePool.Add(trafficCar);
                trafficCar.transform.position = new Vector3(0, 1000 + UnityEngine.Random.Range(0, 1000), 0);
                trafficCar.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    private IEnumerator UpdateTraffic()
    {
        while (true)
        {
            yield return _updateTrafficTime;

            FindPlayers();

            if (_playersTransformArray.Length != 0)
            {
                UpdateTrafficProcess();
            }
        }
    }

    public override void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void UpdateTrafficProcess()
    {
        int trafficCars = _trafficVehiclePool.Count;
        for (int i = 0; i < trafficCars; i++)
        {
            (bool reset, int furthestID) = ShouldResetTraffic(i);

            if (!_foundRequiredTimeForPlayersArray[furthestID])
            {
                _foundRequiredTimeForPlayersArray[furthestID] = true;
                FindPlayerTime(furthestID);
            }

            //reset to near a random player
            if (reset)
            {
                int randomLine = UnityEngine.Random.Range(0, _roadSplines.Length);
                ResetTrafficPosition(_playersTimeArray[furthestID], randomLine, i);
            }
        }
    }

    private (bool, int) ShouldResetTraffic(int i)
    {
        //the amount of players the traffic car is too far away to
        int resetRequestCount = 0;

        float largestDistanceToPlayer = 0;
        int furthestPlayedID = 0;

        for (int p = 0; p < _playersNum; p++)
        {
            float distanceToPlayer = Vector3.Distance(_trafficVehiclePool[i].gameObject.transform.position, _playersTransformArray[p].position);
            if (distanceToPlayer > largestDistanceToPlayer)
            {
                largestDistanceToPlayer = distanceToPlayer;
                furthestPlayedID = p;
            }

            if (distanceToPlayer > _maxDistanceToReset)
                resetRequestCount++;
        }

        return (resetRequestCount == _playersNum, furthestPlayedID);
    }


    private void FindPlayerTime(int playerIndex)
    {
        float3 pos = _playersTransformArray[playerIndex].position;
        SplineUtility.GetNearestPoint(_roadSplines[1].Splines[0], _roadSplines[1].transform.InverseTransformPoint(pos), out float3 temp, out _playersTimeArray[playerIndex]);     
    }

    private void ResetTrafficPosition(float playerTime, int lineIndex, int carIndex)
    {
        float addedDistanceFromNearestPoint = UnityEngine.Random.Range(_resetDistanceMin, _resetDistanceMax);

        //make some spawn before and some after the player
        if (UnityEngine.Random.Range(0, 1f) > 0.5f)
            addedDistanceFromNearestPoint *= -1;

        float newNormTime = playerTime + addedDistanceFromNearestPoint / _roadLength;

        if (newNormTime < 0)
            newNormTime += 1;

        if (newNormTime > 1)
            newNormTime -= 1;

        _trafficVehiclePool[carIndex].SetInterpolationClientRpc(RigidbodyInterpolation.None);
        _trafficVehiclePool[carIndex].ResetTrafficCar(_roadSplines[lineIndex], newNormTime);
        _trafficVehiclePool[carIndex].SetInterpolationClientRpc(RigidbodyInterpolation.Interpolate);
    }
}
