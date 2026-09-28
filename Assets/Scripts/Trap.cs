using UnityEngine;
using System.Collections;

namespace RowdyRobots
{
    public class Trap : MonoBehaviour
    {
        [Header("Trap Cooldown")]
        public float trapCooldown = 2f;
        private float lastTrapTime = -Mathf.Infinity;
        private TrapSpawner m_spawner;

        public float activationDelay = 2f;
        public Color triggeredColor = Color.red;
        public float disappearDelay = 2f;

        private Collider trapCollider;
        private PlayerWalk m_TrappedPlayer; // Store reference for re-enable

        private void Start()
        {
            trapCollider = GetComponent<Collider>();
            if (trapCollider != null)
            {
                trapCollider.enabled = false;
                Invoke(nameof(EnableTrap), activationDelay);
            }
        }
        public void SetSpawner(TrapSpawner spawner)
        {
            m_spawner = spawner;
        }

        private void OnDestroy()
        {
            if (m_spawner != null)
                m_spawner.OnTrapDestroyed();
        }
        private void EnableTrap()
        {
            if (trapCollider != null)
            {
                trapCollider.enabled = true;
            }

            // Hide renderer immediately once trap activates
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (Time.time - lastTrapTime < trapCooldown) return;
            lastTrapTime = Time.time;

            // Drop BallPickup if holding
            BallPickup ball = other.GetComponentInChildren<BallPickup>();
            if (ball == null)
                ball = FindBallHeldBy(other.GetComponent<PlayerBallHolder>());
            if (ball != null)
                ball.Drop();

            // Drop TransferBall if holding
            TransferBall transferBall = FindTransferBallHeldBy(other.GetComponent<PlayerBallHolder>());
            if (transferBall != null)
                transferBall.Drop();

            var playerWalk = other.GetComponent<PlayerWalk>();
            if (playerWalk != null)
            {
                // Disable movement
                playerWalk.enabled = false;
                m_TrappedPlayer = playerWalk;

                // Make noise
                float trapNoise = 5f; // Adjust as needed
                m_TrappedPlayer.SetNoiseAndRadius(trapNoise);
                Debug.Log($"[Trap] Player noise set to {trapNoise}");

                Animator playerAnimator = other.GetComponentInChildren<Animator>();
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("Trapped");
                    Debug.Log("[Trap] Triggered Trapped animation");
                }
                // ===========================================

                // Re-enable movement SLIGHTLY BEFORE trap is destroyed
                float reenableTime = Mathf.Max(0f, disappearDelay - 0.1f);
                Invoke(nameof(ReenableMovement), reenableTime);
            }


            // Visual feedback
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                renderer.material.color = triggeredColor;
            }

            // Destroy trap AFTER player is re-enabled
            Destroy(gameObject, disappearDelay);
        }
        private void ReenableMovement()
        {
            if (m_TrappedPlayer != null)
            {
                m_TrappedPlayer.enabled = true;
                Debug.Log("[Trap] Player movement re-enabled");
                m_TrappedPlayer = null;
            }
        }
        private BallPickup FindBallHeldBy(PlayerBallHolder holder)
        {
            if (holder == null) return null;
            BallPickup[] allBalls = FindObjectsByType<BallPickup>(FindObjectsSortMode.None);
            foreach (var ball in allBalls)
                if (ball.CurrentHolder == holder)
                    return ball;
            return null;
        }

        private TransferBall FindTransferBallHeldBy(PlayerBallHolder holder)
        {
            if (holder == null) return null;
            TransferBall[] allBalls = FindObjectsByType<TransferBall>(FindObjectsSortMode.None);
            foreach (var ball in allBalls)
                if (ball.CurrentHolder == holder)
                    return ball;
            return null;
        }
    }


}