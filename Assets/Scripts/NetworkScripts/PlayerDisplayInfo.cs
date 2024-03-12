using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplayInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playerName;
    [SerializeField]
    private Button _kickButton;


    public void SetPlayerInfo(string playerName, bool host)
    {
        _playerName.text = playerName;
        _kickButton.gameObject.SetActive(host);
    }
}
