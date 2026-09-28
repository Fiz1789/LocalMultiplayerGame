using UnityEngine;

namespace RowdyRobots
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";
        [SerializeField] private string closeTrigger = "Close";

        private void OnTriggerEnter(Collider other)
        {
            animator.SetTrigger(openTrigger);
        }

        private void OnTriggerExit(Collider other)
        {
            animator.SetTrigger(closeTrigger);
        }
    }
}
