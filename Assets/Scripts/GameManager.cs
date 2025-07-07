using System.Collections.Generic; // Add this line
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public GameObject playerPrefab; // Assign in Inspector
    [SerializeField] private string gameplayScene = "BattleField"; // Set this to your gameplay scene name

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
        }
        else 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }

    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && NetworkManager.Singleton.SceneManager != null)
        {
         //   NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

 
    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
    if (sceneName == gameplayScene && IsServer) {
        // Clean up lobby UI when game starts
        if (RoomManager.Instance != null)
            Destroy(RoomManager.Instance.gameObject);
            
        SpawnPlayers(); // Spawn all connected players
    }
}

    private void SpawnPlayers()
    {
        if (!IsServer) return;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    // private void OnClientConnected(ulong clientId)
    // {
    //     // if (IsServer && SceneManager.GetActiveScene().name == gameplayScene)
    //     // {
    //     //     SpawnPlayer(clientId);
    //     // }
    // }

    private void SpawnPlayer(ulong clientId) {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId); // Network spawn
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
           // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            if (NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
        base.OnDestroy();
    }
}