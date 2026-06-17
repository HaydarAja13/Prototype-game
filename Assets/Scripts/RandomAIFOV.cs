using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RandomAIFOV : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    public float wanderSpeed = 3.5f;
    public float chaseSpeed = 7f; // Kecepatan AI saat mengejar
    public float chaseDuration = 3f; // Berapa lama AI mengejar setelah kehilangan jejak player
    
    private float timer;
    private NavMeshAgent agent;

    [Header("FOV Settings")]
    public float viewRadius = 25f; // Jarak pandang dan jarak tembak (DITINGKATKAN)
    [Range(0, 360)]
    public float viewAngle = 60f;
    public LayerMask targetMask;     // Layer untuk player/target
    public LayerMask obstacleMask;   // Layer untuk tembok/halangan

    [Header("Lighthouse Effect")]
    public Light fovLight; // Masukkan object Spotlight ke sini
    public Transform lightMountPoint; // Titik tempel lampu (misal: Kepala musuh) agar tidak delay
    public float sweepSpeed = 50f; // Kecepatan rotasi lampu mercusuar

    [Header("Animation Settings")]
    public Animator animator; // Referensi ke komponen Animator musuh

    [Header("Combat Settings")]
    public GameObject projectilePrefab; // Prefab peluru musuh
    public Transform firePoint;         // Titik muncul peluru
    public float fireRate = 1f;         // Tembak tepat 1 kali setiap 1 detik
    public float projectileSpeed = 20f; // Kecepatan peluru
    public float aimOffsetHeight = 1f;  // Ketinggian bidikan (offset) ke badan player
    public Vector3 projectileScale = new Vector3(0.2f, 0.5f, 0.2f); // Ukuran peluru (X, Y, Z) - Y adalah panjang capsule
    
    private float nextFireTime;
    private Transform currentTarget; // Mengingat target yang sedang dikejar
    private float loseSightTimer;    // Timer untuk menghitung berapa lama kehilangan jejak

    private Collider[] myColliders; // Cache untuk collider musuh agar tidak GetComponentsInChildren saat menembak
    private float visionTimer = 0f; // Timer untuk mengurangi frekuensi pengecekan FOV
    private const float VISION_INTERVAL = 0.1f; // Cek FOV setiap 0.1 detik (10 FPS)
    private Transform lastVisibleTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        
        if (fovLight == null)
        {
            fovLight = GetComponentInChildren<Light>();
            if (fovLight == null)
            {
                Debug.LogWarning("Spotlight belum ditambahkan ke AI ini!");
            }
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Cache collider untuk mengabaikan tabrakan peluru
        myColliders = GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        // Cegah error jika agent sedang tidak aktif atau belum menempel pada NavMesh (terutama saat baru respawn)
        if (!agent.isOnNavMesh) return;

        // Mengirimkan kecepatan saat ini ke Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // 1. Cek apakah ada target di dalam FOV (Dioptimasi agar tidak berjalan setiap frame)
        visionTimer += Time.deltaTime;
        if (visionTimer >= VISION_INTERVAL)
        {
            lastVisibleTarget = FindVisibleTargets(currentTarget);
            visionTimer = 0f;
        }
        
        Transform visibleTarget = lastVisibleTarget;

        if (visibleTarget != null)
        {
            // ==========================================
            // STATE: TERLIHAT (BERHENTI & MENEMBAK)
            // ==========================================
            currentTarget = visibleTarget;
            loseSightTimer = chaseDuration; // Reset timer kehilangan jejak

            agent.isStopped = true; // Berhenti bergerak untuk menembak
            agent.velocity = Vector3.zero; // Paksa kecepatan jadi 0 agar animasi kembali ke Idle

            // Putar seluruh badan musuh agar menghadap player
            Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;
            dirToTarget.y = 0f; // Jaga agar tidak mendongak/menunduk
            if (dirToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToTarget), Time.deltaTime * 10f);
            }

            // Kunci arah lampu menyorot ke arah player
            if (fovLight != null)
            {
                Vector3 lightDir = (currentTarget.position - fovLight.transform.position).normalized;
                fovLight.transform.rotation = Quaternion.Slerp(fovLight.transform.rotation, Quaternion.LookRotation(lightDir), Time.deltaTime * 10f);
            }

            // Logika Menembak
            if (Time.time >= nextFireTime)
            {
                ShootProjectile(currentTarget);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else if (currentTarget != null && loseSightTimer > 0f)
        {
            // ==========================================
            // STATE: MENGEJAR (KEHILANGAN JEJAK SEMENTARA)
            // ==========================================
            loseSightTimer -= Time.deltaTime; // Kurangi waktu mengejar
            
            agent.isStopped = false; // Boleh bergerak lagi
            agent.speed = chaseSpeed; // Lari lebih cepat
            agent.SetDestination(currentTarget.position); // Kejar ke posisi terakhir player

            // Lampu berputar mengikuti arah lari musuh
            if (fovLight != null)
            {
                Vector3 forwardDir = agent.velocity.sqrMagnitude > 0.1f ? agent.velocity.normalized : transform.forward;
                forwardDir.y = 0f;
                if (forwardDir != Vector3.zero)
                {
                    fovLight.transform.rotation = Quaternion.Slerp(fovLight.transform.rotation, Quaternion.LookRotation(forwardDir), Time.deltaTime * 5f);
                }
            }
        }
        else
        {
            // ==========================================
            // STATE: PATROLI RANDOM (WANDERING)
            // ==========================================
            currentTarget = null; // Lupakan target
            agent.isStopped = false; // Boleh bergerak
            agent.speed = wanderSpeed; // Kecepatan normal
            
            timer += Time.deltaTime;
            
            // Cari jalan baru JIKA waktu habis ATAU sudah sampai tujuan
            if (timer >= wanderTimer || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0;
            }

            // Lampu mengikuti arah jalan musuh
            if (fovLight != null)
            {
                Vector3 forwardDir = agent.velocity.sqrMagnitude > 0.1f ? agent.velocity.normalized : transform.forward;
                forwardDir.y = 0f;
                if (forwardDir != Vector3.zero)
                {
                    fovLight.transform.rotation = Quaternion.Slerp(fovLight.transform.rotation, Quaternion.LookRotation(forwardDir), Time.deltaTime * 5f);
                }
                
                fovLight.spotAngle = viewAngle;
                fovLight.range = viewRadius;
                fovLight.type = LightType.Spot;
            }
        }
    }

    void LateUpdate()
    {
        // Memastikan posisi lampu terus menempel pada mount point (kepala) setelah semua pergerakan animasi/navmesh di Update selesai
        // Ini mencegah lampu terlihat tertinggal saat frame berjalan
        if (fovLight != null && lightMountPoint != null)
        {
            fovLight.transform.position = lightMountPoint.position;
        }
    }

    // Menembakkan proyektil ke arah target
    void ShootProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab atau Fire Point belum di-assign pada musuh!");
            return;
        }

        // Putar animasi menembak
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        // Buat proyektil di posisi firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Hitung arah tembakan dengan mempertimbangkan offset tinggi
        Vector3 targetAimPosition = target.position + Vector3.up * aimOffsetHeight;
        Vector3 shootDirection = (targetAimPosition - firePoint.position).normalized;
        
        // Ubah ukuran peluru
        projectile.transform.localScale = projectileScale;

        // Putar peluru agar menghadap ke target
        projectile.transform.forward = shootDirection;
        // Karena capsule bawaan Unity berdiri tegak (sumbu Y), putar 90 derajat di sumbu X agar rebah ke depan
        projectile.transform.Rotate(90f, 0f, 0f);

        // Beri gaya (dorongan) pada proyektil (pastikan prefab memiliki Rigidbody)
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(shootDirection * projectileSpeed, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Proyektil musuh tidak memiliki komponen Rigidbody!");
        }

        // --- TAMBAHAN PENTING ---
        // Abaikan tabrakan antara peluru ini dengan tubuh musuh yang menembaknya
        Collider projCollider = projectile.GetComponent<Collider>();
        
        if (projCollider != null && myColliders != null)
        {
            for (int i = 0; i < myColliders.Length; i++)
            {
                Physics.IgnoreCollision(myColliders[i], projCollider);
            }
        }
    }

    // Mencari titik acak di atas NavMesh
    Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    // Mengembalikan Transform target jika terlihat, mengembalikan null jika hilang dari pandangan
    Transform FindVisibleTargets(Transform lastTarget)
    {
        Transform fovTransform = fovLight != null ? fovLight.transform : transform;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // Jika target ini adalah target yang SEDANG kita kejar, ubah sudut pandang jadi 360 derajat
            // Artinya: Selama player masih dalam View Radius dan tidak tertutup tembok, musuh tidak akan kehilangan target!
            float checkAngle = (target == lastTarget) ? 180f : (viewAngle / 2);

            // Cek apakah target ada di dalam radius & sudut FOV
            if (Vector3.Angle(fovTransform.forward, dirToTarget) < checkAngle)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // Cek apakah terhalang tembok
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    // Garis debug di editor
                    Debug.DrawLine(transform.position, target.position, Color.red);
                    return target; // Target ditemukan!
                }
            }
        }
        
        return null; // Target tidak terlihat
    }

    // Menggambar area FOV di Unity Editor agar mudah di-tweak
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Transform fovTransform = fovLight != null ? fovLight.transform : transform;
        
        Vector3 viewAngleA = DirFromAngle(fovTransform.eulerAngles.y, -viewAngle / 2);
        Vector3 viewAngleB = DirFromAngle(fovTransform.eulerAngles.y, viewAngle / 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    Vector3 DirFromAngle(float angleInDegrees, float angleOffset)
    {
        angleInDegrees += angleOffset;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
