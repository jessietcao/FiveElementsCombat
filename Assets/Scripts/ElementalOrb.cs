using Unity.Netcode;
using UnityEngine;

public abstract class ElementalOrb : NetworkBehaviour
{
    public enum ElementType { Metal, Wood, Water, Fire, Earth }
    
    [SerializeField] private ElementType element;
    [SerializeField] private float respawnTime = 10f;
    
    public ElementType Element => element;
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
        if (other.TryGetComponent<PlayerElements>(out var player))
        {
            player.AddElement(element);
            DespawnOrb();
        }
    }

    protected void setElement(ElementType newElement)
    {
        element = newElement;
    }
    
    private void DespawnOrb()
    {
        GetComponent<NetworkObject>().Despawn();
        Invoke(nameof(RespawnOrb), respawnTime);
    }
    
    private void RespawnOrb()
    {
        if (IsServer) GetComponent<NetworkObject>().Spawn();
    }
}