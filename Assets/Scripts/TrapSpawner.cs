using UnityEngine;

namespace RowdyRobots
{
    public class TrapSpawner : MonoBehaviour
    {
        public Color triggeredColor = Color.red;
        public float disappearDelay = 0.5f;
        public GameObject trapPrefab;

        [Header("Trap Cooldown")]
        public float trapCooldown = 2f;
        private float lastTrapTime = -Mathf.Infinity;
        public float LastTrapTime => lastTrapTime;

        [Header("Trap Limit")]
        public int maxTraps = 2;
        private int m_trapsPlaced = 0;
        public int TrapsPlaced => m_trapsPlaced;


        private void Start()
        {
            lastTrapTime = Time.time; // Start on cooldown
        }
        public bool IsOnCooldown()
        {
            return Time.time - lastTrapTime < trapCooldown;
        }

        public bool HasReachedLimit()
        {
            return m_trapsPlaced >= maxTraps;
        }
        public void SpawnTrap(Vector3 position)
        {
            Debug.Log("SpawnTrap called");

            if (LevelManager.instance != null && LevelManager.instance.levelState != LevelManager.LevelStates.Battle)
                return;

            if (IsOnCooldown())
            {
                float timeLeft = trapCooldown - (Time.time - lastTrapTime);
                Debug.Log($"[TrapSpawner] On cooldown! Wait {timeLeft:F1}s more");
                return;
            }

            if (HasReachedLimit())
            {
                Debug.Log("[TrapSpawner] Max traps reached!");
                return;
            }

            if (trapPrefab != null)
            {
                GameObject trapObject = Instantiate(trapPrefab, position, Quaternion.identity);
                Trap trap = trapObject.GetComponent<Trap>();
                if (trap != null)
                    trap.SetSpawner(this);

                m_trapsPlaced++;
                lastTrapTime = Time.time;
                Debug.Log($"[TrapSpawner] Trap spawned! ({m_trapsPlaced}/{maxTraps})");
            }
            else
            {
                Debug.LogError("trapPrefab not assigned in Inspector!");
            }
        }
        public void OnTrapDestroyed()
        {
            m_trapsPlaced = Mathf.Max(0, m_trapsPlaced - 1);
            Debug.Log($"[TrapSpawner] Trap destroyed! ({m_trapsPlaced}/{maxTraps})");
        }
    }
}