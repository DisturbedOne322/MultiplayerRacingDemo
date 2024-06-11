using TMPro;
using Unity.Collections;
using UnityEngine;

public class ChatMessageTemplate : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _messageInfo;


    public void Initialize(FixedString32Bytes playerName, FixedString128Bytes playerMessage)
    {
        _messageInfo.text = playerName.ToString() + ": " + playerMessage.ToString();
    }
}
