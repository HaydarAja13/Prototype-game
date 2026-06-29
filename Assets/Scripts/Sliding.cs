using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMov pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Audio")]
    public AudioClip slideSound;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMov>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        // Jangan proses input saat game sedang dipause
        if (PauseManager.isPaused) return;

        // Cek apakah pemain sedang membuka dokumen atau CCTV
        bool isInspecting = (PlayerInventory.Instance != null && PlayerInventory.Instance.isInspecting);
        bool isViewingCCTV = (CCTVManager.Instance != null && CCTVManager.Instance.isViewingCCTV);
        bool isBusy = isInspecting || isViewingCCTV;

        horizontalInput = isBusy ? 0f : Input.GetAxisRaw("Horizontal");
        verticalInput = isBusy ? 0f : Input.GetAxisRaw("Vertical");

        // Cek apakah pemain menahan tombol lari (L3 atau Shift)
        bool isSprinting = Input.GetKey(pm.sprintKey) || Input.GetKey(KeyCode.JoystickButton8);

        // Hanya bisa slide jika SEDANG LARI dan tidak sedang sibuk di UI
        if (!isBusy && isSprinting && (Input.GetKeyDown(slideKey) || Input.GetKeyDown(KeyCode.JoystickButton1)))
            StartSlide();

        if (Input.GetKeyUp(slideKey) || Input.GetKeyUp(KeyCode.JoystickButton1))
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        if (pm.audioSource != null && slideSound != null)
            pm.audioSource.PlayOneShot(slideSound);

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
