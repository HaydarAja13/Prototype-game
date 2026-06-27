// WeaponSwitcher.cs
// This script allows the player to switch between weapon objects using the number keys (1, 2, ...).
// Attach this script to a suitable GameObject (e.g., the Player or a dedicated WeaponManager).
// Ensure that the weapon GameObjects are assigned in the Inspector and that each weapon has the WeaponRotation script attached.

using UnityEngine;
using UnityEngine.UI; // Diperlukan untuk mengakses komponen Image di Canvas

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapon References")]
    // Assign your weapon GameObjects (e.g., prefabs or child objects) in the Inspector.
    // The order corresponds to the number keys (1 => index 0, 2 => index 1, etc.).
    public GameObject[] weapons;

    [Header("UI Weapon Icon")]
    public Image weaponIconUI;     // Tarik objek UI Image tempat ikon senjata akan ditampilkan
    public Sprite[] weaponIcons;   // Daftar gambar (Sprite) senjata. Urutannya HARUS sama dengan daftar weapons di atas

    // Currently active weapon index
    private int currentWeapon = -1;

    void Start()
    {
        // Ensure at least one weapon is assigned
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("WeaponSwitcher: No weapons assigned.");
            return;
        }
        // Deactivate all weapons first to avoid multiple active at start
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(false);
        }
        // Activate the first weapon by default
        SwitchWeapon(0);
    }

    void Update()
    {
        if (PauseManager.isPaused) return;
        // Detect number key presses for weapon selection
        for (int i = 0; i < weapons.Length && i < 9; i++) // support up to key 9
        {
            // KeyCode.Alpha1 corresponds to "1", etc.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchWeapon(i);
            }
        }

        // Mouse ScrollWheel weapon selection
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            // Scroll ke atas -> senjata selanjutnya
            int nextWeapon = currentWeapon + 1;
            if (nextWeapon >= weapons.Length) nextWeapon = 0;
            SwitchWeapon(nextWeapon);
        }
        else if (scroll < 0f)
        {
            // Scroll ke bawah -> senjata sebelumnya
            int prevWeapon = currentWeapon - 1;
            if (prevWeapon < 0) prevWeapon = weapons.Length - 1;
            SwitchWeapon(prevWeapon);
        }

        // Gamepad Y/Triangle weapon selection
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            int nextWeapon = currentWeapon + 1;
            if (nextWeapon >= weapons.Length) nextWeapon = 0;
            SwitchWeapon(nextWeapon);
        }
    }

    private void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
            return;

        // Deactivate currently active weapon
        if (currentWeapon >= 0 && currentWeapon < weapons.Length && weapons[currentWeapon] != null)
        {
            weapons[currentWeapon].SetActive(false);
        }

        // Activate the new weapon
        GameObject newWeapon = weapons[index];
        if (newWeapon != null)
        {
            newWeapon.SetActive(true);
            currentWeapon = index;

            // ==========================================
            // LOGIKA PENGGANTIAN IKON UI
            // ==========================================
            if (weaponIconUI != null && weaponIcons != null && index < weaponIcons.Length)
            {
                if (weaponIcons[index] != null)
                {
                    // Ganti gambar dengan sprite senjata yang sesuai
                    weaponIconUI.sprite = weaponIcons[index];
                    weaponIconUI.color = Color.white; // Pastikan warnanya solid (terlihat)
                }
                else
                {
                    // Jika tidak ada gambar/sprite untuk senjata ini, sembunyikan ikonnya (transparan)
                    weaponIconUI.color = Color.clear;
                }
            }
        }
        else
        {
            Debug.LogWarning($"WeaponSwitcher: Weapon at index {index} is null.");
        }
    }
}
