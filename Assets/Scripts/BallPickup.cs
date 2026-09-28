using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace RowdyRobots
{
    public class BallPickup : MonoBehaviour
    {
        public PlayerBallHolder CurrentHolder => m_currentHolder;
        [Header("Orbit Settings")]
        [SerializeField] private float orbitRadius = 1f;
        [SerializeField] private float orbitSpeed = 90f;
        [SerializeField] private float orbitHeight = 1f;

        [Header("Hold Task Settings")]
        [SerializeField] private float holdDuration = 3f; // Seconds to hold before complete

        [Header("Decay Settings")]
        [SerializeField] private float decayRate = 1.5f; // How fast progress decays per second
        [SerializeField] private float decayDelay = 1.0f; // Seconds before decay starts
        [Header("UI")]
        [SerializeField] private Slider progressBar;
        private float m_decayTimer = 0f;
        private PlayerBallHolder m_currentHolder;
        private bool m_isHeld = false;
        private float m_orbitAngle = 0f;
        private float m_holdTimer = 0f;
        private bool m_isComplete = false;


        // private float m_scoreTimer = 0f;
        // private Dictionary<PlayerBallHolder, int> m_playerScores = new();

        private void Update()
        {
            // Hide progress bar only if progress is zero and not held
            if (progressBar != null && !m_isHeld && m_holdTimer == 0f && progressBar.gameObject.activeSelf)
                progressBar.gameObject.SetActive(false);
            // Update progress bar value
            if (progressBar != null)
                progressBar.value = Mathf.Clamp01(m_holdTimer / holdDuration);
            if (m_isComplete) return;

            if (m_isHeld && m_currentHolder != null)
            {
                // Orbit
                m_orbitAngle += orbitSpeed * Time.deltaTime;
                float x = Mathf.Cos(m_orbitAngle * Mathf.Deg2Rad) * orbitRadius;
                float z = Mathf.Sin(m_orbitAngle * Mathf.Deg2Rad) * orbitRadius;
                transform.position = m_currentHolder.transform.position + new Vector3(x, orbitHeight, z);

                // Progress increases while held
                m_holdTimer += Time.deltaTime;
                m_decayTimer = 0f;
                if (m_holdTimer >= holdDuration)
                {
                    CompleteTask();
                }
            }
            else if (m_holdTimer > 0f)
            {
                // Not held: decay progress after delay
                m_decayTimer += Time.deltaTime;
                if (m_decayTimer >= decayDelay)
                {
                    m_holdTimer -= decayRate * Time.deltaTime;
                    m_holdTimer = Mathf.Max(0f, m_holdTimer);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_isHeld) return;

            PlayerBallHolder holder = other.GetComponent<PlayerBallHolder>();
            if (holder != null && !holder.IsHoldingAny)
                PickUp(holder);
        }

        public void PickUp(PlayerBallHolder holder)
        {
            // Disable sprint
            PlayerWalk walk = holder.GetComponent<PlayerWalk>();
            if (walk != null)
                walk.SprintEnabled = false;

            if (progressBar != null)
                progressBar.gameObject.SetActive(true);
            m_currentHolder = holder;
            m_isHeld = true;
            AudioManager.Instance?.PlayPickupItemSFX();
            // Do not reset m_holdTimer here, so progress is preserved if picked up again
            m_decayTimer = 0f;
            m_isComplete = false;
            holder.SetHeldBall(this);

            // Hide arrows on all OTHER players while this ball is held
            foreach (var indicator in FindObjectsByType<TaskArrowIndicator>(FindObjectsSortMode.None))
                if (indicator.gameObject != holder.gameObject) indicator.Hide();

            Debug.Log($"[Ball] Picked up by {holder.name}");
        }

        public void Drop()
        {
            // Re-enable sprint
            if (m_currentHolder != null)
            {
                PlayerWalk walk = m_currentHolder.GetComponent<PlayerWalk>();
                if (walk != null)
                    walk.SprintEnabled = true;
            }
            // Do not hide progress bar here; handled in Update()
            if (!m_isHeld) return;

            if (m_currentHolder != null)
                m_currentHolder.SetHeldBall(null); // clear holder's reference

            m_isHeld = false;
            m_currentHolder = null;
            // Do not reset m_holdTimer here; allow decay
            m_decayTimer = 0f;
            m_isComplete = false;

            // Show all arrows again now ball is free
            foreach (var indicator in FindObjectsByType<TaskArrowIndicator>(FindObjectsSortMode.None))
                indicator.Show();

            // Ball just stays at its last orbit position
            Debug.Log("[Ball] Dropped!");
        }

        private void CompleteTask()
        {
            if (progressBar != null)
                progressBar.gameObject.SetActive(false);
            m_isComplete = true;
            AudioManager.Instance?.PlayTaskCompletedSFX();
            Debug.Log("[BallPickup] Hold task complete!");

            if (m_currentHolder != null && ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(m_currentHolder.gameObject);

            Drop();
            Destroy(gameObject);
        }
    }

    // public int GetScore(PlayerBallHolder player)
    // {
    //     return m_playerScores.ContainsKey(player) ? m_playerScores[player] : 0;
    // }

    // public Dictionary<PlayerBallHolder, int> GetAllScores() => m_playerScores;
}
