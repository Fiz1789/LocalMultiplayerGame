using UnityEngine;
using System.Collections.Generic;

namespace RowdyRobots
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Win Settings")]
        [SerializeField] private int scoreToWin = 5;

        [Header("UI")]
        [SerializeField] private ScoreUI scoreUI;

        private Dictionary<GameObject, int> m_playerScores = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddScore(GameObject player, int amount = 1)
        {
            if (player == null) return;

            if (!m_playerScores.ContainsKey(player))
                m_playerScores[player] = 0;

            m_playerScores[player] += amount;
            Debug.Log($"[ScoreManager] {player.name} (ID:{player.GetInstanceID()}) score: {m_playerScores[player]}");

            if (m_playerScores[player] >= scoreToWin)
            {
                Debug.Log($"[ScoreManager] {player.name} WINS with {m_playerScores[player]} points!");
                AudioManager.Instance?.PlayGameWinSFX();
                if (scoreUI != null)
                    StartCoroutine(DelayedShowWinner(player));
            }
        }

        private System.Collections.IEnumerator DelayedShowWinner(GameObject player)
        {
            // Wait one frame so ScoreUI.Update can show the final icon first
            yield return null;
            scoreUI.ShowWinner(player);
        }

        public int GetScore(GameObject player)
        {
            if (player == null) return 0;
            return m_playerScores.ContainsKey(player) ? m_playerScores[player] : 0;
        }

        public Dictionary<GameObject, int> GetAllScores() => m_playerScores;
    }
}
