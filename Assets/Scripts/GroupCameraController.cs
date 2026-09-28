// ─────────────────────────────────────────────────────────────────────────────
// GroupCameraController — top-down multiplayer camera
//
// Scene setup:
//   1. Add a CinemachineTargetGroup component to an empty GameObject.
//      Assign it to the "Target Group" field below.
//   2. On your CinemachineCamera:
//        • Set "Follow"  → the CinemachineTargetGroup GameObject
//        • Set "Look At" → the CinemachineTargetGroup GameObject
//        • Add the "Cinemachine Group Framing" extension
//          – set Max Ortho Size (or Max FOV) = 70  (your "range 70")
//        • Lock rotation X to 80 in the camera's Body settings
//          (use "Do Nothing" for vertical aim, set it manually)
//   3. Attach this script to any persistent GameObject in the battle scene.
//      Assign the CinemachineTargetGroup to the Inspector field.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RowdyRobots
{
    public class GroupCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup targetGroup;

        [Header("Target Settings")]
        [SerializeField] private float memberWeight = 1f;
        [SerializeField] private float memberRadius = 2f;

        [Header("Alien Tracking")]
        [SerializeField] private float alienWeight = 0.5f;
        [SerializeField] private float alienRadius = 1f;

        private readonly List<PlayerHealth> m_trackedPlayers = new();
        private readonly HashSet<Transform> m_trackedAliens = new();

        private IEnumerator Start()
        {
            // Wait until LevelManager has finished spawning players
            yield return new WaitUntil(() =>
                LevelManager.instance != null &&
                LevelManager.instance.playerList.Count > 0);

            foreach (var player in LevelManager.instance.playerList)
            {
                if (player == null) continue;

                targetGroup.AddMember(player.transform, memberWeight, memberRadius);

                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    m_trackedPlayers.Add(health);
                    Transform t = player.transform;
                    health.OnDeath    += () => targetGroup.RemoveMember(t);
                    health.OnRespawn  += () => targetGroup.AddMember(t, memberWeight, memberRadius);
                }
            }
        }

        private void Update()
        {
            // Track any newly spawned aliens
            AlienAttack[] aliens = FindObjectsByType<AlienAttack>(FindObjectsSortMode.None);
            foreach (var alien in aliens)
            {
                if (alien == null) continue;
                if (m_trackedAliens.Add(alien.transform))
                    targetGroup.AddMember(alien.transform, alienWeight, alienRadius);
            }

            // Remove destroyed aliens
            m_trackedAliens.RemoveWhere(t =>
            {
                if (t == null)
                    return true;
                // Check if the GameObject was destroyed
                if (!t.gameObject.activeInHierarchy)
                {
                    targetGroup.RemoveMember(t);
                    return true;
                }
                return false;
            });
        }

        private void OnDestroy()
        {
            foreach (var health in m_trackedPlayers)
            {
                if (health == null) continue;
                health.OnDeath    -= null;
                health.OnRespawn  -= null;
            }
        }
    }
}
