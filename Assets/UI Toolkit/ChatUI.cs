using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatUI : NetworkBehaviour
{
    public static bool IsChatOpened;

    [SerializeField]
    private UIDocument _document;

    private TextField _textField;
    private ListView _list;
    private List<string> _messages = new List<string>();
    public int _fontSize = 23;

    private PlayerNetworkInputActions _inputActions;

    private FixedString32Bytes _localPlayerName;

    private void Awake()
    {
        _document.rootVisualElement.style.display = DisplayStyle.None;
    }

    public override void OnNetworkSpawn()
    {
        IsChatOpened = false;
        _localPlayerName = new FixedString32Bytes(GameObject.FindFirstObjectByType<JoinServerHandler>().LocalPlayerName);

        SetupChatList();
        SetupInputField();
        SetupPlayerInput();
        _document.rootVisualElement.style.display = DisplayStyle.Flex;
        _document.rootVisualElement.style.opacity = 0.5f;
    }

    private Label CreateLabel()
    {
        var label = new Label();
        label.style.color = Color.white;
        label.style.fontSize = _fontSize;
        label.style.maxWidth = 420;
        label.style.width = 420;
        label.style.whiteSpace = WhiteSpace.Normal;
        return label;
    }

    private void SetupChatList()
    {
        _list = _document.rootVisualElement.Q<ListView>();

        Func<VisualElement> makeItem = () => CreateLabel();
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = _messages[i];

        int itemHeight = _fontSize + 5;
        _list.itemsSource = _messages;
        _list.makeItem = makeItem;
        _list.bindItem = bindItem;
        _list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _list.SetEnabled(false);
    }
    private void SetupInputField()
    {
        _textField = _document.rootVisualElement.Q<TextField>();
        _textField.maxLength = 64;
        _textField.SetEnabled(false);
    }

    private void SetupPlayerInput()
    {
        _inputActions = new PlayerNetworkInputActions();
        _inputActions.Enable();
        _inputActions.Chat.OpenChat.performed += OpenChat_performed;
    }

    private void OpenChat_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (_textField.enabledSelf)
        {
            if(_textField.value != "")
            {
                SendMessageServerRpc(_localPlayerName, new FixedString128Bytes(_textField.value));
                _textField.value = "";
            }
            _textField.SetEnabled(false);
        }
        else
        {
            _textField.SetEnabled(true);
            _textField.Focus();
        }

        _document.rootVisualElement.style.opacity = _textField.enabledSelf ? 1 : 0.5f;
        IsChatOpened = _textField.enabledSelf;
    }


    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SendMessageServerRpc(FixedString32Bytes playerName, FixedString128Bytes message)
    {
        SendMessageClientRpc(playerName, message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendMessageClientRpc(FixedString32Bytes playerName, FixedString128Bytes message)
    {
        _list.SetEnabled(true);
        _messages.Add(playerName + ": " + message);
        _list.RefreshItems();
    }
}
