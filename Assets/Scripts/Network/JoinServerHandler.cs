using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JoinServerHandler : MonoBehaviour
{
    [SerializeField]
    private Button _hostButton;
    [SerializeField]
    private Button _clientButton;

    // Start is called before the first frame update
    void Start()
    {
        _hostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); gameObject.SetActive(false); });
        _clientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); gameObject.SetActive(false); });
      
    }
}
