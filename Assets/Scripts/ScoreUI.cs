using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RowdyRobots
{
    public class ScoreUI : MonoBehaviour
    {
        [Header("Player Panels (outer + inner per player)")]
        [SerializeField] private Image[] player1Panels; // outer panel, inner panel
        [SerializeField] private Image[] player2Panels;
        [SerializeField] private Image[] player3Panels;
        [SerializeField] private Image[] player4Panels;

        [Header("Score Icons Per Player (SetActive based)")]
        [SerializeField] private GameObject[] player1Icons; // 5 icons
        [SerializeField] private GameObject[] player2Icons;
        [SerializeField] private GameObject[] player3Icons;
        [SerializeField] private GameObject[] player4Icons;

        [Header("Trap UI Per Player")]
        [SerializeField] private Image[] player1TrapIcons; // trap1, trap2
        [SerializeField] private Image[] player2TrapIcons;
        [SerializeField] private Image[] player3TrapIcons;
        [SerializeField] private Image[] player4TrapIcons;

        [Header("Trap UI Sprites")]
        [SerializeField] private Sprite trapOnSprite;
        [SerializeField] private Sprite trapOffSprite; // placed/used

        [Header("Trap Cooldown Animation")]
        [SerializeField] private float cooldownSlideDistance = 40f;

        [Header("Win")]
        [SerializeField] private TextMeshProUGUI winText;

        private GameObject[] players;
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private float returnToMenuDelay = 3f;
        private bool m_gameOver = false;
        private int[] m_lastScores;
        private int[] m_lastTrapsPlaced;
        private Dictionary<Image, float> m_trapIconOriginalY = new();

        private void Start()
        {
            if (winText != null)
                winText.gameObject.SetActive(false);

            // Get players from LevelManager in correct spawn order
            if (LevelManager.instance != null)
            {
                players = new GameObject[LevelManager.instance.playerList.Count];
                for (int i = 0; i < LevelManager.instance.playerList.Count; i++)
                    players[i] = LevelManager.instance.playerList[i].gameObject;
            }
            else
            {
                PlayerWalk[] allPlayers = FindObjectsByType<PlayerWalk>(FindObjectsSortMode.InstanceID);
                players = new GameObject[allPlayers.Length];
                for (int i = 0; i < allPlayers.Length; i++)
                    players[i] = allPlayers[i].gameObject;
            }

            m_lastScores = new int[players.Length];
            m_lastTrapsPlaced = new int[players.Length];

            Debug.Log($"[ScoreUI] Found {players.Length} players");

            // Diagnostic warnings
            GameObject[][] allIcons = { player1Icons, player2Icons, player3Icons, player4Icons };
            Image[][] allTrapIcons = { player1TrapIcons, player2TrapIcons, player3TrapIcons, player4TrapIcons };
            for (int i = 0; i < players.Length; i++)
            {
                Debug.Log($"[ScoreUI] Player {i} = {players[i].name} (ID:{players[i].GetInstanceID()})");
                if (allIcons[i] == null || allIcons[i].Length == 0)
                    Debug.LogWarning($"[ScoreUI] player{i + 1}Icons is empty! Score UI won't work for P{i + 1}");
                if (allTrapIcons[i] == null || allTrapIcons[i].Length == 0)
                    Debug.LogWarning($"[ScoreUI] player{i + 1}TrapIcons is empty! Trap UI won't work for P{i + 1}");
                PlayerWalk pw = players[i].GetComponent<PlayerWalk>();
                if (pw != null && pw.trapSpawner == null)
                    Debug.LogWarning($"[ScoreUI] Player {i} ({players[i].name}) has no TrapSpawner assigned on PlayerWalk!");
            }

            // Apply character sprites and panel colors from character selection
            if (GameManager.instance != null)
            {
                var outerColors = GameManager.instance.playerCharacterOuterColors;
                var innerColors = GameManager.instance.playerCharacterInnerColors;
                Image[][] panels = { player1Panels, player2Panels, player3Panels, player4Panels };

                for (int i = 0; i < panels.Length; i++)
                {
                    if (panels[i] != null)
                    {
                        // panels[0] = outer panel, panels[1] = inner panel
                        if (panels[i].Length > 0 && panels[i][0] != null && outerColors != null && i < outerColors.Count)
                        {
                            Color c = outerColors[i];
                            c.a = 1f;
                            panels[i][0].color = c;
                        }
                        if (panels[i].Length > 1 && panels[i][1] != null && innerColors != null && i < innerColors.Count)
                        {
                            Color c = innerColors[i];
                            c.a = 1f;
                            panels[i][1].color = c;
                        }
                    }
                }
            }

            // Hide all score icons at start
            HideAllIcons(player1Icons);
            HideAllIcons(player2Icons);
            HideAllIcons(player3Icons);
            HideAllIcons(player4Icons);

            // Set all trap icons to "on" sprite at start and cache original Y positions
            SetTrapIcons(player1TrapIcons, 0);
            SetTrapIcons(player2TrapIcons, 0);
            SetTrapIcons(player3TrapIcons, 0);
            SetTrapIcons(player4TrapIcons, 0);
            CacheOriginalY(player1TrapIcons);
            CacheOriginalY(player2TrapIcons);
            CacheOriginalY(player3TrapIcons);
            CacheOriginalY(player4TrapIcons);
        }

        private void Update()
        {
            if (m_gameOver || ScoreManager.Instance == null) return;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;

                // Score icons
                int score = ScoreManager.Instance.GetScore(players[i]);
                if (score != m_lastScores[i])
                {
                    m_lastScores[i] = score;
                    GameObject[] icons = GetIconsForPlayer(i);
                    Debug.Log($"[ScoreUI] P{i + 1} score changed to {score}, icons array: {(icons == null ? "NULL" : icons.Length.ToString())}");
                    UpdateScoreIcons(icons, score);
                }

                    // Trap UI
                TrapSpawner trapSpawner = players[i].GetComponent<PlayerWalk>()?.trapSpawner;
                if (trapSpawner != null)
                {
                    Image[] trapIcons = GetTrapIconsForPlayer(i);
                    int trapsPlaced = trapSpawner.TrapsPlaced;

                    if (trapsPlaced != m_lastTrapsPlaced[i])
                    {
                        m_lastTrapsPlaced[i] = trapsPlaced;
                        Debug.Log($"[ScoreUI] P{i + 1} traps changed to {trapsPlaced}, trapIcons: {(trapIcons == null ? "NULL" : trapIcons.Length.ToString())}");
                        SetTrapIcons(trapIcons, trapsPlaced);
                    }

                    // Cooldown animation on next trap icon
                    if (trapSpawner.IsOnCooldown() && trapIcons != null && trapsPlaced < trapIcons.Length)
                    {
                        float elapsed = Time.time - trapSpawner.LastTrapTime;
                        float t = Mathf.Clamp01(elapsed / trapSpawner.trapCooldown);

                        // P1/P2 (index 0,1) = top UI → start above, slide down
                        // P3/P4 (index 2,3) = bottom UI → start below, slide up
                        float direction = (i < 2) ? 1f : -1f;
                        float offset = Mathf.Lerp(cooldownSlideDistance * direction, 0f, t);

                        RectTransform rt = trapIcons[trapsPlaced].GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            float origY = GetOriginalY(trapIcons[trapsPlaced]);
                            Vector2 pos = rt.anchoredPosition;
                            pos.y = origY + offset;
                            rt.anchoredPosition = pos;
                        }
                    }
                    else if (trapIcons != null)
                    {
                        // Reset all icons to original Y when not on cooldown
                        for (int j = 0; j < trapIcons.Length; j++)
                        {
                            if (trapIcons[j] == null) continue;
                            RectTransform rt = trapIcons[j].GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                float origY = GetOriginalY(trapIcons[j]);
                                Vector2 pos = rt.anchoredPosition;
                                pos.y = origY;
                                rt.anchoredPosition = pos;
                            }
                        }
                    }
                }
            }
        }

        // ── Score Icons ────────────────────────────────────

        private GameObject[] GetIconsForPlayer(int index)
        {
            return index switch
            {
                0 => player1Icons,
                1 => player2Icons,
                2 => player3Icons,
                3 => player4Icons,
                _ => null
            };
        }

        private void HideAllIcons(GameObject[] icons)
        {
            if (icons == null) return;
            foreach (var icon in icons)
                if (icon != null) icon.SetActive(false);
        }

        private void UpdateScoreIcons(GameObject[] icons, int score)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                    icons[i].SetActive(i < score);
            }
        }

        // ── Trap Icons ─────────────────────────────────────

        private void CacheOriginalY(Image[] icons)
        {
            if (icons == null) return;
            foreach (var icon in icons)
            {
                if (icon == null) continue;
                RectTransform rt = icon.GetComponent<RectTransform>();
                if (rt != null)
                    m_trapIconOriginalY[icon] = rt.anchoredPosition.y;
            }
        }

        private float GetOriginalY(Image icon)
        {
            return m_trapIconOriginalY.TryGetValue(icon, out float y) ? y : 0f;
        }

        private Image[] GetTrapIconsForPlayer(int index)
        {
            return index switch
            {
                0 => player1TrapIcons,
                1 => player2TrapIcons,
                2 => player3TrapIcons,
                3 => player4TrapIcons,
                _ => null
            };
        }

        private void SetTrapIcons(Image[] icons, int trapsPlaced)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] == null) continue;
                icons[i].sprite = (i < trapsPlaced) ? trapOffSprite : trapOnSprite;
            }
        }

        public void ShowWinner(GameObject winner)
        {
            if (m_gameOver) return;
            m_gameOver = true;

            // Find player number
            int playerNumber = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == winner)
                {
                    playerNumber = i + 1;
                    break;
                }
            }

            if (winText != null)
            {
                winText.text = $"Player {playerNumber} Wins!";
                winText.gameObject.SetActive(true);
            }

            // Disable all players
            foreach (var player in FindObjectsByType<PlayerWalk>(FindObjectsSortMode.None))
                player.enabled = false;

            StartCoroutine(ReturnToMainMenu());
        }

        private System.Collections.IEnumerator ReturnToMainMenu()
        {
            yield return new WaitForSeconds(returnToMenuDelay);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}


// colours: outerpanel blue- 53,197,252 innerpanel blue - 0,255,255
// outerpanel yellow - 249,176,45 innerpanel yellow - 255,247,0
// outerpanel green - 53,252,159 innerpanel green - 29,255,0