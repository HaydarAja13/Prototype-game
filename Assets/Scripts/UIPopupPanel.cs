using UnityEngine;
using UnityEngine.EventSystems; // Diperlukan untuk sistem UI navigasi

public class UIPopupPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan GameObject Panel (Credit / Control) ke sini")]
    public GameObject popupPanel;

    [Header("UI Navigation")]
    [Tooltip("Tombol pertama yang otomatis terpilih saat panel ini muncul (misal: tombol Back)")]
    public GameObject firstSelectedButton;

    // Untuk menyimpan tombol apa yang terakhir dipilih sebelum panel ini terbuka
    private GameObject previousSelectedButton;

    private void Update()
    {
        // Jika panel sedang aktif
        if (popupPanel != null && popupPanel.activeSelf)
        {
            // 1. Jika tidak ada UI yang terpilih (misal karena terklik area kosong) 
            // dan pemain menggerakkan analog/d-pad, otomatis pilih tombol pertama lagi
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
                {
                    if (firstSelectedButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                    }
                }
            }

            // 2. Kembali / Back menggunakan tombol Y (JoystickButton3) pada Xbox Controller atau Escape
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                HidePanel();
            }
        }
    }

    /// <summary>
    /// Panggil fungsi ini pada Event OnClick() dari UI Button di Inspector 
    /// untuk memunculkan panel.
    /// </summary>
    public void ShowPanel()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);

            // Simpan tombol yang sedang terpilih sebelum panel terbuka (misal tombol di Main Menu)
            if (EventSystem.current != null)
            {
                previousSelectedButton = EventSystem.current.currentSelectedGameObject;
                
                // Bersihkan seleksi saat ini dan pilih tombol pertama di panel ini
                EventSystem.current.SetSelectedGameObject(null);
                if (firstSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                }
            }
        }
    }

    /// <summary>
    /// Fungsi untuk menyembunyikan panel. 
    /// Bisa juga dipanggil dari Event OnClick() tombol "Close" / "X" pada panel.
    /// </summary>
    public void HidePanel()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);

            // Kembalikan seleksi ke tombol sebelumnya
            if (EventSystem.current != null && previousSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(previousSelectedButton);
            }
        }
    }
}
