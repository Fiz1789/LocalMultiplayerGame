using UnityEngine;

namespace RowdyRobots
{
    public class TaskLifetime : MonoBehaviour
    {
        private TaskSpawner m_spawner;
        private bool m_isTaskZone;
        private int m_taskType = -1;

        public void Init(TaskSpawner spawner, bool isTaskZone = false, int taskType = -1)
        {
            m_spawner = spawner;
            m_isTaskZone = isTaskZone;
            m_taskType = taskType;
        }

        private void OnDestroy()
        {
            if (m_spawner != null)
                m_spawner.OnTaskDestroyed(m_isTaskZone, m_taskType);
        }
    }
}
