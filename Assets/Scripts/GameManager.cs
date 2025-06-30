using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    
    void Awake() {
        Instance = this;
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            SpawnPlayers();
        }
    }

    void SpawnPlayers() {
        // Spawn host player
        NetworkObject hostPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
    }
}
