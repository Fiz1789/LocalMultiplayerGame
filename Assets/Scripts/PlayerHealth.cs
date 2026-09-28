// using System.Collections;
// using UnityEngine;
// using UnityEngine.InputSystem; 

// namespace RowdyRobots
// {
//     public class PlayerHealth : MonoBehaviour
//     {
//         [Header("Respawn Settings")]
//         [SerializeField] private float respawnDelay = 3f;
//         [SerializeField] private Vector3 respawnPosition = new Vector3(0f, 0f, 0f);
//         [SerializeField] private float deathAnimationLength = 1.5f;

//         private Animator m_animator;
//         private PlayerWalk m_playerWalk;
//         private Rigidbody m_rigidbody;

//         private bool m_isDead = false;

//         private void Awake()
//         {
//             m_animator = GetComponentInChildren<Animator>();
//             m_playerWalk = GetComponent<PlayerWalk>();
//             m_rigidbody = GetComponent<Rigidbody>();
//         }

//         public void TakeDamage()
//         {
//             // Prevents multiple deaths at once
//             if (m_isDead) return;
//             StartCoroutine(RespawnRoutine());

//             // TEMP: Set player inactive immediately (for testing)
//             gameObject.SetActive(false);
//         }

//         private IEnumerator RespawnRoutine()
//         {
//             m_isDead = true;

//             // 1. Disable input immediately
//             m_playerWalk.enabled = false;

//             // 2. Stop rigidbody movement
//             m_rigidbody.linearVelocity = Vector3.zero;
//             m_rigidbody.angularVelocity = Vector3.zero;

//             // 3. Play death animation once
//             if (m_animator != null)
//             {
//                 m_animator.ResetTrigger("Die");
//                 m_animator.SetTrigger("Die");
//             }

//             // 4. Wait for death animation to finish
//             yield return new WaitForSeconds(deathAnimationLength);

//             // 5. Hide player
//             foreach (var renderer in GetComponentsInChildren<Renderer>())
//                 renderer.enabled = false;

//             // TEMP: Set player inactive after animation (for testing)
//             gameObject.SetActive(false);

//             // // 6. Wait remaining respawn delay
//             // yield return new WaitForSeconds(respawnDelay - deathAnimationLength);

//             // // 7. Respawn — move position, show player, re-enable
//             // transform.position = respawnPosition;

//             // foreach (var renderer in GetComponentsInChildren<Renderer>())
//             //     renderer.enabled = true;

//             // // 8. Reset animator state
//             // if (m_animator != null)
//             // {
//             //     m_animator.ResetTrigger("Die");
//             //     m_animator.Play("Idle"); // ← replace "Idle" with your idle state name
//             // }

//             // m_playerWalk.enabled = true;
//             // m_isDead = false;

//             // Debug.Log("[PlayerHealth] Player respawned!");
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RowdyRobots
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float deathAnimationLength = 1f;
        [SerializeField] private float respawnDelay = 5f;
        [SerializeField] private float alienSafeRadius = 10f;
        [SerializeField] private LayerMask alienLayer;

        private static readonly Vector3[] s_respawnPositions = new Vector3[]
        {
            new Vector3(-40.7f, 1f, 26.8f),
            new Vector3(33.6f, 1f, 33.4f),
            new Vector3(-9f, 1f, -7.1f),
            new Vector3(45f, 1f, 12.7f)
        };

        private Animator m_animator;
        private PlayerWalk m_playerWalk;
        private Rigidbody m_rigidbody;
        private Collider m_collider;
        private bool m_isDead = false;

        public event System.Action OnDeath;
        public event System.Action OnRespawn;
        
        private void Awake()
        {
            m_animator = GetComponentInChildren<Animator>();
            m_playerWalk = GetComponent<PlayerWalk>();
            m_rigidbody = GetComponent<Rigidbody>();
            m_collider = GetComponent<Collider>();
        }

        public void TakeDamage()
        {
            if (m_isDead) return;
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            m_isDead = true;

            // 1. Disable input immediately
            m_playerWalk.enabled = false;

            // Cancel any active throw aim so the arc line disappears
            PlayerThrow playerThrow = GetComponent<PlayerThrow>();
            if (playerThrow != null) playerThrow.CancelAim();

            // 2. Stop movement
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;

            OnDeath?.Invoke();
            AudioManager.Instance?.PlayDeathSFX();
            // 3. Play death animation
            if (m_animator != null)
            {
                m_animator.ResetTrigger("Die");
                m_animator.SetTrigger("Die");
            }

            // 4. Wait for animation to finish
            yield return new WaitForSeconds(deathAnimationLength);

            // 5. Hide player
            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            // Force-exit any task zone — OnTriggerExit won't fire when our collider is disabled
            GetComponent<PlayerTaskInteract>()?.ForceExit();
            if (m_collider != null)
                m_collider.enabled = false;

            // 6. Wait remaining respawn delay
            float remaining = respawnDelay - deathAnimationLength;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            // 7. Respawn — pick safe spawn point, show player, re-enable
            Vector3 spawnPos = GetSafeRespawnPoint();
            m_rigidbody.isKinematic = false; // clear any freeze effect that may have locked physics
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.MovePosition(spawnPos);
            transform.position = spawnPos;
            Debug.Log($"[PlayerHealth] Respawning at {spawnPos}");

            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderer.enabled = true;
            if (m_collider != null)
                m_collider.enabled = true;

            // Ensure arc line / landing circle are hidden even though renderers were re-enabled
            GetComponent<PlayerThrow>()?.CancelAim();

            // 8. Reset animator state
            if (m_animator != null)
            {
                m_animator.ResetTrigger("Die");
                m_animator.SetFloat("Speed", 0f);
                m_animator.SetFloat("Crouch", 0f);
            }

            m_playerWalk.enabled = true;
            m_isDead = false;

            OnRespawn?.Invoke();

            Debug.Log("[PlayerHealth] Player respawned!");
        }

        private Vector3 GetSafeRespawnPoint()
        {
            // Use scene spawn points from LevelManager if available
            List<GameObject> spawnPoints = LevelManager.instance != null ? LevelManager.instance.spawnPoints : null;

            if (spawnPoints != null && spawnPoints.Count > 0)
            {
                // Shuffle order
                int[] indices = new int[spawnPoints.Count];
                for (int i = 0; i < indices.Length; i++) indices[i] = i;
                for (int i = indices.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (indices[i], indices[j]) = (indices[j], indices[i]);
                }

                foreach (int i in indices)
                {
                    Vector3 pos = spawnPoints[i].transform.position;
                    if (alienLayer == 0) return pos; // layer not configured, just pick first
                    Collider[] hits = Physics.OverlapSphere(pos, alienSafeRadius, alienLayer);
                    if (hits.Length == 0) return pos;
                }

                // All points near aliens — pick random one anyway
                Debug.LogWarning("[PlayerHealth] No safe respawn point found, using random scene point");
                return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
            }

            // Fallback: hardcoded positions
            int[] fallbackIndices = new int[s_respawnPositions.Length];
            for (int i = 0; i < fallbackIndices.Length; i++) fallbackIndices[i] = i;
            for (int i = fallbackIndices.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (fallbackIndices[i], fallbackIndices[j]) = (fallbackIndices[j], fallbackIndices[i]);
            }

            foreach (int i in fallbackIndices)
            {
                Collider[] hits = Physics.OverlapSphere(s_respawnPositions[i], alienSafeRadius, alienLayer);
                if (hits.Length == 0) return s_respawnPositions[i];
            }

            Debug.LogWarning("[PlayerHealth] No safe respawn point found, using random fallback");
            return s_respawnPositions[Random.Range(0, s_respawnPositions.Length)];
        }
    }
}