using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    // Singleton agar mudah diakses dari script lain (seperti PlayerInteract)
    public static PlayerInventory Instance;

    [Header("UI References")]
    [Tooltip("Panel atau Game Object yang berisi slot item (Keycard)")]
    public GameObject itemSlotUI; 
    public Image itemIconUI;

    [Tooltip("Panel atau Game Object yang berisi slot item (Floppy Disk)")]
    public GameObject floppySlotUI; 
    public Image floppyIconUI;

    [Header("Inspect UI References")]
    [Tooltip("Panel layar penuh (latar belakang gelap) untuk Inspect")]
    public GameObject inspectPanelUI;
    [Tooltip("Image UI besar di tengah layar untuk gambar HD")]
    public Image inspectImageUI;

    [Header("Current Item State")]
    public bool hasItem = false; // For Keycard
    public string currentItemName;
    
    public bool hasFloppyDisk = false;
    public UnityEngine.Video.VideoClip currentFloppyVideoClip;
    private bool isInspecting = false;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Sembunyikan UI saat game mulai
        if (itemSlotUI != null) itemSlotUI.SetActive(false);
        if (floppySlotUI != null) floppySlotUI.SetActive(false);
        if (inspectPanelUI != null) inspectPanelUI.SetActive(false);
    }

    private void Update()
    {
        // Jika kita memiliki item dan menekan I, buka/tutup inspect
        if (hasItem && Input.GetKeyDown(KeyCode.I))
        {
            ToggleInspect();
        }

        // Jika sedang inspect dan menekan Escape, tutup inspect
        if (isInspecting && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInspect();
        }
    }

    private void ToggleInspect()
    {
        if (!isInspecting)
        {
            if (itemIconUI != null && itemIconUI.sprite != null)
            {
                ShowInspect(itemIconUI.sprite);
            }
        }
        else
        {
            CloseInspect();
        }
    }

    // Fungsi ini bisa dipanggil langsung dari luar untuk meng-inspect dokumen/gambar tertentu
    public void ShowInspect(Sprite imageToShow)
    {
        isInspecting = true;

        if (inspectPanelUI != null)
        {
            inspectPanelUI.SetActive(true);
            
            if (inspectImageUI != null && imageToShow != null)
            {
                inspectImageUI.sprite = imageToShow;
                // Paksa agar rasio asli gambar tetap dipertahankan (tidak terpotong/melar)
                inspectImageUI.preserveAspect = true;
            }
        }
    }

    private void CloseInspect()
    {
        isInspecting = false;
        if (inspectPanelUI != null) inspectPanelUI.SetActive(false);
    }

    // Fungsi ini dipanggil dari PlayerInteract saat mengambil PickableItem
    public void PickUpItem(PickableItem item)
    {
        string lowerName = item.itemName.ToLower();
        
        if (lowerName.Contains("floppy") || lowerName.Contains("disk"))
        {
            hasFloppyDisk = true;
            currentFloppyVideoClip = item.videoClip;
            if (floppySlotUI != null)
            {
                floppySlotUI.SetActive(true);
                
                if (floppyIconUI != null && item.itemIcon != null) 
                {
                    floppyIconUI.sprite = item.itemIcon;
                    floppyIconUI.gameObject.SetActive(true);
                }
                else if (floppyIconUI != null)
                {
                    floppyIconUI.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            hasItem = true;
            currentItemName = item.itemName;

            // Tampilkan UI Slot
            if (itemSlotUI != null)
            {
                itemSlotUI.SetActive(true);
                
                if (itemIconUI != null && item.itemIcon != null) 
                {
                    itemIconUI.sprite = item.itemIcon;
                    itemIconUI.gameObject.SetActive(true);
                }
                else if (itemIconUI != null)
                {
                    itemIconUI.gameObject.SetActive(false);
                }
            }
        }
    }

    // Fungsi ini dipanggil dari Unity Event (Keypad) saat "On Access Granted"
    public void ConsumeItem()
    {
        hasItem = false;
        currentItemName = "";
        CloseInspect(); // Pastikan layar inspect tertutup jika item hilang

        // Sembunyikan UI Slot Keycard
        if (itemSlotUI != null)
        {
            itemSlotUI.SetActive(false);
        }
    }

    // Fungsi ini dipanggil saat menggunakan floppy disk di komputer
    public void ConsumeFloppyDisk()
    {
        hasFloppyDisk = false;
        currentFloppyVideoClip = null;
        
        // Sembunyikan UI Slot Floppy Disk
        if (floppySlotUI != null)
        {
            floppySlotUI.SetActive(false);
        }
    }
}
