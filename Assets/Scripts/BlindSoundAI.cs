using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BlindSoundAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    public float wanderSpeed = 3.5f;
    public float chaseSpeed = 10f; // Kecepatan lari tinggi untuk tipe Medium
    public float chaseDuration = 5f; // Lama mengejar jika suara hilang
    
    private float timer;
    private NavMeshAgent agent;

    [Header("Sound Sensitivity (Hearing)")]
    [Tooltip("Jika batas suara player di SoundMeter mencapai angka ini, musuh akan langsung tahu posisi player (Sangat Sensitif)")]
    public float hearingThreshold = 10f;
    [Tooltip("Jarak maksimal musuh bisa mendengar suara tersebut")]
    public float hearingRange = 30f;
    
    [Header("Combat Settings")]
    public float attackRange = 15f; // Jarak untuk mulai melempar proyektil
    public float fireRate = 1.5f; 
    private float nextFireTime;
    public GameObject projectilePrefab; // Gunakan prefab Ball/Proyektil dengan script EnemyProjectile (damage di-set tinggi di inspector)
    public Transform firePoint;
    public float projectileSpeed = 25f;
    public float aimOffsetHeight = 1f;
    
    [Header("Animation")]
    public Animator animator;

    [Header("Audio Settings")]
    public AudioClip patrolSound;
    public AudioClip chaseSound;
    public AudioClip ambientSound;
    public AudioClip shootSound;
    
    [Range(0f, 1f)] public float ambientVolume = 0.3f;
    [Range(0f, 1f)] public float movementVolume = 0.3f;
    [Range(0f, 1f)] public float chaseVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 0.6f;
    
    private AudioSource movementAudioSource;
    private AudioSource ambientAudioSource;
    private AudioSource sfxAudioSource;
    private AudioSource chaseAudioSource; // Audio terpisah khusus stress ambient

    private Transform playerTarget;
    private float loseSightTimer;
    private Vector3 lastHeardPosition;
    private Collider[] myColliders;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        myColliders = GetComponentsInChildren<Collider>();

        // Mencari target otomatis
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }

        // --- SETUP AUDIO SOURCES ---
        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.spatialBlend = 1f; // Suara 3D
        movementAudioSource.volume = movementVolume;
        movementAudioSource.minDistance = 15f;
        movementAudioSource.maxDistance = 50f;
        movementAudioSource.rolloffMode = AudioRolloffMode.Linear;

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.spatialBlend = 1f;
        ambientAudioSource.volume = ambientVolume;
        ambientAudioSource.loop = true;
        ambientAudioSource.minDistance = 30f;  // Mulai terdengar jelas dari jarak 30m
        ambientAudioSource.maxDistance = 150f; // Jangkauan global (bisa terdengar dari ujung map)
        ambientAudioSource.rolloffMode = AudioRolloffMode.Linear;
        if (ambientSound != null)
        {
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.Play();
        }

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.spatialBlend = 1f;
        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.minDistance = 20f;
        sfxAudioSource.maxDistance = 80f;
        sfxAudioSource.rolloffMode = AudioRolloffMode.Linear;

        chaseAudioSource = gameObject.AddComponent<AudioSource>();
        chaseAudioSource.spatialBlend = 1f;
        chaseAudioSource.volume = chaseVolume;
        chaseAudioSource.loop = true;
        chaseAudioSource.minDistance = 25f;
        chaseAudioSource.maxDistance = 100f;
        chaseAudioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    void OnEnable()
    {
        // Berlangganan event saat bar suara penuh (100%)
        SoundMeter.OnMaxSoundReached += OnLoudNoiseHeard;
    }

    void OnDisable()
    {
        SoundMeter.OnMaxSoundReached -= OnLoudNoiseHeard;
    }

    private void OnLoudNoiseHeard(Transform target)
    {
        // Langsung mengejar jika mendengar suara sangat bising
        if (target != null)
        {
            HearPlayer(target);
        }
    }

    private void HearPlayer(Transform target)
    {
        loseSightTimer = chaseDuration;
        lastHeardPosition = target.position;
        // Hanya update arah kejar kalau agent sedang aktif di navmesh
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(lastHeardPosition);
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return;

        // Kirim parameter kecepatan ke animator
        float speedMag = agent.velocity.magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", speedMag);
        }

        // --- AUDIO CHASE (STRESS AMBIENT) ---
        // Diputar selama musuh dalam mode waspada/mengejar (loseSightTimer > 0), walau dia sedang diam menembak
        if (loseSightTimer > 0f)
        {
            if (chaseSound != null && (!chaseAudioSource.isPlaying || chaseAudioSource.clip != chaseSound))
            {
                chaseAudioSource.clip = chaseSound;
                chaseAudioSource.Play();
            }
        }
        else
        {
            if (chaseAudioSource.isPlaying) chaseAudioSource.Stop();
        }

        // --- AUDIO MOVEMENT (PATROL) ---
        if (speedMag > 0.1f && !agent.isStopped)
        {
            if (patrolSound != null && (!movementAudioSource.isPlaying || movementAudioSource.clip != patrolSound))
            {
                movementAudioSource.clip = patrolSound;
                movementAudioSource.loop = true;
                movementAudioSource.Play();
            }
        }
        else
        {
            if (movementAudioSource.isPlaying) movementAudioSource.Stop();
        }

        if (SoundMeter.Instance != null && playerTarget != null)
        {
            // Pendengaran Global: Jika suara mencapai threshold sensitif, langsung kejar tanpa mempedulikan jarak!
            if (SoundMeter.Instance.currentSound >= hearingThreshold)
            {
                HearPlayer(playerTarget);
            }
        }

        // State Machine Logika
        if (loseSightTimer > 0f && playerTarget != null)
        {
            // --- STATE: MENGEJAR ---
            loseSightTimer -= Time.deltaTime;
            
            float distance = Vector3.Distance(transform.position, playerTarget.position);

            if (distance <= attackRange)
            {
                // Berhenti untuk serang/lempar
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                // Menghadap player
                Vector3 dirToTarget = (playerTarget.position - transform.position).normalized;
                dirToTarget.y = 0f;
                if (dirToTarget != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToTarget), Time.deltaTime * 10f);
                }

                if (Time.time >= nextFireTime)
                {
                    AttackPlayer();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
            else
            {
                // Kejar LOKASI SUARA TERAKHIR, bukan posisi player yang sedang diam-diam menjauh
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                agent.SetDestination(lastHeardPosition);
            }
        }
        else
        {
            // --- STATE: WANDERING / PATROLI RANDOM ---
            agent.isStopped = false;
            agent.speed = wanderSpeed;
            
            timer += Time.deltaTime;
            if (timer >= wanderTimer || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0;
            }
        }
    }

    private void AttackPlayer()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Trigger animasi, asset Demon memiliki animasi "Throw" atau "Shoot"
        if (animator != null)
        {
            animator.SetTrigger("Shoot"); // Ganti "Shoot" ke trigger "Throw" sesuai settingan Animator jika perlu
        }

        // Putar suara menembak
        if (shootSound != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(shootSound);
        }

        // Instansiasi proyektil
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Vector3 targetAimPosition = playerTarget.position + Vector3.up * aimOffsetHeight;
        Vector3 shootDirection = (targetAimPosition - firePoint.position).normalized;
        
        projectile.transform.forward = shootDirection;
        projectile.transform.Rotate(90f, 0f, 0f);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * projectileSpeed, ForceMode.VelocityChange);
        }

        // Abaikan tabrakan peluru dengan tubuhnya sendiri
        Collider projCollider = projectile.GetComponent<Collider>();
        if (projCollider != null && myColliders != null)
        {
            for (int i = 0; i < myColliders.Length; i++)
            {
                Physics.IgnoreCollision(myColliders[i], projCollider);
            }
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}
