using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveTriggerArea : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Index objective yang akan diselesaikan ketika player menyentuh area ini (mulai dari 0)")]
    public int targetObjectiveIndex = 0;
    
    [Tooltip("Centang ini agar trigger langsung hancur setelah disentuh (mencegah terpanggil 2x)")]
    public bool destroyOnTrigger = true;

    private void Start()
    {
        // Pastikan collidernya menjadi trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Sembunyikan MeshRenderer jika tidak sengaja tertinggal
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            mesh.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.CompleteObjectiveByIndex(targetObjectiveIndex);
                
                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("ObjectiveTriggerArea: ObjectiveManager tidak ditemukan di scene!");
            }
        }
    }
}
