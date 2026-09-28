using System.Collections;
using UnityEngine;

namespace RowdyRobots
{
    [CreateAssetMenu(fileName = "FreezeEffect", menuName = "RowdyRobots/Item Effects/Freeze")]
    public class FreezeEffect : ThrowableItemEffect
    {
        [Header("Freeze Settings")]
        public float radius = 2.5f;
        public float freezeDuration = 3f;
        public LayerMask playerLayer;

        public override void Apply(Vector3 position, MonoBehaviour runner)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, playerLayer);
            foreach (var hit in hits)
            {
                // Drop held balls
                PlayerBallHolder holder = hit.GetComponent<PlayerBallHolder>();
                if (holder != null)
                {
                    BallPickup[] allBalls = Object.FindObjectsByType<BallPickup>(FindObjectsSortMode.None);
                    foreach (var ball in allBalls)
                        if (ball.CurrentHolder == holder) ball.Drop();

                    TransferBall[] allTransferBalls = Object.FindObjectsByType<TransferBall>(FindObjectsSortMode.None);
                    foreach (var ball in allTransferBalls)
                        if (ball.CurrentHolder == holder) ball.Drop();
                }

                // Freeze movement and physics
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                PlayerWalk playerWalk = hit.GetComponent<PlayerWalk>();
                if (playerWalk != null && playerWalk.enabled)
                {
                    playerWalk.enabled = false;
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }
                    // Run on the player's own MonoBehaviour so it survives the item being destroyed
                    playerWalk.StartCoroutine(UnfreezeAfterDelay(playerWalk, rb, freezeDuration));
                }
            }
        }

        private IEnumerator UnfreezeAfterDelay(PlayerWalk player, Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (player != null)
            {
                if (rb != null) rb.isKinematic = false;
                player.enabled = true;
            }
        }
    }
}
