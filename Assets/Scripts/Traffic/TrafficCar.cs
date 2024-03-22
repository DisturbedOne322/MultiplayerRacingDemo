using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class TrafficCar : NetworkBehaviour
{
    private float _multiplier = 10;

    private void OnCollisionEnter(Collision collision)
    {
        if(IsServer)
        {
            if (collision.gameObject.CompareTag("Traffic"))
                OnCollisionServerRPC();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            OnCollisionServerRPC();
            OnCollisionWithPlayerServerRPC(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, collision.relativeVelocity);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnCollisionServerRPC()
    {
        if (!GetComponent<SplineAnimate>().IsPlaying)
            return;

        GetComponent<SplineAnimate>().Pause();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnCollisionWithPlayerServerRPC(ulong collidedClientID, Vector3 relativeVel)
    {
        ReactToCollisionClientRPC(collidedClientID, relativeVel);
    }

    [ClientRpc]
    private void ReactToCollisionClientRPC(ulong testID, Vector3 relatvieVel)
    {
        if(NetworkManager.Singleton.LocalClientId == testID)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Rigidbody>().AddForce(relatvieVel * _multiplier, ForceMode.Impulse);
        }
    }
}
