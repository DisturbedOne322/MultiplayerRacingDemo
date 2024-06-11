using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatSystem : NetworkBehaviour
{
    [SerializeField]
    private ChatMessageTemplate _chatMessagePrefab;

    [SerializeField]
    private ScrollRect _scrollRect;

    [SerializeField]
    private Transform _chatParentGO;

    [SerializeField]
    private TMP_InputField _messageInputField;

    private PlayerNetworkInputActions _inputActions;

    private FixedString32Bytes _localPlayerName;

    private bool _chatOpened = false;

    public override void OnNetworkSpawn()
    {
        _localPlayerName = new FixedString32Bytes(GameObject.FindFirstObjectByType<JoinServerHandler>().LocalPlayerName);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActions = new PlayerNetworkInputActions();
        _inputActions.Enable();

        _messageInputField.characterLimit = 64;

        _inputActions.Chat.OpenChat.performed += _ =>
        {
            if (_chatOpened)
            {
                if(_messageInputField.text != "")
                {
                    SendMessageServerRpc(_localPlayerName, new FixedString128Bytes(_messageInputField.text));
                    _messageInputField.text = "";
                }
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                _messageInputField.Select();
                _messageInputField.ActivateInputField();
            }
            _chatOpened = !_chatOpened;
        };
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SendMessageServerRpc(FixedString32Bytes playerName, FixedString128Bytes message)
    {
        SendMessageClientRpc(playerName, message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendMessageClientRpc(FixedString32Bytes playerName, FixedString128Bytes message)
    {
        ChatMessageTemplate newMessage = Instantiate(_chatMessagePrefab).GetComponent<ChatMessageTemplate>();
        newMessage.Initialize(playerName, message);

        newMessage.transform.SetParent(_chatParentGO, false);
        if (EventSystem.current.currentSelectedGameObject == _scrollRect.verticalScrollbar)
            return;
        StartCoroutine(ApplyScrollPosition(_scrollRect, 0));
    }

    private IEnumerator ApplyScrollPosition(ScrollRect sr, float verticalPos)
    {
        yield return new WaitForEndOfFrame();
        sr.verticalNormalizedPosition = verticalPos;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform);
    }
}
