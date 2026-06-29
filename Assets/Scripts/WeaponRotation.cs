using UnityEngine;

public class WeaponRotation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referensi ke Transform kamera player (yang punya script PlayerCam)")]
    public Transform playerCamera;

    [Tooltip("Referensi ke Transform orientation player (sama seperti yang dipakai di PlayerMov)")]
    public Transform orientation;

    [Tooltip("Referensi ke Transform player (root GameObject player, yang punya Rigidbody)")]
    public Transform player;

    [Header("Position Settings")]
    [Tooltip("Offset posisi senjata relatif terhadap arah pandang player.\n" +
             "X = kanan/kiri, Y = atas/bawah, Z = depan/belakang")]
    public Vector3 positionOffset = new Vector3(0.5f, -0.3f, 0.8f);

    [Tooltip("Kecepatan posisi senjata mengikuti player (semakin besar = semakin responsif)")]
    public float positionSpeed = 15f;

    [Header("Rotation Settings")]
    [Tooltip("Kecepatan rotasi senjata mengikuti arah player (semakin besar = semakin responsif)")]
    public float rotationSpeed = 15f;

    [Tooltip("Jika true, senjata juga mengikuti rotasi vertikal (atas/bawah) dari kamera")]
    public bool followVerticalRotation = true;

    [Header("Rotation Offset")]
    [Tooltip("Offset rotasi tambahan jika model senjata tidak menghadap arah yang benar (misal Y=90)")]
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Sway (Opsional)")]
    [Tooltip("Aktifkan efek sway/goyang ringan saat menggerakkan mouse")]
    public bool enableSway = true;

    [Tooltip("Intensitas efek sway")]
    public float swayAmount = 2f;

    [Tooltip("Batas maksimal efek sway (derajat)")]
    public float maxSwayAmount = 5f;

    [Tooltip("Kecepatan sway kembali ke posisi semula")]
    public float swaySmooth = 6f;

    [Header("Gamepad Settings")]
    [Tooltip("Multiplier khusus untuk efek sway menggunakan analog kanan controller")]
    public float gamepadSwayMultiplier = 3f;

    // Internal
    private float swayX;
    private float swayY;

    private void LateUpdate()
    {
        // Jangan proses saat game sedang dipause
        if (PauseManager.isPaused) return;

        // Gunakan LateUpdate agar dihitung SETELAH
        // PlayerCam.Update() selesai mengupdate rotasi kamera & orientation.

        UpdatePosition();
        UpdateRotation();
    }

    /// <summary>
    /// Mengatur posisi senjata agar selalu berada di depan player sesuai arah pandang.
    /// Posisi dihitung relatif terhadap rotasi kamera, sehingga senjata 
    /// selalu terlihat di depan meskipun player melihat ke belakang/atas/bawah.
    /// </summary>
    private void UpdatePosition()
    {
        if (player == null)
        {
            Debug.LogWarning("WeaponRotation: Assign 'player' Transform di Inspector!");
            return;
        }

        // Tentukan referensi rotasi untuk menghitung posisi
        // Gunakan kamera agar senjata ikut naik/turun saat melihat atas/bawah
        Transform rotRef = (followVerticalRotation && playerCamera != null) ? playerCamera : orientation;

        if (rotRef == null) return;

        // Hitung posisi target berdasarkan offset relatif terhadap arah pandang
        // positionOffset.x = kanan/kiri (right)
        // positionOffset.y = atas/bawah (up)  
        // positionOffset.z = depan/belakang (forward)
        Vector3 targetPosition = player.position
            + rotRef.right * positionOffset.x
            + rotRef.up * positionOffset.y
            + rotRef.forward * positionOffset.z;

        // Smooth position agar tidak kaku
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Mengatur rotasi senjata agar mengikuti arah pandang player.
    /// </summary>
    private void UpdateRotation()
    {
        Quaternion baseRotation = CalculateBaseRotation();
        Quaternion swayRotation = Quaternion.identity;

        if (enableSway)
            swayRotation = CalculateSway();

        // Gabungkan base rotation + sway, lalu smooth ke target
        Quaternion finalTarget = baseRotation * swayRotation;

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTarget, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Menghitung rotasi dasar senjata berdasarkan arah pandang player.
    /// </summary>
    private Quaternion CalculateBaseRotation()
    {
        if (followVerticalRotation && playerCamera != null)
        {
            // Ikuti rotasi kamera penuh (horizontal + vertikal)
            return playerCamera.rotation * Quaternion.Euler(rotationOffset);
        }
        else if (orientation != null)
        {
            // Hanya ikuti rotasi horizontal (Y-axis) dari orientation
            return orientation.rotation * Quaternion.Euler(rotationOffset);
        }

        Debug.LogWarning("WeaponRotation: Assign playerCamera atau orientation di Inspector!");
        return transform.rotation;
    }

    /// <summary>
    /// Menghitung efek sway (goyang ringan) berdasarkan input mouse dan controller.
    /// </summary>
    private Quaternion CalculateSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float stickX = 0f;
        float stickY = 0f;
        try
        {
            stickX = Input.GetAxisRaw("RightStickX") * gamepadSwayMultiplier;
            stickY = Input.GetAxisRaw("RightStickY") * gamepadSwayMultiplier;
        }
        catch (System.Exception)
        {
            // Abaikan jika Axis belum dibuat di Input Manager
        }

        float totalX = mouseX + stickX;
        float totalY = mouseY + stickY;

        float targetSwayX = Mathf.Clamp(-totalY * swayAmount, -maxSwayAmount, maxSwayAmount);
        float targetSwayY = Mathf.Clamp(-totalX * swayAmount, -maxSwayAmount, maxSwayAmount);

        swayX = Mathf.Lerp(swayX, targetSwayX, swaySmooth * Time.deltaTime);
        swayY = Mathf.Lerp(swayY, targetSwayY, swaySmooth * Time.deltaTime);

        return Quaternion.Euler(swayX, swayY, 0f);
    }
}
