using UnityEngine;

public class InteractableCCTV : MonoBehaviour
{
    [Tooltip("Nama terminal CCTV yang muncul saat player melihat object ini")]
    public string terminalName = "CCTV System";

    [Tooltip("Urutan CCTV ke-berapa yang akan terbuka pertama kali saat hack (Mulai dari 0 untuk CCTV pertama, 1 untuk kedua, dst)")]
    public int targetCameraIndex = 0;
}
