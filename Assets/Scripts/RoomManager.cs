using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance;
    
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    public GameObject MenuPanel => menuPanel;
    public GameObject LobbyPanel => lobbyPanel;

    [SerializeField] private TMP_Text CodeText;
    [SerializeField] private string gameplayScene = "BattleField";
    
    private string roomCode = "";
    private bool isHost = false;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void CreateRoom()
    {
        roomCode = GenerateRandomCode();
        isHost = true;
        
        // Add this to ensure we don't load scene until ready
        NetworkManager.Singleton.OnServerStarted += OnHostStarted;
        NetworkManager.Singleton.StartHost();
    }

    private void OnHostStarted()
    {
        NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
        ShowLobbyUI(); // Show lobby before game starts
    }

    public void StartGame()
    {
        if (!IsHost) return;
        
        // Hide UI before transition
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
    }
    // Called by Join button
    public void JoinRoom() {
        roomCode = codeInputField.text;
        isHost = false;
        
        NetworkManager.Singleton.StartClient();
        ShowLobbyUI();
    }


    private string GenerateRandomCode() {
        return Random.Range(1000, 9999).ToString();
    }

    private void ShowLobbyUI() {
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        
        if (IsHost) {
            // Show code to host
            CodeText.text = $"Room Code: {roomCode}";
        }
    }
}