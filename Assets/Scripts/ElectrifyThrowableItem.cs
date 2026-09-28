using UnityEngine;
using System.Collections;

namespace RowdyRobots
{
    public class ElectrifyThrowableItem : ThrowableItem
    {
        [Header("Electrify Settings")]
        [SerializeField] private float electrifyRadius = 2.5f;
        [SerializeField] private float electrifyDuration = 2f;
        [SerializeField] private LayerMask playerLayer;

        public override void OnLand(Vector3 position)
        {
            base.OnLand(position);

            // Electrify all players in radius (disable movement, re-enable after duration)
            Collider[] hits = Physics.OverlapSphere(position, electrifyRadius, playerLayer);
            foreach (var hit in hits)
            {
                // Drop BallPickup
                PlayerBallHolder holder = hit.GetComponent<PlayerBallHolder>();
                if (holder != null)
                {
                    BallPickup[] allBalls = FindObjectsByType<BallPickup>(FindObjectsSortMode.None);
                    foreach (var ball in allBalls)
                        if (ball.CurrentHolder == holder)
                            ball.Drop();
                }

                // Drop TransferBall
                if (holder != null)
                {
                    TransferBall[] allTransferBalls = FindObjectsByType<TransferBall>(FindObjectsSortMode.None);
                    foreach (var ball in allTransferBalls)
                        if (ball.CurrentHolder == holder)
                            ball.Drop();
                }

                // Disable movement
                PlayerWalk player = hit.GetComponent<PlayerWalk>();
                if (player != null && player.enabled)
                {
                    player.enabled = false;
                    // Optionally: trigger animation or VFX here
                    player.StartCoroutine(ReenablePlayerAfterDelay(player, electrifyDuration));
                }
            }
        }

        private IEnumerator ReenablePlayerAfterDelay(PlayerWalk player, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (player != null)
                player.enabled = true;
        }
    }
}

