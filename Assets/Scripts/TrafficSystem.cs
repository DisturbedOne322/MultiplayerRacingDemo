using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficSystem : NetworkBehaviour
{
    [SerializeField]
    private SplineContainer _splineContainer;
    

    [SerializeField]
    private GameObject[] _trafficVehiclePrefabArray;

    private int _maxDistanceToReset = 600;

    [SerializeField]
    private float _trafficSpeed = 45;

    private List<SplineAnimate> _trafficVehiclePool;
    private int _minVehiclesPerType = 1;
    private int _maxVehiclesPerType = 3;

    private float _minDistanceBetweenCars = 30;
    private float _maxDistanceBetweenCars = 100;

    private float _currentStartDistance = 0;

    private WaitForSeconds _updateTrafficTime = new WaitForSeconds(5);

    private int _playersNum = 0;
    private Transform[] _playersPosArray;
    private float[] _playerTime;

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

        _playersPosArray = new Transform[_playersNum];
        _playerTime = new float[_playersNum];

        int i = 0;
        foreach (var client in clients)
        {
            _playersPosArray[i] = client.Value.PlayerObject.GetComponent<Transform>();
            i++;
        }
    }

    private void Initialize()
    {
        _trafficVehiclePool = new List<SplineAnimate>();

        _roadLength = _splineContainer.CalculateLength();

        for (int i = 0; i < _trafficVehiclePrefabArray.Length; i++)
        {
            int vehicleAmountForType = UnityEngine.Random.Range(_minVehiclesPerType, _maxVehiclesPerType + 1);

            for (int v = 0; v < vehicleAmountForType; v++)
            {
                GameObject trafficCar = Instantiate(_trafficVehiclePrefabArray[i]);

                SplineAnimate splineAnimate = trafficCar.AddComponent<SplineAnimate>();
                splineAnimate.Container = _splineContainer;

                splineAnimate.ObjectUpAxis = SplineComponent.AlignAxis.ZAxis;
                splineAnimate.ObjectForwardAxis = SplineComponent.AlignAxis.NegativeYAxis;

                splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
                splineAnimate.MaxSpeed = _trafficSpeed;

                _currentStartDistance += UnityEngine.Random.Range(_minDistanceBetweenCars, _maxDistanceBetweenCars);
                splineAnimate.StartOffset = _currentStartDistance / _roadLength;
                _trafficVehiclePool.Add(splineAnimate);

                trafficCar.GetComponent<NetworkObject>().Spawn();
            }
        }
    }


    private IEnumerator UpdateTraffic()
    {
        while(true)
        {
            yield return _updateTrafficTime;

            FindPlayers();

            if (_playersPosArray.Length != 0)
            {
                FindPlayersT();

                int count = _trafficVehiclePool.Count;
                for (int i = 0; i < count; i++)
                {
                    //the amount of players the traffic car is too far away to
                    int resetRequestCount = 0;
                    for(int p = 0; p < _playersNum; p++)
                    {
                        float distanceToPlayer = Vector3.Distance(_trafficVehiclePool[i].gameObject.transform.position, _playersPosArray[p].position);

                        if (distanceToPlayer > _maxDistanceToReset)
                            resetRequestCount++;
                    }

                    //reset to near a random player
                    if(resetRequestCount == _playersNum)
                        ResetTrafficPosition(_playerTime[UnityEngine.Random.Range(0, _playersNum)], i);
                }
            }
        }
    }

    private void FindPlayersT()
    {
        for(int i = 0; i < _playersNum; i++)
        {
            float3 pos = _playersPosArray[i].position;
            float3 temp;
            SplineUtility.GetNearestPoint(_splineContainer.Splines[0], pos, out temp, out _playerTime[i]);
        }
    }

    private void ResetTrafficPosition(float playetT, int i)
    {
        float addedDistanceFromNearestPoint = _maxDistanceToReset + UnityEngine.Random.Range(_minDistanceBetweenCars, _maxDistanceBetweenCars);

        //make some spawn before and some after the player
        if (i % 2 == 0)
            addedDistanceFromNearestPoint *= -1;

        float newNormTime = (playetT + addedDistanceFromNearestPoint / _roadLength) % 1;

        if (newNormTime < 0)
            newNormTime = newNormTime + 1;

        _trafficVehiclePool[i].NormalizedTime = newNormTime;
    }
}
