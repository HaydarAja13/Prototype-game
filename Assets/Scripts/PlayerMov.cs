using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMov : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip crouchSound;
    
    [Tooltip("Jeda waktu antar langkah kaki")]
    public float footstepDelayWalk = 0.5f;
    public float footstepDelaySprint = 0.3f;
    public float footstepDelayCrouch = 0.6f;
    private float footstepTimer;
    private bool wasGrounded;

    [Header("Stealth / Sound Generation")]
    [Tooltip("Jumlah suara yang dihasilkan per detik saat berjalan")]
    public float walkNoise = 15f; 
    [Tooltip("Jumlah suara yang dihasilkan per detik saat berlari")]
    public float sprintNoise = 40f; 
    [Tooltip("Jumlah suara yang dihasilkan per detik saat jongkok")]
    public float crouchNoise = 2f; 
    [Tooltip("Jumlah suara yang dihasilkan per detik saat sliding")]
    public float slideNoise = 25f; 
    [Tooltip("Jumlah suara yang dihasilkan secara instan saat melompat")]
    public float jumpNoise = 35f;

    [Header("Animation")]
    public Animator animator;
    [Tooltip("Offset vertikal model dari posisi player. Negatif = turun, Positif = naik. Atur di Inspector sampai kaki Kyle tepat di tanah.")]
    public float modelYOffset = 0f;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Timer untuk mencegah animasi jatuh flicker saat berjalan di permukaan tidak rata
    private float airborneTimer = 0f;
    private const float AIRBORNE_GRACE_TIME = 0.15f; // Durasi grace period sebelum animasi jatuh aktif

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        gliding,
        air
    }

    public bool sliding;
    public bool gliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        // Matikan SEMUA komponen yang konflik pada model RobotKyle
        // Prefab RobotKyle punya ThirdPersonController & CharacterController sendiri yang bentrok!
        if (animator != null)
        {
            // 1. Matikan SEMUA collider agar tidak bertabrakan dengan capsule player
            Collider[] modelColliders = animator.GetComponentsInChildren<Collider>();
            foreach (Collider col in modelColliders)
            {
                col.enabled = false;
            }

            // 2. Matikan CharacterController bawaan RobotKyle (penyebab error di console)
            CharacterController[] charControllers = animator.GetComponentsInChildren<CharacterController>();
            foreach (CharacterController cc in charControllers)
            {
                cc.enabled = false;
            }

            // 3. Matikan SEMUA script/MonoBehaviour pada RobotKyle KECUALI Animator
            //    Ini akan mematikan ThirdPersonController dan script bawaan lainnya
            MonoBehaviour[] scripts = animator.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // Jangan matikan script milik kita sendiri (PlayerMov)
                if (script == this) continue;
                script.enabled = false;
            }

            // 4. Set layer ke IgnoreRaycast agar ground check tidak mengenai model
            SetLayerRecursive(animator.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
        }
    }

    // Fungsi helper untuk set layer pada semua child object
    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        UpdateAnimations();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        HandleAudio();
        HandleStealthNoise();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if ((Input.GetKey(jumpKey) || Input.GetKey(KeyCode.JoystickButton0)) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (audioSource != null && crouchSound != null && grounded)
                audioSource.PlayOneShot(crouchSound);

            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) || Input.GetKeyUp(KeyCode.JoystickButton1))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - Sliding
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey) || Input.GetKey(KeyCode.JoystickButton1))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && (Input.GetKey(sprintKey) || Input.GetKey(KeyCode.JoystickButton10)))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Gliding
        else if (gliding)
        {
            state = MovementState.gliding;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        if (audioSource != null && jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
            
        // Tambahkan suara ke SoundMeter
        if (SoundMeter.Instance != null)
            SoundMeter.Instance.AddSound(jumpNoise);

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        
        if (animator != null)
        {
            animator.SetBool("Jump", true);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
        
        if (animator != null)
        {
            animator.SetBool("Jump", false);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Pastikan model 3D (RobotKyle) mengikuti arah rotasi kamera/orientasi (Yaw)
        animator.transform.rotation = orientation.rotation;

        // Kunci posisi model agar selalu menempel pada capsule player (mencegah terpisah)
        // Gunakan modelYOffset yang bisa diatur di Inspector agar posisi kaki Kyle tepat di tanah
        animator.transform.position = transform.position + new Vector3(0, modelYOffset, 0);

        // Hitung kecepatan horizontal untuk animasi
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = flatVel.magnitude;

        animator.SetFloat("Speed", speed);
        
        // Grace timer: hanya aktifkan animasi jatuh jika sudah melayang cukup lama
        // Ini mencegah animasi fall/floating yang flicker saat jalan di permukaan tidak rata
        if (grounded)
        {
            airborneTimer = 0f;
            animator.SetBool("Grounded", true);
            animator.SetBool("FreeFall", false);
        }
        else
        {
            airborneTimer += Time.deltaTime;

            // Baru aktifkan animasi jatuh setelah melayang lebih dari grace time
            if (airborneTimer > AIRBORNE_GRACE_TIME)
            {
                animator.SetBool("Grounded", false);
                animator.SetBool("FreeFall", rb.linearVelocity.y < -0.5f);
            }
        }

        // Matikan trigger Jump jika sudah menyentuh tanah agar tidak nge-bug / float
        if (grounded && readyToJump)
        {
            animator.SetBool("Jump", false);
        }
        
        // Set motion speed ke 1 untuk kecepatan animasi default
        animator.SetFloat("MotionSpeed", 1f);
    }

    public bool OnSlope()
    {
        // Tambahkan whatIsGround agar raycast tidak mengenai model player (RobotKyle)
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void HandleAudio()
    {
        if (audioSource == null) return;

        // Cek Landing (Impact)
        if (!wasGrounded && grounded)
        {
            if (landSound != null) audioSource.PlayOneShot(landSound);
        }
        wasGrounded = grounded;

        // Handle Footsteps
        if (!grounded || state == MovementState.air || state == MovementState.sliding) return;
        
        // Cek pergerakan
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0)
        {
            if (state == MovementState.sprinting) footstepTimer = footstepDelaySprint;
            else if (state == MovementState.crouching) footstepTimer = footstepDelayCrouch;
            else footstepTimer = footstepDelayWalk;

            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                if (footstepSounds[randomIndex] != null)
                    audioSource.PlayOneShot(footstepSounds[randomIndex]);
            }
        }
    }

    private void HandleStealthNoise()
    {
        // Jika belum ada SoundMeter di scene, lewati
        if (SoundMeter.Instance == null) return;
        
        // Cek apakah player mencoba bergerak
        bool isInputting = Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
        // Cek apakah player benar-benar bergerak secara fisik (mencegah jalan di tempat tapi bunyi)
        bool isMovingPhysically = rb.linearVelocity.sqrMagnitude > 0.1f;

        if (isInputting && isMovingPhysically && grounded)
        {
            float noiseAmount = 0f;

            if (state == MovementState.sprinting)
                noiseAmount = sprintNoise;
            else if (state == MovementState.sliding)
                noiseAmount = slideNoise;
            else if (state == MovementState.crouching)
                noiseAmount = crouchNoise;
            else if (state == MovementState.walking)
                noiseAmount = walkNoise;

            SoundMeter.Instance.AddSound(noiseAmount * Time.deltaTime);
        }
    }
}
