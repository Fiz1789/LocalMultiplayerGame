using UnityEngine;

namespace RowdyRobots
{

    public class PlayerBallHolder : MonoBehaviour
    {
        private BallPickup m_heldBall;
        private TransferBall m_heldTransferBall;
        private PlayerHealth m_playerHealth;

        private void Awake()
        {
            m_playerHealth = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            // Listen for death to drop ball
            if (m_playerHealth != null)
                m_playerHealth.OnDeath += DropBall;
        }

        private void OnDisable()
        {
            if (m_playerHealth != null)
                m_playerHealth.OnDeath -= DropBall;
        }

        public bool IsHoldingAny => m_heldBall != null || m_heldTransferBall != null;

        public void SetHeldBall(BallPickup ball)
        {
            m_heldBall = ball;
        }

        public void SetHeldTransferBall(TransferBall ball)
        {
            m_heldTransferBall = ball;
        }

        private void DropBall()
        {
            if (m_heldBall != null)
            {
                m_heldBall.Drop();
                m_heldBall = null;
            }
            if (m_heldTransferBall != null)
            {
                m_heldTransferBall.Drop();
                m_heldTransferBall = null;
            }
        }
    }
}