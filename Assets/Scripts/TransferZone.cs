using UnityEngine;

namespace RowdyRobots
{
    public class TransferZone : MonoBehaviour
    {
        private bool m_isComplete = false;

        private void Start()
        {
            // Ensure the zone starts inactive
            gameObject.SetActive(false);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (m_isComplete) return;

            // Check if player entering is holding the transfer object
            PlayerBallHolder holder = other.GetComponent<PlayerBallHolder>();
            if (holder == null) return;

            // Find the transfer object the player is holding
            TransferBall obj = FindTransferObjectHeldBy(holder);
            if (obj != null)
                CompleteTransfer(obj, holder);
        }

        private TransferBall FindTransferObjectHeldBy(PlayerBallHolder holder)
        {
            TransferBall[] allObjects = FindObjectsByType<TransferBall>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.CurrentHolder == holder)
                    return obj;
            }
            return null;
        }

        private void CompleteTransfer(TransferBall obj, PlayerBallHolder holder)
        {
            m_isComplete = true;

            obj.Drop();

            AudioManager.Instance?.PlayTaskCompletedSFX();
            Debug.Log($"[TransferZone] Transfer complete by {holder.name}!");

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(holder.gameObject);

            // Destroy the root task object (has TaskLifetime) which cleans up arrow + spawner
            TaskLifetime rootTask = obj.GetComponentInParent<TaskLifetime>();
            if (rootTask != null)
                Destroy(rootTask.gameObject);
            else
                Destroy(obj.gameObject);

            Destroy(gameObject);
        }
    }
}