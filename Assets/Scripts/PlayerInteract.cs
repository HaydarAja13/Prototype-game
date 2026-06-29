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
        // Jangan proses interaksi saat game sedang dipause
        if (PauseManager.isPaused) return;

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
                
                if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                {
                    keypadButton.PressButton();
                }
            }
            else if (hit.collider.TryGetComponent(out PickableItem pickableItem))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Pickup {pickableItem.itemName}";
                
                if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                {
                    // Masukkan ke inventory player
                    PlayerInventory.Instance.PickUpItem(pickableItem);
                    
                    // Cek apakah item ini menyelesaikan objective
                    if (pickableItem.completesObjective && ObjectiveManager.Instance != null)
                    {
                        ObjectiveManager.Instance.CompleteObjectiveByIndex(pickableItem.objectiveIndexToComplete);
                    }
                    
                    // Mainkan dialog/subtitle jika ada
                    if (DialogueManager.Instance != null && (pickableItem.voiceClip != null || !string.IsNullOrEmpty(pickableItem.subtitleText)))
                    {
                        DialogueManager.Instance.PlayDialogue(pickableItem.voiceClip, pickableItem.subtitleText);
                    }
                    
                    // Hapus objek 3D dari dunia
                    Destroy(hit.collider.gameObject);
                }
            }
            else if (hit.collider.TryGetComponent(out InspectableItem inspectableItem))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Inspect {inspectableItem.itemName}";
                
                if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                {
                    // Tampilkan gambar di layar tanpa memasukkan ke inventory
                    PlayerInventory.Instance.ShowInspect(inspectableItem.documentImage);

                    // Cek apakah menginspeksi item ini menyelesaikan objective
                    if (inspectableItem.completesObjective && ObjectiveManager.Instance != null)
                    {
                        ObjectiveManager.Instance.CompleteObjectiveByIndex(inspectableItem.objectiveIndexToComplete);
                    }

                    // Mainkan dialog/subtitle jika ada
                    if (DialogueManager.Instance != null && (inspectableItem.voiceClip != null || !string.IsNullOrEmpty(inspectableItem.subtitleText)))
                    {
                        DialogueManager.Instance.PlayDialogue(inspectableItem.voiceClip, inspectableItem.subtitleText);
                    }

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

                if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                {
                    if (CCTVManager.Instance != null)
                    {
                        CCTVManager.Instance.EnterCCTVMode(cctvTerminal.targetCameraIndex, cctvTerminal.targetGroupID);
                    }
                    else
                    {
                        Debug.LogWarning("CCTVManager tidak ditemukan di scene!");
                    }
                }
            }
            else if (hit.collider.TryGetComponent(out FlashlightItem flashlightItem))
            {
                hitInteractable = true;
                hintMessage = $"[{interactKey.ToString()}] Take a Flashlight";

                if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                {
                    PlayerFlashlight playerFlashlight = GetComponent<PlayerFlashlight>();
                    if (playerFlashlight == null) playerFlashlight = GetComponentInParent<PlayerFlashlight>();
                    if (playerFlashlight == null) playerFlashlight = FindFirstObjectByType<PlayerFlashlight>();

                    if (playerFlashlight != null)
                    {
                        playerFlashlight.PickUpFlashlight();
                    }

                    // Mainkan dialog/subtitle jika ada
                    if (DialogueManager.Instance != null && (flashlightItem.voiceClip != null || !string.IsNullOrEmpty(flashlightItem.subtitleText)))
                    {
                        DialogueManager.Instance.PlayDialogue(flashlightItem.voiceClip, flashlightItem.subtitleText);
                    }

                    Destroy(hit.collider.gameObject);
                }
            }
            else if (hit.collider.TryGetComponent(out InteractableComputer computer))
            {
                hitInteractable = true;
                
                if (PlayerInventory.Instance != null && PlayerInventory.Instance.hasFloppyDisk)
                {
                    hintMessage = $"[{interactKey.ToString()}] Use Floppy Disk on {computer.computerName}";
                    
                    if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.JoystickButton2))  // PS4: bSquare = button 0
                    {
                        var clip = PlayerInventory.Instance.currentFloppyVideoClip;
                        PlayerInventory.Instance.ConsumeFloppyDisk();
                        computer.PlayVideo(clip);
                    }
                }
                else
                {
                    hintMessage = $"Need Floppy Disk to use {computer.computerName}";
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
