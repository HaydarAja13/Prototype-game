using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Keycard";
    
    [Tooltip("Ikon yang akan muncul di slot item UI (opsional)")]
    public Sprite itemIcon; 

    [Header("Floppy Disk Settings")]
    [Tooltip("Video yang akan diputar jika item ini adalah Floppy Disk")]
    public UnityEngine.Video.VideoClip videoClip;

    [Header("Objective Settings (Opsional)")]
    [Tooltip("Centang ini jika mengambil item ini akan menyelesaikan sebuah Objective")]
    public bool completesObjective = false;
    [Tooltip("Index objective yang akan diselesaikan (mulai dari 0 untuk objective pertama)")]
    public int objectiveIndexToComplete = 0;

    [Header("Dialogue/Subtitle Settings (Opsional)")]
    [Tooltip("Suara/Voice yang diputar saat item diambil")]
    public AudioClip voiceClip;
    [TextArea(2,3)]
    [Tooltip("Teks subtitle yang muncul di layar saat item diambil")]
    public string subtitleText;
}
