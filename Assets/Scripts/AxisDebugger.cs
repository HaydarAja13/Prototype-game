using UnityEngine;

/// <summary>
/// SCRIPT SEMENTARA - Hapus setelah menemukan axis yang benar!
/// Tempel script ini ke GameObject apapun, lalu Play.
/// Gerakkan analog kanan ke atas/bawah dan lihat axis mana yang berubah di Console.
/// </summary>
public class AxisDebugger : MonoBehaviour
{
    private void Update()
    {
        string output = "";

        // Cek semua axis dari 1 sampai 28
        float xAxis   = Input.GetAxis("Horizontal");
        float yAxis   = Input.GetAxis("Vertical");
        float axis3   = Input.GetAxis("3rd axis (Joysticks and Scrollwheel)") ;
        float mouseX  = Input.GetAxisRaw("Mouse X");
        float mouseY  = Input.GetAxisRaw("Mouse Y");

        // Raw joystick axes
        for (int i = 1; i <= 10; i++)
        {
            float val = Input.GetAxisRaw("joystick axis " + i);
            if (Mathf.Abs(val) > 0.1f)
            {
                Debug.Log($"[AXIS AKTIF] joystick axis {i} = {val:F3}  ← GERAKKAN ANALOG KANAN ATAS/BAWAH");
            }
        }

        // Tampilkan Mouse Y untuk cek apakah terbaca
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            Debug.Log($"[Mouse Y] = {mouseY:F3}");
        }
    }
}
