using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class NetworkCollisionHandler : NetworkBehaviour
{
    private float _multiplier = 300;

    private Rigidbody _rb;
    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody>();      
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.transform.root.CompareTag("Player"))
            return;

        Vector3 relativeVelocity = collision.relativeVelocity;
        HandleCollisionServerRpc(relativeVelocity);
    }


    [ServerRpc(RequireOwnership = false)]
    private void HandleCollisionServerRpc(Vector3 relativeVelocity)
    {
        HandleCollisionClientRpc(relativeVelocity * _multiplier);
    }

    [ClientRpc]
    private void HandleCollisionClientRpc(Vector3 force)
    {

        _rb.AddForce(-_rb.velocity / 10, ForceMode.Impulse);      
    }
}
