using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Document";
    
    [Tooltip("Gambar dokumen yang akan muncul di layar penuh saat di-inspect")]
    public Sprite documentImage; 

    [Header("Objective Settings (Opsional)")]
    [Tooltip("Centang ini jika meng-inspect item ini akan menyelesaikan sebuah Objective")]
    public bool completesObjective = false;
    [Tooltip("Index objective yang akan diselesaikan (mulai dari 0 untuk objective pertama)")]
    public int objectiveIndexToComplete = 0;

    [Header("Dialogue/Subtitle Settings (Opsional)")]
    [Tooltip("Suara/Voice yang diputar saat item di-inspect")]
    public AudioClip voiceClip;
    [TextArea(2,3)]
    [Tooltip("Teks subtitle yang muncul di layar saat item di-inspect")]
    public string subtitleText;
}
