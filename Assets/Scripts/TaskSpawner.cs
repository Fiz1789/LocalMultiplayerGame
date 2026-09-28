using UnityEngine;

namespace RowdyRobots
{
    public class TaskSpawner : MonoBehaviour
    {
        [Header("Task Prefabs")]
        [SerializeField] private GameObject[] taskZonePrefabs;      // your 3 specific TaskZone prefabs
        [SerializeField] private Transform[] taskZoneSpawnPoints;   // scene positions to spawn at
        [SerializeField] private int taskZoneSpawnCount = 2;        // how many to pick each time
        [SerializeField] private GameObject transferBallPrefab;   // Task 2: TransferBall + TransferZone
        [SerializeField] private Transform[] transferBallSpawnPoints; // scene positions for TransferBall
        [SerializeField] private Transform transferZonePoint;        // fixed scene position for the drop-off zone
        [SerializeField] private GameObject ballPickupPrefab;     // Task 3: BallPickup
        [SerializeField] private Transform[] ballPickupSpawnPoints;  // scene positions for BallPickup

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 30f;
        [SerializeField] private float taskZoneCooldown = 20f; // cooldown after all task zones complete
        [SerializeField] private int maxActiveTasks = 3;

        private float m_spawnTimer = 0f;
        private int m_activeTaskCount = 0;
        private int m_activeTaskZoneCount = 0;
        private bool m_taskZonesActive = false;
        private bool m_transferBallActive = false;
        private bool m_ballPickupActive = false;

        private void Update()
        {
            if (LevelManager.instance != null && LevelManager.instance.levelState != LevelManager.LevelStates.Battle)
                return;

            m_spawnTimer += Time.deltaTime;
            if (m_spawnTimer >= spawnInterval && m_activeTaskCount < maxActiveTasks)
            {
                m_spawnTimer = 0f;
                SpawnRandomTask();
            }
        }

        private void SpawnRandomTask()
        {
            // Pick random task type (0 = TaskZone, 1 = TransferBall, 2 = BallPickup)
            // Skip types that are already active
            var available = new System.Collections.Generic.List<int>();
            if (!m_taskZonesActive) available.Add(0);
            if (!m_transferBallActive) available.Add(1);
            if (!m_ballPickupActive) available.Add(2);

            if (available.Count == 0) return;
            int taskType = available[Random.Range(0, available.Count)];

            if (taskType == 0)
            {
                if (taskZonePrefabs == null || taskZonePrefabs.Length == 0) return;
                if (taskZoneSpawnPoints == null || taskZoneSpawnPoints.Length == 0) return;
                int count = Mathf.Min(taskZoneSpawnCount, Mathf.Min(taskZonePrefabs.Length, taskZoneSpawnPoints.Length));

                // Shuffle prefab indices
                int[] prefabIndices = new int[taskZonePrefabs.Length];
                for (int i = 0; i < prefabIndices.Length; i++) prefabIndices[i] = i;
                for (int i = prefabIndices.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (prefabIndices[i], prefabIndices[j]) = (prefabIndices[j], prefabIndices[i]);
                }

                // Shuffle spawn point indices
                int[] spawnIndices = new int[taskZoneSpawnPoints.Length];
                for (int i = 0; i < spawnIndices.Length; i++) spawnIndices[i] = i;
                for (int i = spawnIndices.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (spawnIndices[i], spawnIndices[j]) = (spawnIndices[j], spawnIndices[i]);
                }

                m_taskZonesActive = true;
                m_activeTaskZoneCount = 0;

                for (int i = 0; i < count; i++)
                {
                    GameObject prefab = taskZonePrefabs[prefabIndices[i]];
                    Transform spawnPoint = taskZoneSpawnPoints[spawnIndices[i]];
                    if (prefab == null) continue;
                    GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                    m_activeTaskCount++;
                    m_activeTaskZoneCount++;
                    TaskLifetime lifetime = spawned.AddComponent<TaskLifetime>();
                    lifetime.Init(this, isTaskZone: true);
                }
                Debug.Log($"[TaskSpawner] Spawned {count} TaskZone(s)");
            }
            else
            {
                GameObject prefab = taskType switch
                {
                    1 => transferBallPrefab,
                    2 => ballPickupPrefab,
                    _ => null
                };

                if (prefab == null) return;

                Transform[] points = taskType == 1 ? transferBallSpawnPoints : ballPickupSpawnPoints;
                Vector3 spawnPos = prefab.transform.position;
                if (points != null && points.Length > 0)
                    spawnPos = points[Random.Range(0, points.Length)].position;

                GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity);

                // For TransferBall: root is the TransferZone, ball is a child.
                // Place zone first at its fixed point, then move ball child to its spawn.
                if (taskType == 1)
                {
                    if (transferZonePoint != null)
                        spawned.transform.position = transferZonePoint.position;

                    TransferBall ball = spawned.GetComponentInChildren<TransferBall>();
                    if (ball != null) ball.transform.position = spawnPos;
                }

                m_activeTaskCount++;

                if (taskType == 1) m_transferBallActive = true;
                if (taskType == 2) m_ballPickupActive = true;

                TaskLifetime lifetime = spawned.AddComponent<TaskLifetime>();
                lifetime.Init(this, isTaskZone: false, taskType: taskType);

                Debug.Log($"[TaskSpawner] Spawned task type {taskType} at prefab position {prefab.transform.position}");
            }
        }

        public void OnTaskDestroyed(bool isTaskZone, int taskType = -1)
        {
            m_activeTaskCount--;
            m_activeTaskCount = Mathf.Max(0, m_activeTaskCount);

            if (isTaskZone)
            {
                m_activeTaskZoneCount--;
                if (m_activeTaskZoneCount <= 0)
                {
                    m_activeTaskZoneCount = 0;
                    m_taskZonesActive = false;
                    // Apply dedicated taskZone cooldown: set timer so it waits taskZoneCooldown seconds
                    m_spawnTimer = spawnInterval - taskZoneCooldown;
                    Debug.Log("[TaskSpawner] All TaskZones completed, cooldown reset");
                }
            }
            else
            {
                if (taskType == 1) m_transferBallActive = false;
                if (taskType == 2) m_ballPickupActive = false;
                m_spawnTimer = 0f; // Reset cooldown when task destroyed
            }
        }
    }
}
