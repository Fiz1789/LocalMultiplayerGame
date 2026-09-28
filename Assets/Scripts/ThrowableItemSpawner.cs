using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RowdyRobots
{
    public class ThrowableItemSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        [SerializeField] private GameObject[] grenadePrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 10f;
        private float timer = 0f;
        private GameObject[] spawnedGrenades;

        private void Start()
        {
            timer = spawnInterval;
            spawnedGrenades = new GameObject[spawnPoints.Length];
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                TrySpawnRandomGrenade();
                timer = spawnInterval;
            }
        }

        private void TrySpawnRandomGrenade()
        {
            if (grenadePrefabs.Length == 0 || spawnPoints.Length == 0) return;

            // Build a list of available spawn indices
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnedGrenades[i] == null)
                    availableIndices.Add(i);
            }

            if (availableIndices.Count == 0) return; // All spots full

            int spawnIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            int prefabIndex = Random.Range(0, grenadePrefabs.Length);

            GameObject prefab = grenadePrefabs[prefabIndex];
            Transform spawnPoint = spawnPoints[spawnIndex];

            GameObject grenade = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            spawnedGrenades[spawnIndex] = grenade;

            // When grenade is destroyed, clear the slot
            GrenadeDespawn despawn = grenade.AddComponent<GrenadeDespawn>();
            despawn.spawner = this;
            despawn.spawnIndex = spawnIndex;
        }

        // Called by GrenadeDespawn when grenade is destroyed
        public void ClearSpawnIndex(int index)
        {
            if (index >= 0 && index < spawnedGrenades.Length)
                spawnedGrenades[index] = null;
        }

        // Helper component to notify spawner when grenade is destroyed
        private class GrenadeDespawn : MonoBehaviour
        {
            public ThrowableItemSpawner spawner;
            public int spawnIndex;
            private void OnDestroy()
            {
                if (spawner != null)
                    spawner.ClearSpawnIndex(spawnIndex);
            }
        }
    }
}
