using Unity.Netcode;
using System.Collections.Generic;

public class PlayerElements : NetworkBehaviour
{
    private Dictionary<ElementalOrb.ElementType, int> elements = new();
    private int highestElementCount = 0;

    public void AddElement(ElementalOrb.ElementType type)
    {
        if (!IsOwner) return;

        if (!elements.ContainsKey(type)) elements[type] = 0;
        elements[type]++;

        if (elements[type] > highestElementCount)
        {
            highestElementCount = elements[type];
            // When highest element count is shared between multiple elements, the newst one is used
        }

        // ApplyElementEffects(type);
    }
    
    // private void ApplyElementEffects(ElementalOrb.ElementType type)
    // {
    //     switch (type)
    //     {
    //         case ElementalOrb.ElementType.Metal:
    //             // Metal power logic
    //             break;
    //             // Other elements...
    //         case ElementalOrb.ElementType.Wood:
    //     }
    // }
}