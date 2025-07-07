using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance;
    
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;

    [SerializeField] private TMP_Text codeText;

    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string gameplayScene = "BattleField";
    
    private string relayJoinCode;
    private Lobby currentLobby;
    private const int MaxPlayers = 4;
    private bool isHost = false;
    private bool isInitialized = false;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            isInitialized = true;
            Debug.Log("Unity Services initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Services initialization failed: {e.Message}");
            isInitialized = false;
        }
    }

    public async void CreateRoom() 
    {
        if (!isInitialized)
        {
            Debug.LogError("Services not initialized yet");
            return;
        }

        try 
        {
            // 1. Allocate Relay server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 2. Create Lobby with Relay code
            CreateLobbyOptions options = new CreateLobbyOptions 
            {
                Data = new Dictionary<string, DataObject> 
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };
            
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("GameLobby", MaxPlayers, options);

            // 3. Configure network transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );
            }
            else
            {
                Debug.LogError("UnityTransport component not found");
                return;
            }

            // 4. Start host
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost();
                ShowLobbyUI();
                codeText.text = $"Room Code: {relayJoinCode}";
            }
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"CreateRoom failed: {e}");
        }
    }
    
    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        
        // Auto-hide after 3 seconds
        CancelInvoke(nameof(HideError));
        Invoke(nameof(HideError), 3f);
    }

    private void HideError()
    {
        if (errorText != null) errorText.gameObject.SetActive(false);
    }
    public async void JoinRoom()
    {
        try
        {
            // 1. Find lobby by code
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(codeInputField.text);
            relayJoinCode = currentLobby.Data["RelayCode"].Value;

            // 2. Join Relay allocation
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // 3. Configure client transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // 4. Start client
            NetworkManager.Singleton.StartClient();
            ShowLobbyUI();
        }
        catch (LobbyServiceException e) 
        {
            string errorMessage = e.Reason switch
            {
                LobbyExceptionReason.LobbyNotFound => "Room not found",
                LobbyExceptionReason.LobbyFull => "Room is full",
                LobbyExceptionReason.Unauthorized => "Not authorized",
                LobbyExceptionReason.RateLimited => "Too many attempts - try again later",
                _ => $"Failed to join room"
            };
            ShowError(errorMessage);
        }
        catch (RelayServiceException e)
        {
            ShowError($"Relay error: {e.Message}");
        }
        catch (System.Exception e)
        {
            ShowError($"Connection failed: {e.Message}");
        }
    }

    public void StartGame()
    {
        if (!IsHost) return;
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
    }

    public override async void OnDestroy() {
        if (currentLobby != null && IsServer) {
            try {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
            }
            catch (System.Exception e) {
                Debug.LogError($"Failed to delete lobby: {e.Message}");
            }
        }
    }

    private string GenerateRandomCode() {
        return UnityEngine.Random.Range(1000, 9999).ToString(); // Explicitly using UnityEngine.Random
    }

    private void ShowLobbyUI() {
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        
        if (IsHost) {
            codeText.text = $"Room Code: {relayJoinCode}"; // Using relayJoinCode instead of roomCode
        }
    }
}