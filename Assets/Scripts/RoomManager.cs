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
    [SerializeField] private string gameplayScene = "Battlefield";
    
    private string roomCode = "";
    private bool isHost = false;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    // Called by Host button
    public void CreateRoom() {
        roomCode = GenerateRandomCode();
        isHost = true;
        
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
        
        ShowLobbyUI();
        Debug.Log($"Room created! Code: {roomCode}");
    }

    // Called by Join button
    public void JoinRoom() {
        roomCode = codeInputField.text;
        isHost = false;
        
        NetworkManager.Singleton.StartClient();
        ShowLobbyUI();
    }

    // Called by Start button (host only)
    public void StartGame() {
        if (!IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplayScene, LoadSceneMode.Single);
    }

    private string GenerateRandomCode() {
        return Random.Range(1000, 9999).ToString();
    }

    private void ShowLobbyUI() {
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        
        if (IsHost) {
            // Show code to host
            lobbyPanel.GetComponentInChildren<TMP_Text>().text = $"Room Code: {roomCode}";
        }
    }
}