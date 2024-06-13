using Assets.VehicleController;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

    public override void OnNetworkSpawn()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && IsOwner)
        {
            RaceStartHandler raceStartHandler = GameObject.FindFirstObjectByType<RaceStartHandler>();
            rb.position = raceStartHandler.SpawnPos;
            rb.rotation = raceStartHandler.SpawnRot;
        }
        base.OnNetworkSpawn();
    }
}
