using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("Masukkan Spotlight yang ada di bawah Player/Camera")]
    public Light flashlightSpotlight; 
    public KeyCode toggleKey = KeyCode.Alpha4; // Tombol 4 untuk toggle

    private bool hasFlashlight = false;
    private bool isFlashlightOn = false;

    void Start()
    {
        // Pastikan senter mati di awal permainan
        if (flashlightSpotlight != null)
        {
            flashlightSpotlight.enabled = false;
            isFlashlightOn = false;
        }
    }

    private bool dpadUpLocker = false;

    void Update()
    {
        // D-Pad Up detection via Axis
        float dpadY = 0f;
        try { dpadY = Input.GetAxisRaw("DPadY"); } catch {} 

        bool dpadUpPressed = dpadY > 0.5f;
        bool toggleTriggered = Input.GetKeyDown(toggleKey);

        if (dpadUpPressed && !dpadUpLocker)
        {
            toggleTriggered = true;
            dpadUpLocker = true;
        }
        else if (!dpadUpPressed)
        {
            dpadUpLocker = false;
        }

        // Hanya bisa di-toggle jika player sudah mengambil senter
        if (hasFlashlight && toggleTriggered)
        {
            ToggleFlashlight();
        }
    }

    public void PickUpFlashlight()
    {
        hasFlashlight = true;
        // Opsional: otomatis nyalakan senter saat pertama kali diambil
        if (flashlightSpotlight != null)
        {
            isFlashlightOn = true;
            flashlightSpotlight.enabled = isFlashlightOn;
        }
    }

    private void ToggleFlashlight()
    {
        if (flashlightSpotlight != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightSpotlight.enabled = isFlashlightOn;
        }
    }
}
