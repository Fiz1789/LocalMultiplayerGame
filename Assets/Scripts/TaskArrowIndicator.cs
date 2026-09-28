using System.Collections.Generic;
using UnityEngine;

namespace RowdyRobots
{
    public class TaskArrowIndicator : MonoBehaviour
    {
        [Header("Arrow Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float orbitRadius = 1.5f;   // how far from the player center
        [SerializeField] private float heightOffset = 2f;    // how high above the player

        private Quaternion m_rotationOffset;
        private Vector3 m_prefabScale;
        private readonly List<GameObject> m_arrows = new();
        private PlayerBallHolder m_ballHolder;
        private bool m_isHidden = false;

        public void Hide() { m_isHidden = true; }
        public void Show() { m_isHidden = false; }

        private void Start()
        {
            if (arrowPrefab == null) return;
            m_rotationOffset = arrowPrefab.transform.rotation;
            m_prefabScale    = arrowPrefab.transform.localScale;
            m_ballHolder     = GetComponent<PlayerBallHolder>();
        }

        private void Update()
        {
            if (arrowPrefab == null) return;

            TaskLifetime[] tasks = FindObjectsByType<TaskLifetime>(FindObjectsSortMode.None);
            int count = tasks != null ? tasks.Length : 0;

            // Grow pool
            while (m_arrows.Count < count)
                m_arrows.Add(Instantiate(arrowPrefab));

            // Shrink pool
            while (m_arrows.Count > count)
            {
                Destroy(m_arrows[m_arrows.Count - 1]);
                m_arrows.RemoveAt(m_arrows.Count - 1);
            }

            Vector3 basePos = transform.position + Vector3.up * heightOffset;

            for (int i = 0; i < count; i++)
            {
                Transform target = GetTaskTarget(tasks[i]);

                Vector3 dir = target.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.001f) dir = Vector3.forward;
                dir.Normalize();

                // Position: orbit around player in world space (ignores player rotation)
                m_arrows[i].transform.position  = basePos + dir * orbitRadius;
                // Rotation: face the task direction + preserve prefab offset
                m_arrows[i].transform.rotation  = Quaternion.LookRotation(dir) * m_rotationOffset;
                // Scale: always match the original prefab scale regardless of player scale
                m_arrows[i].transform.localScale = m_prefabScale;
                m_arrows[i].SetActive(!m_isHidden);
            }
        }

        /// <summary>
        /// Returns the world-space Transform to point at for this task.
        /// For TransferBall: points at the ball until THIS player picks it up,
        /// then points at the drop-off zone.
        /// </summary>
        private Transform GetTaskTarget(TaskLifetime task)
        {
            TransferBall ball = task.GetComponentInChildren<TransferBall>(includeInactive: true);
            if (ball != null)
            {
                // This player is holding the ball — point to the drop zone
                if (ball.IsHeld && ball.CurrentHolder == m_ballHolder && ball.Zone != null)
                    return ball.Zone.transform;

                // Not held, or held by someone else — point to the ball itself
                return ball.transform;
            }

            return task.transform;
        }

        private void OnDestroy()
        {
            foreach (var arrow in m_arrows)
                if (arrow != null) Destroy(arrow);
            m_arrows.Clear();
        }
    }
}
