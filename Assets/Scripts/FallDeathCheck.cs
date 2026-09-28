using UnityEngine;

namespace RowdyRobots
{
    public class FallDeathCheck : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage();
        }
    }
}
