using UnityEngine;

public class FlashlightItem : MonoBehaviour
{
    // Script ini berfungsi sebagai penanda bahwa object ini adalah senter yang bisa diambil
    
    [Header("Dialogue Settings (Optional)")]
    [Tooltip("Suara dialog yang akan diputar saat item ini diambil (Boleh dikosongkan)")]
    public AudioClip voiceClip;
    [Tooltip("Teks subtitle yang akan muncul saat item ini diambil (Boleh dikosongkan)")]
    [TextArea]
    public string subtitleText;
}
