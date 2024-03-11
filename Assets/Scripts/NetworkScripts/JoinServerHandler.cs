using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class JoinServerHandler : MonoBehaviour
{
    private UnityTransport _unityTransport;

    private const int MAX_PLAYERS = 4;

    [SerializeField]
    private Button _hostButton;
    [SerializeField]
    private Button _clientButton;

    [SerializeField]
    private TMP_InputField _inputField;
    [SerializeField]
    private TextMeshProUGUI _joinCode;

    private async void Awake()
    {
        _unityTransport = GameObject.FindObjectOfType<UnityTransport>();

        EnableButtons(false);

         await Authenticate();

        EnableButtons(true);
    }

    private void EnableButtons(bool enable)
    {
        _hostButton.gameObject.SetActive(enable);
        _clientButton.gameObject.SetActive(enable);
    }

    private async void CreateGame()
    {
        EnableButtons(false);

        Allocation a = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
        _joinCode.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _unityTransport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        NetworkManager.Singleton.StartHost();
    }

    private async void JoinGame()
    {
        if (_inputField.text == "")
            return;

        EnableButtons(false);
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(_inputField.text);

        _unityTransport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Start is called before the first frame update
    void Start()
    {
        _hostButton.onClick.AddListener(() => { CreateGame(); });
        _clientButton.onClick.AddListener(() => { JoinGame(); });
    }
}
