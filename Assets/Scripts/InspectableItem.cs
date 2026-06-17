using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Document";
    
    [Tooltip("Gambar dokumen yang akan muncul di layar penuh saat di-inspect")]
    public Sprite documentImage; 
}
