using UnityEngine;

namespace RowdyRobots
{
    [CreateAssetMenu(fileName = "KnockbackEffect", menuName = "RowdyRobots/Item Effects/Knockback")]
    public class KnockbackEffect : ThrowableItemEffect
    {
        [Header("Knockback Settings")]
        public float radius = 3f;
        public float force = 12f;
        public float upwardForce = 4f;
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

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (hit.transform.position - position).normalized;
                    dir.y = 0f;
                    Vector3 impulse = dir * force + Vector3.up * upwardForce;
                    rb.AddForce(impulse, ForceMode.Impulse);
                }
            }
        }
    }
}
