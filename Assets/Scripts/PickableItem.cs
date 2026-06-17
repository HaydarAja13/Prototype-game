using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Keycard";
    
    [Tooltip("Ikon yang akan muncul di slot item UI (opsional)")]
    public Sprite itemIcon; 
}
