using UnityEngine;
using TMPro; // Untuk TextMeshPro
using NavKeypad;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera fpsCam;               // Masukkan kamera FPS Player Anda ke sini
    public float interactDistance = 3f; // Jarak maksimal player bisa berinteraksi
    public KeyCode interactKey = KeyCode.E; // Tombol untuk berinteraksi

    [Header("UI References")]
    [Tooltip("Text UI untuk menampilkan hint (contoh: TextMeshProUGUI)")]
    public TextMeshProUGUI hintTextUI;  // Assign di Inspector dengan objek TextMeshPro

    [Header("Audio Settings")]
    [Tooltip("AudioSource untuk memutar efek suara interaksi (bisa ditambahkan komponen AudioSource di GameObject ini)")]
    public AudioSource interactAudioSource;
    [Tooltip("Suara saat menginspeksi item (InspectableItem)")]
    public AudioClip inspectSound;

    void Update()
    {
        // 1. Lakukan Raycast SETIAP FRAME untuk mengecek apa yang dilihat player
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        bool hitInteractable = false;
        string hintMessage = "";

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Cek jenis objek yang dilihat
            if (hit.collider.TryGetComponent(out KeypadButton keypadButton))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Press Button";
                
                if (Input.GetKeyDown(interactKey))
                {
                    keypadButton.PressButton();
                }
            }
            else if (hit.collider.TryGetComponent(out PickableItem pickableItem))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Pickup {pickableItem.itemName}";
                
                if (Input.GetKeyDown(interactKey))
                {
                    // Masukkan ke inventory player
                    PlayerInventory.Instance.PickUpItem(pickableItem);
                    // Hapus objek 3D dari dunia
                    Destroy(hit.collider.gameObject);
                }
            }
            else if (hit.collider.TryGetComponent(out InspectableItem inspectableItem))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Inspect {inspectableItem.itemName}";
                
                if (Input.GetKeyDown(interactKey))
                {
                    // Tampilkan gambar di layar tanpa memasukkan ke inventory
                    PlayerInventory.Instance.ShowInspect(inspectableItem.documentImage);

                    // Mainkan suara inspect
                    if (interactAudioSource != null && inspectSound != null)
                    {
                        interactAudioSource.PlayOneShot(inspectSound);
                    }
                }
            }
            else if (hit.collider.TryGetComponent(out InteractableCCTV cctvTerminal))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Hack {cctvTerminal.terminalName}";

                if (Input.GetKeyDown(interactKey))
                {
                    if (CCTVManager.Instance != null)
                    {
                        CCTVManager.Instance.EnterCCTVMode(cctvTerminal.targetCameraIndex);
                    }
                    else
                    {
                        Debug.LogWarning("CCTVManager tidak ditemukan di scene!");
                    }
                }
            }
        }

        // 2. Tampilkan atau sembunyikan UI Text berdasarkan hasil raycast
        if (hintTextUI != null)
        {
            if (hitInteractable)
            {
                hintTextUI.text = hintMessage;
                hintTextUI.gameObject.SetActive(true);
            }
            else
            {
                // Sembunyikan hint jika tidak melihat objek interactable
                hintTextUI.gameObject.SetActive(false);
            }
        }
    }
}
