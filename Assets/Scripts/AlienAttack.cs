using UnityEngine;

namespace RowdyRobots
{
    public class AlienAttack : MonoBehaviour
    {
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private LayerMask playerLayer;

        public void DealDamage()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
            foreach (var hit in hits)
            {
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    AudioManager.Instance?.PlayAlienAttackSFX();
                    health.TakeDamage();
                }
            }
        }
    }
}