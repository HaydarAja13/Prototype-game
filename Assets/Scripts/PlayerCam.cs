using UnityEngine;

public class PlayerCam : MonoBehaviour
{
     public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    [Header("Recoil Settings")]
    public float recoilSnappiness = 10f;
    public float recoilReturnSpeed = 5f;
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    [Header("Aim Scope Settings")]
    public float normalFOV = 60f;
    public float aimFOV = 30f;
    public float aimSpeed = 10f;
    private Camera cam;

    [Header("Gamepad Settings")]
    [Tooltip("Sensitivity multiplier khusus untuk analog kanan controller")]
    public float gamepadSensMultiplier = 3f;

    [Header("Aim Assist Settings (Controller Only)")]
    public bool useAimAssist = true;
    public float aimAssistRadius = 1.5f;
    public float aimAssistMaxDistance = 50f;
    [Range(0f, 1f)]
    public float aimAssistFriction = 0.5f; // Semakin kecil, semakin lambat saat membidik musuh
    public float aimAssistMagnetism = 3f; // Kekuatan tarikan kamera ke musuh

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            normalFOV = cam.fieldOfView; // Simpan FOV awal
        }
    }

  // Update is called once per frame
  void Update()
  {
    // Jangan proses input kamera saat game sedang dipause
    if (PauseManager.isPaused) return;

    // Baca input mouse (bawaan Unity, Type: Mouse Movement)
    float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
    float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

    // Baca input analog kanan controller secara TERPISAH (Type: Joystick Axis)
    // Menggunakan nama axis berbeda agar tidak tercampur dengan Mouse
    float stickX = 0f;
    float stickY = 0f;
    try
    {
        stickX = Input.GetAxisRaw("RightStickX") * Time.deltaTime * sensX * gamepadSensMultiplier;
        stickY = Input.GetAxisRaw("RightStickY") * Time.deltaTime * sensY * gamepadSensMultiplier;
    }
    catch (System.Exception)
    {
        // Axis belum dibuat di Input Manager, abaikan saja
    }

    // --- AIM ASSIST LOGIC (CONTROLLER ONLY) ---
    float assistX = 0f;
    float assistY = 0f;

    // Cek apakah ada input pergerakan pandangan dari controller (meskipun kecil)
    bool isUsingController = (Mathf.Abs(stickX) > 0.001f || Mathf.Abs(stickY) > 0.001f);

    if (useAimAssist && isUsingController)
    {
        // 1. Gunakan SphereCastAll untuk mendeteksi semua objek dalam radius (mengabaikan lantai yang menghalangi)
        RaycastHit[] hits = Physics.SphereCastAll(cam.transform.position, aimAssistRadius, cam.transform.forward, aimAssistMaxDistance);
        
        Transform bestTarget = null;
        float closestDistance = float.MaxValue;
        Vector3 targetCenter = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            BotHitbox hitbox = hit.transform.GetComponent<BotHitbox>();
            if (hitbox != null)
            {
                // Ambil titik tengah persis dari badan musuh (Center of Mass)
                Vector3 hitboxCenter = hit.collider.bounds.center;
                
                // 2. Anti-Wallhack (Line of Sight Check)
                // Lakukan Linecast dari kamera ke titik tengah badan musuh.
                // Jika terhalang sesuatu yang BUKAN bagian dari musuh itu sendiri (misal tembok), lewati target ini.
                RaycastHit sightHit;
                if (Physics.Linecast(cam.transform.position, hitboxCenter, out sightHit))
                {
                    // Pastikan linecast mengenai musuh tersebut, jika mengenai benda lain, berarti terhalang
                    if (sightHit.transform != hit.transform && sightHit.transform.GetComponent<BotHitbox>() == null)
                    {
                        continue; // Terhalang tembok/benda lain
                    }
                }

                // Cari target yang terdekat dari tengah layar
                float distanceToScreenCenter = Vector3.Distance(cam.transform.position + cam.transform.forward * hit.distance, hitboxCenter);
                if (distanceToScreenCenter < closestDistance)
                {
                    closestDistance = distanceToScreenCenter;
                    bestTarget = hit.transform;
                    targetCenter = hitboxCenter;
                }
            }
        }

        if (bestTarget != null)
        {
            // 3. FRICTION: Perlambat kecepatan belok analog agar bidikan lebih stabil
            stickX *= aimAssistFriction;
            stickY *= aimAssistFriction;

            // 4. MAGNETISM: Tarik pandangan perlahan menuju TENGAH BADAN musuh
            Vector3 localTargetPos = cam.transform.InverseTransformPoint(targetCenter);
            
            // Perhitungan arah tarikan
            float pullX = Mathf.Clamp(localTargetPos.x, -1f, 1f);
            float pullY = Mathf.Clamp(localTargetPos.y, -1f, 1f);
            
            // Aplikasikan kekuatan magnetism (dikalikan dengan Time.deltaTime agar stabil di semua frame rate)
            assistX = pullX * aimAssistMagnetism * Time.deltaTime;
            assistY = pullY * aimAssistMagnetism * Time.deltaTime;
        }
    }
    // ------------------------------------------

    // Gabungkan mouse + stick + aim assist magnetism
    float totalX = mouseX + stickX + assistX;
    float totalY = mouseY + stickY + assistY;

    yRotation += totalX;
    xRotation -= totalY;
    xRotation = Mathf.Clamp(xRotation, -90f, 90f);

    // Menghitung kembalinya recoil secara halus (spring effect)
    targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
    currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, recoilSnappiness * Time.deltaTime);

    // Terapkan rotasi dasar ditambah dengan offset recoil
    transform.rotation = Quaternion.Euler(xRotation - currentRecoil.x, yRotation + currentRecoil.y, currentRecoil.z);
    orientation.rotation = Quaternion.Euler(0, yRotation, 0);  

    // Logika Aim Scope (Zoom) dengan Mouse Kanan
    if (cam != null)
    {
        float targetFOV = (Input.GetMouseButton(1) || Input.GetKey(KeyCode.JoystickButton4)) ? aimFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, aimSpeed * Time.deltaTime);
    }
  }

  // Fungsi untuk dipanggil saat menembak
  public void ApplyRecoil(float recoilX, float recoilY, float recoilZ)
  {
      // Tambahkan nilai recoil ke target recoil saat ini
      targetRecoil += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
  }
}