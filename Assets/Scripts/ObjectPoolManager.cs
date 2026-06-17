using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    // Singleton agar mudah dipanggil dari script mana saja
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string poolName;       // Nama panggilan pool (contoh: "DamageText", "MuzzleFlash")
        public GameObject prefab;     // Prefab yang akan di-pool
        public int poolSize = 20;     // Jumlah objek yang disiapkan di awal
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        // Setup Singleton
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Membuat pool untuk setiap item yang didaftarkan
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // Matikan dulu
                // Supaya rapi, jadikan child dari manager ini
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    // Fungsi untuk mengambil objek dari pool
    public GameObject SpawnFromPool(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning("Pool dengan nama " + poolName + " tidak ditemukan!");
            return null;
        }

        // Ambil objek paling depan di antrean
        GameObject objectToSpawn = poolDictionary[poolName].Dequeue();

        // Aktifkan dan atur posisi & rotasi
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Masukkan kembali ke antrean belakang agar nanti bisa dipakai lagi bergantian
        poolDictionary[poolName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
