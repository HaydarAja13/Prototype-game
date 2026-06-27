using UnityEngine;

public class UIPopupPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan GameObject Panel (Credit / Control) ke sini")]
    public GameObject popupPanel;

    private void Update()
    {
        // Jika panel sedang aktif dan pemain menekan tombol ESC di keyboard
        if (popupPanel != null && popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
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
        }
    }
}
