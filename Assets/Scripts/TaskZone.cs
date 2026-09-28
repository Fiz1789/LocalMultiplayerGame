using UnityEngine;
using UnityEngine.UI;

namespace RowdyRobots
{
    public class TaskZone : MonoBehaviour
    {
        [Header("Task Settings")]
        [SerializeField] private float progressPerPress = 5f;    // How much each press fills
        [SerializeField] private float decayRate = 3f;           // How fast bar drains when not pressing
        [SerializeField] private float decayDelay = 1.5f;        // Seconds before bar starts draining

        [Header("UI")]
        [SerializeField] private Image buttonSprite;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image handleBaseImage;  // base/background image of the handle
        [SerializeField] private Image handleOuterImage; // outer colour layer of the handle
        [SerializeField] private Image handleInnerImage; // inner colour layer of the handle

        private float m_progress = 0f;
        private float m_decayTimer = 0f;
        private PlayerTaskInteract m_currentPlayer;
        private bool m_isBeingPressed = false;
        private bool m_isComplete = false;

        private void Start()
        {
            if (progressBar != null)
                progressBar.gameObject.SetActive(false);
            if (buttonSprite != null)
                buttonSprite.gameObject.SetActive(false);
        }
        private void Update()

        {
            if (m_isComplete) return;

            // Reset press flag every frame — OnPress sets it back to true if pressing
            m_isBeingPressed = false;

            // Decay if not being pressed
            if (!m_isBeingPressed)
            {
                m_decayTimer += Time.deltaTime;
                if (m_decayTimer >= decayDelay)
                {
                    m_progress -= decayRate * Time.deltaTime;
                    m_progress = Mathf.Max(0f, m_progress);
                }
            }
            else
            {
                m_decayTimer = 0f;
            }

            if (progressBar != null)
                progressBar.value = m_progress / 100f;

            // Keep outer and inner images at the same position as the handle base
            if (handleBaseImage != null)
            {
                Vector3 handlePos = handleBaseImage.transform.position;
                if (handleOuterImage != null) handleOuterImage.transform.position = handlePos;
                if (handleInnerImage != null) handleInnerImage.transform.position = handlePos;
            }

            if (m_progress >= 100f)
                CompleteTask();
            // Hide progress bar if no player is present and progress is zero
            if (progressBar != null && m_currentPlayer == null && m_progress == 0f && progressBar.gameObject.activeSelf)
                progressBar.gameObject.SetActive(false);
        }

        public void OnPlayerEnter(PlayerTaskInteract player)
        {
            if (m_currentPlayer == null)
            {
                m_currentPlayer = player;

                // Only tint handle for the player who takes control
                Color outerColor = GetPlayerOuterColor(player.gameObject);
                Color innerColor = GetPlayerInnerColor(player.gameObject);
                if (handleOuterImage != null) handleOuterImage.color = outerColor;
                if (handleInnerImage != null) handleInnerImage.color = innerColor;
            }

            if (progressBar != null)
                progressBar.gameObject.SetActive(true);
            if (buttonSprite != null)
                buttonSprite.gameObject.SetActive(true);
        }

        public void OnPlayerExit(PlayerTaskInteract player)
        {
            if (m_currentPlayer == player)
                m_currentPlayer = null;
            // Only hide progress bar if progress is zero
            if (progressBar != null && m_progress == 0f)
                progressBar.gameObject.SetActive(false);
            if (buttonSprite != null)
                buttonSprite.gameObject.SetActive(false);
        }

        public void OnPress(PlayerTaskInteract player)
        {
            if (m_isComplete) return;
            if (m_currentPlayer != player) return;

            m_isBeingPressed = true;
            m_progress += progressPerPress;
            m_progress = Mathf.Min(100f, m_progress);
            m_decayTimer = 0f;

            Debug.Log($"[TaskZone] Progress: {m_progress}%");
        }

        public void Interrupt()
        {
            m_progress = 0f;
            m_decayTimer = 0f;
            Debug.Log("[TaskZone] Task interrupted!");
        }

        private Color GetPlayerOuterColor(GameObject playerObj)
        {
            if (LevelManager.instance == null || GameManager.instance == null)
                return Color.white;

            int index = LevelManager.instance.playerList.FindIndex(p => p != null && p.gameObject == playerObj);
            if (index < 0 || index >= GameManager.instance.playerCharacterOuterColors.Count)
                return Color.white;

            Color c = GameManager.instance.playerCharacterOuterColors[index];
            c.a = 1f;
            return c;
        }

        private Color GetPlayerInnerColor(GameObject playerObj)
        {
            if (LevelManager.instance == null || GameManager.instance == null)
                return Color.white;

            int index = LevelManager.instance.playerList.FindIndex(p => p != null && p.gameObject == playerObj);
            if (index < 0 || index >= GameManager.instance.playerCharacterInnerColors.Count)
                return Color.white;

            Color c = GameManager.instance.playerCharacterInnerColors[index];
            c.a = 1f;
            return c;
        }

        private void CompleteTask()
        {
            m_isComplete = true;
            AudioManager.Instance?.PlayTaskCompletedSFX();
            Debug.Log("[TaskZone] Task complete!");

            if (m_currentPlayer != null && ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(m_currentPlayer.gameObject);

            Destroy(gameObject);
        }
    }
}