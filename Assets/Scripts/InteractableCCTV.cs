using UnityEngine;

public class InteractableCCTV : MonoBehaviour
{
    [Tooltip("Nama terminal CCTV yang muncul saat player melihat object ini")]
    public string terminalName = "CCTV System";

    [Tooltip("ID Grup/Ruang CCTV (Misal: 'A'). Terminal ini hanya akan mengakses kamera dengan ID yang sama.")]
    public string targetGroupID = "A";

    [Tooltip("Urutan CCTV ke-berapa yang akan terbuka pertama kali saat hack (Mulai dari 0 untuk CCTV pertama di grup ini)")]
    public int targetCameraIndex = 0;
}
