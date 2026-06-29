using UnityEngine;

public class Gliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMov pm;

    [Header("Gliding")]
    public float glideFallSpeed = 2f;          // kecepatan jatuh saat glide (makin kecil = makin lambat)
    public float glideForwardSpeed = 8f;       // dorongan maju saat gliding
    public float glideSteerSpeed = 5f;         // kecepatan belok kiri/kanan saat gliding

    [Header("Input")]
    public KeyCode glideKey = KeyCode.F;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMov>();
    }

    private void Update()
    {
        // Jangan proses input saat game sedang dipause
        if (PauseManager.isPaused) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Mulai glide: tekan F saat di udara (tidak grounded) dan sedang jatuh
        if (Input.GetKeyDown(glideKey) && !pm.grounded && !pm.gliding)
        {
            StartGlide();
        }

        // Berhenti glide: lepas F/JoystickButton3, atau mendarat
        if (pm.gliding)
        {
            if (Input.GetKeyUp(glideKey) || Input.GetKeyUp(KeyCode.JoystickButton3) || pm.grounded)
            {
                StopGlide();
            }
        }
    }

    private void FixedUpdate()
    {
        if (pm.gliding)
        {
            GlideMovement();
        }
    }

    private void StartGlide()
    {
        pm.gliding = true;

        // Reset velocity vertikal agar transisi ke glide terasa halus
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    private void GlideMovement()
    {
        // --- Kontrol jatuh: clamp velocity Y agar turun perlahan ---
        if (rb.linearVelocity.y < -glideFallSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -glideFallSpeed, rb.linearVelocity.z);
        }

        // --- Dorongan maju berdasarkan arah kamera ---
        Vector3 forwardDir = orientation.forward;
        forwardDir.y = 0f;
        forwardDir.Normalize();

        rb.AddForce(forwardDir * glideForwardSpeed, ForceMode.Force);

        // --- Steering kiri/kanan ---
        Vector3 rightDir = orientation.right;
        rightDir.y = 0f;
        rightDir.Normalize();

        rb.AddForce(rightDir * horizontalInput * glideSteerSpeed, ForceMode.Force);

        // --- Kontrol maju/mundur tambahan ---
        rb.AddForce(forwardDir * verticalInput * glideSteerSpeed, ForceMode.Force);

        // --- Kurangi gravity saat gliding ---
        // Gravity sudah dikurangi efeknya karena kita clamp velocity Y di atas,
        // tapi kita juga bisa counteract sebagian gravity agar lebih smooth
        rb.AddForce(Vector3.up * Physics.gravity.magnitude * 0.7f, ForceMode.Acceleration);
    }

    private void StopGlide()
    {
        pm.gliding = false;
    }
}
