using UnityEngine;

namespace RowdyRobots
{
    public class TransferBall : MonoBehaviour
    {
        [Header("Transfer Zone")]
        [SerializeField] private TransferZone transferZone;
        [Header("Orbit Settings")]
        [SerializeField] private float orbitRadius = 1f;
        [SerializeField] private float orbitSpeed = 90f;
        [SerializeField] private float orbitHeight = 1f;

        private PlayerBallHolder m_currentHolder;
        private bool m_isHeld = false;
        private float m_orbitAngle = 0f;

        public bool IsHeld => m_isHeld;
        public TransferZone Zone => transferZone;

        private void Update()
        {
            if (!m_isHeld || m_currentHolder == null) return;

            m_orbitAngle += orbitSpeed * Time.deltaTime;
            float x = Mathf.Cos(m_orbitAngle * Mathf.Deg2Rad) * orbitRadius;
            float z = Mathf.Sin(m_orbitAngle * Mathf.Deg2Rad) * orbitRadius;
            transform.position = m_currentHolder.transform.position + new Vector3(x, orbitHeight, z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_isHeld) return;

            PlayerBallHolder holder = other.GetComponent<PlayerBallHolder>();
            if (holder != null && !holder.IsHoldingAny)
                PickUp(holder);
        }

        public void PickUp(PlayerBallHolder holder)
        {
            m_currentHolder = holder;
            m_isHeld = true;

            // Disable sprint
            PlayerWalk walk = holder.GetComponent<PlayerWalk>();
            if (walk != null)
                walk.SprintEnabled = false;

            // Register with holder so it drops on death
            holder.SetHeldTransferBall(this);

            // Activate transfer zone
            if (transferZone != null)
                transferZone.gameObject.SetActive(true);

            // Hide THIS player's task arrows — they only need to deliver the ball
            SetHolderArrows(holder, hide: true);

            Debug.Log($"[TransferObject] Picked up by {holder.name}");
        }

        public void Drop()
        {
            if (!m_isHeld) return;

            // Re-enable sprint
            if (m_currentHolder != null)
            {
                PlayerWalk walk = m_currentHolder.GetComponent<PlayerWalk>();
                if (walk != null)
                    walk.SprintEnabled = true;
                m_currentHolder.SetHeldTransferBall(null);
            }

            m_isHeld = false;
            m_currentHolder = null;

            // Show all arrows again now ball is free
            SetAllArrows(hide: false);
            // Debug.Log("[TransferObject] Dropped!");
        }

        private void SetHolderArrows(PlayerBallHolder holder, bool hide)
        {
            foreach (var indicator in FindObjectsByType<TaskArrowIndicator>(FindObjectsSortMode.None))
            {
                if (indicator.gameObject != holder.gameObject) continue;
                if (hide) indicator.Hide(); else indicator.Show();
            }
        }

        private void SetAllArrows(bool hide)
        {
            foreach (var indicator in FindObjectsByType<TaskArrowIndicator>(FindObjectsSortMode.None))
            {
                if (hide) indicator.Hide(); else indicator.Show();
            }
        }

        public PlayerBallHolder CurrentHolder => m_currentHolder;
    }
}