using UnityEngine;
using Unity.Behavior;
using System.Collections.Generic;

namespace RowdyRobots
{
    public class AlienSpawner : MonoBehaviour
    {
        [Header("Alien Spawn Settings")]
        [SerializeField] private GameObject alienPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnDelay = 10f;

        [Header("Waypoints")]
        [SerializeField] private List<GameObject> waypoints;

        private void Start()
        {
            Invoke(nameof(WaitForBattle), 0.1f);
        }

        private void WaitForBattle()
        {
            if (LevelManager.instance != null && LevelManager.instance.levelState != LevelManager.LevelStates.Battle)
            {
                Invoke(nameof(WaitForBattle), 0.1f);
                return;
            }
            Invoke(nameof(SpawnAlien), spawnDelay);
        }

        private void SpawnAlien()
        {
            if (alienPrefab != null)
            {
                Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
                Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

                GameObject alien = Instantiate(alienPrefab, pos, rot);

                // Shuffle
                List<GameObject> shuffledWaypoints = new List<GameObject>(waypoints);
                for (int i = shuffledWaypoints.Count - 1; i > 0; i--)
                {
                    int rand = Random.Range(0, i + 1);
                    (shuffledWaypoints[i], shuffledWaypoints[rand]) = (shuffledWaypoints[rand], shuffledWaypoints[i]);
                }

                BehaviorGraphAgent behaviorAgent = alien.GetComponent<BehaviorGraphAgent>();
                behaviorAgent.BlackboardReference.SetVariableValue("Waypoints", shuffledWaypoints);
            }
        }
    }
}

