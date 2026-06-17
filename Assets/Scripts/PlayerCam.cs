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
    float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
    float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

    yRotation += mouseX;
    xRotation -= mouseY;
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
        float targetFOV = Input.GetMouseButton(1) ? aimFOV : normalFOV;
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