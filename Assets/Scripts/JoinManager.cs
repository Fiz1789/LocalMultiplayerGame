using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace RowdyRobots
{
    public class JoinManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] playerPrefabs; // 4 prefabs in order
        [SerializeField] private Transform[] spawnPoints; // 4 spawn points

        private PlayerInputManager m_playerInputManager;
        private int m_joinCount = 0;

        private void Awake()
        {
            m_playerInputManager = GetComponent<PlayerInputManager>();
            m_playerInputManager.onPlayerJoined += OnPlayerJoined;
        }

        private void OnDestroy()
        {
            m_playerInputManager.onPlayerJoined -= OnPlayerJoined;
        }

        private void OnPlayerJoined(PlayerInput playerInput)
        {
            int index = m_joinCount;
            m_joinCount++;

            // Set spawn position
            if (spawnPoints != null && index < spawnPoints.Length)
                playerInput.transform.position = spawnPoints[index].position;

            // Swap texture from correct prefab
            if (playerPrefabs != null && index < playerPrefabs.Length)
            {
                // Get renderer from the joining player
                Renderer playerRenderer = playerInput.GetComponentInChildren<Renderer>();
                // Get renderer from the correct prefab
                Renderer prefabRenderer = playerPrefabs[index].GetComponentInChildren<Renderer>();

                if (playerRenderer != null && prefabRenderer != null)
                    playerRenderer.material = prefabRenderer.sharedMaterial;
            }

            if (m_joinCount >= 4)
                m_playerInputManager.DisableJoining();

            Debug.Log($"[JoinManager] Player {index + 1} joined!");
        }
    }
}