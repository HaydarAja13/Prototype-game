using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HardEnemyAI : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    public float moveSpeed = 2.5f; // Gerak lambat tapi pasti
    public float shootingDistance = 15f; // Jarak untuk mulai menembak
    
    [Header("Animation Settings")]
    public Animator animator;
    
    [Header("Combat Settings")]
    public GameObject projectilePrefab; // Gunakan prefab peluru dengan damage tinggi
    public Transform firePoint;
    public float fireRate = 0.5f; // Tembak 1 kali per 2 detik (lebih lambat tapi mematikan)
    public float projectileSpeed = 25f;
    public float aimOffsetHeight = 1f;
    public Vector3 projectileScale = new Vector3(0.2f, 0.5f, 0.2f);
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip shootSound;

    private NavMeshAgent agent;
    private Transform player;
    private float nextFireTime;
    private Collider[] myColliders;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        myColliders = GetComponentsInChildren<Collider>();
        
        // Cari player di scene secara otomatis (tidak perlu FOV)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player tidak ditemukan! Pastikan Player memiliki tag 'Player'.");
        }
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- MENGATUR ANIMASI RUN / IDLE ---
        float speedMagnitude = agent.velocity.magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", speedMagnitude);
        }

        // --- MENGATUR SUARA LANGKAH ---
        if (audioSource != null && walkSound != null)
        {
            if (speedMagnitude > 0.05f)
            {
                if (!audioSource.isPlaying || audioSource.clip != walkSound)
                {
                    audioSource.clip = walkSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == walkSound)
                {
                    audioSource.Pause(); // Gunakan Pause daripada Stop agar tidak selalu mengulang dari awal
                }
            }
        }

        // --- LOGIKA MENGEJAR & MENEMBAK ---
        if (distanceToPlayer <= shootingDistance)
        {
            // STATE: BERHENTI & MENEMBAK
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Putar badan musuh agar menghadap player
            Vector3 dirToTarget = (player.position - transform.position).normalized;
            dirToTarget.y = 0f;
            if (dirToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToTarget), Time.deltaTime * 5f);
            }

            if (Time.time >= nextFireTime)
            {
                ShootProjectile(player);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            // STATE: MENGEJAR KEMANAPUN
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void ShootProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab atau Fire Point belum di-assign pada musuh Hard!");
            return;
        }

        // --- MENGATUR ANIMASI MENEMBAK ---
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Instantiate Peluru
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        Vector3 targetAimPosition = target.position + Vector3.up * aimOffsetHeight;
        Vector3 shootDirection = (targetAimPosition - firePoint.position).normalized;
        
        projectile.transform.localScale = projectileScale;
        projectile.transform.forward = shootDirection;
        projectile.transform.Rotate(90f, 0f, 0f); // Rebah ke depan

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * projectileSpeed, ForceMode.VelocityChange);
        }

        // Abaikan tabrakan peluru dengan diri sendiri
        Collider projCollider = projectile.GetComponent<Collider>();
        if (projCollider != null && myColliders != null)
        {
            for (int i = 0; i < myColliders.Length; i++)
            {
                Physics.IgnoreCollision(myColliders[i], projCollider);
            }
        }
    }
}
