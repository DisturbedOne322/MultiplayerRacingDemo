using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinimapDisplay : MonoBehaviour
{
    [SerializeField]
    private Transform _playerTransform;

    [SerializeField]
    private RectTransform _playerMinimap; 

    // Update is called once per frame
    void Update()
    {
        //_playerMinimap.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(_playerTransform.forward, Vector3.forward, Vector3.up) + 90);
    }
}
