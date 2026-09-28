using System.Collections;
using UnityEngine;

namespace RowdyRobots
{
    [CreateAssetMenu(fileName = "ElectrifyEffect", menuName = "RowdyRobots/Item Effects/Electrify")]
    public class ElectrifyEffect : ThrowableItemEffect
    {
        [Header("Electrify Settings")]
        public float radius = 2.5f;
        public float stunDuration = 2f;
        public LayerMask playerLayer;

        public override void Apply(Vector3 position, MonoBehaviour runner)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, playerLayer);
            foreach (var hit in hits)
            {
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

                PlayerWalk player = hit.GetComponent<PlayerWalk>();
                if (player != null && player.enabled)
                {
                    player.enabled = false;
                    // Run coroutine on player (not runner) so it survives item destruction
                    player.StartCoroutine(ReenableAfterDelay(player, stunDuration));
                }
            }
        }

        private IEnumerator ReenableAfterDelay(PlayerWalk player, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (player != null)
                player.enabled = true;
        }
    }
}
