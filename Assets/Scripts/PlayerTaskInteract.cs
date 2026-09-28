using UnityEngine;
using UnityEngine.InputSystem;

namespace RowdyRobots
{
    public class PlayerTaskInteract : MonoBehaviour
    {
        private TaskZone m_currentZone;
        private PlayerThrow m_playerThrow;
        public bool IsInTaskZone => m_currentZone != null;
        private void Awake()
        {
            m_playerThrow = GetComponent<PlayerThrow>();
        }

        private void Update()
        {
            // If the zone was destroyed while the player was inside, OnTriggerExit never fires.
            // Detect this via the null-ified reference and restore throwing.
            if (m_currentZone == null && m_playerThrow != null && !m_playerThrow.ThrowingEnabled)
            {
                m_playerThrow.ThrowingEnabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TaskZone zone = other.GetComponent<TaskZone>();
            if (zone != null)
            {
                m_currentZone = zone;
                zone.OnPlayerEnter(this);

                // If currently aiming, let them finish the throw first
                if (m_playerThrow != null && !m_playerThrow.IsAiming)
                    m_playerThrow.ThrowingEnabled = false;

                Debug.Log("[PlayerTaskInteract] Entered task zone!");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TaskZone zone = other.GetComponent<TaskZone>();
            if (zone != null && m_currentZone == zone)
            {
                zone.OnPlayerExit(this);
                m_currentZone = null;

                // Re-enable throwing
                if (m_playerThrow != null)
                    m_playerThrow.ThrowingEnabled = true;

                Debug.Log("[PlayerTaskInteract] Left task zone!");
            }
        }

        // Called by Input System — Interact action
        public void OnInteract(InputValue value)
        {
            if (m_currentZone == null) return;
            if (!value.isPressed) return;

            m_currentZone.OnPress(this);
        }

        // Called on death — OnTriggerExit won't fire when our own collider is disabled
        public void ForceExit()
        {
            if (m_currentZone != null)
            {
                m_currentZone.OnPlayerExit(this);
                m_currentZone = null;
            }
            if (m_playerThrow != null)
                m_playerThrow.ThrowingEnabled = true;
        }
    }
}