using UnityEngine;
using System.Collections.Generic;

namespace RowdyRobots
{
    public class AlienVisionCone : MonoBehaviour
    {
        public enum DetectionType { None, Vision, CloseNoise, DistantNoise }
        [Header("Vision Settings")]
        [SerializeField] private float visionDistance = 10f;
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Noise Settings")]
        [SerializeField] public float noiseDistance = 15f;
        [SerializeField] private float minimumNoiseThreshold = 0.5f;

        [Header("Distance Noise Settings")]
        [SerializeField] private float distantNoiseDistance = 30f;
        [SerializeField] private float distantNoiseThreshold = 3.0f;

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color visionColor = Color.yellow;
        [SerializeField] private Color noiseColor = Color.blue;
        [SerializeField] private Color distantNoiseColor = Color.orange;
        [SerializeField] private Color visibleTargetColor = Color.green;
        [SerializeField] private Color priorityTargetColor = Color.red;


        private List<GameObject> visibleTargets = new();

        public LayerMask ObstacleLayer => obstacleLayer;
        public LayerMask TargetLayer => targetLayer;

        /*
        =========================
        VISION DETECTION
        =========================
        */

        public List<GameObject> GetVisibleTargets()
        {
            visibleTargets.Clear();

            Collider[] targets = Physics.OverlapSphere(transform.position, visionDistance, targetLayer);

            foreach (var collider in targets)
            {
                GameObject target = collider.gameObject;

                if (target.GetComponent<ThrowableItem>() != null)
                    continue;
                if (target == gameObject)
                    continue;

                Vector3 direction = (target.transform.position - transform.position);
                float distance = direction.magnitude;

                direction.Normalize();
                direction.y = 0;

                float angle = Vector3.Angle(transform.forward, direction);

                if (angle > viewAngle * 0.5f)
                    continue;

                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, obstacleLayer | targetLayer))
                {
                    // Debug.Log($"[AlienVisionCone] Raycast to {target.name} hit {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
                    if (hit.collider.gameObject == target || hit.collider.transform.root == target.transform.root)
                    {
                        visibleTargets.Add(target);
                    }
                }
                else
                {
                    // Debug.Log($"[AlienVisionCone] Raycast to {target.name} did not hit anything, assuming visible.");
                    visibleTargets.Add(target);
                }
            }

            return visibleTargets;
        }

        /*
        =========================
        NOISE DETECTION
        =========================
        */

        public GameObject GetNoisyTarget()
        {
            Collider[] targets = Physics.OverlapSphere(transform.position, noiseDistance, targetLayer);

            GameObject loudestTarget = null;
            float highestNoise = minimumNoiseThreshold;

            foreach (var collider in targets)
            {
                GameObject target = collider.gameObject;
                float noise = 0f;
                PlayerWalk playerScript = target.GetComponent<PlayerWalk>();
                if (playerScript != null)
                    noise = playerScript.CurrentNoise;

                ThrowableItem throwable = target.GetComponent<ThrowableItem>();
                // Debug.Log($"[GetNoisyTarget] PlayerWalk: {playerScript != null} | ThrowableItem: {throwable != null} | CurrentNoise: {throwable?.CurrentNoise}");
                if (throwable != null && throwable.CurrentNoise > 0f)
                    noise = throwable.CurrentNoise;

                if (noise >= highestNoise)
                {
                    highestNoise = noise;
                    loudestTarget = target;
                }
            }

            return loudestTarget;
        }

        /*
        =========================
        PRIORITY TARGET
        =========================
        */

        // public GameObject GetPriorityTarget()
        // {
        //     // 1️⃣ Vision always wins → Chase
        //     List<GameObject> visible = GetVisibleTargets();
        //     if (visible.Count > 0)
        //     {
        //         return visible[0];
        //     }

        //     // 2️⃣ Close noise → Chase
        //     GameObject noisyClose = GetNoisyTarget();
        //     if (noisyClose != null)
        //     {
        //         return noisyClose;
        //     }

        //     // 3️⃣ Distant loud noise → Return for Check state
        //     GameObject noisyDistant = GetDistantNoisyTarget();
        //     if (noisyDistant != null)
        //     {
        //         return noisyDistant; // Will be used for Check, not Chase
        //     }

        //     // 4️⃣ Nothing detected
        //     return null;
        // }

        /*
        =========================
        VISIBILITY CHECK
        =========================
        */

        public bool IsTargetVisible(GameObject target)
        {
            if (target == null) return false;

            Vector3 direction = (target.transform.position - transform.position);
            float distance = direction.magnitude;

            if (distance > visionDistance)
                return false;

            direction.Normalize();
            direction.y = 0;

            float angle = Vector3.Angle(transform.forward, direction);

            if (angle > viewAngle * 0.5f)
                return false;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance, obstacleLayer))
            {
                if (hit.collider.gameObject != target)
                    return false;
            }

            return true;
        }
        /*
=========================
GET DISTANT NOISE TARGET
=========================
Returns: Player GameObject if VERY loud sound detected in distant radius, else null
*/
        public GameObject GetDistantNoisyTarget()
        {
            // Only check if distant settings are valid
            if (distantNoiseDistance <= 0f || distantNoiseThreshold <= 0f)
                return null;

            Collider[] targets = Physics.OverlapSphere(transform.position, distantNoiseDistance, targetLayer);

            GameObject loudestTarget = null;
            float highestNoise = distantNoiseThreshold; // Must exceed this high threshold

            foreach (var collider in targets)
            {
                GameObject target = collider.gameObject;

                // Skip if already in closer detection ranges (avoid duplicate checks)
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= noiseDistance)
                    continue; // Let GetNoisyTarget() handle close-range noise

                PlayerWalk playerScript = target.GetComponent<PlayerWalk>();
                ThrowableItem throwable = target.GetComponent<ThrowableItem>();
                float noise = 0f;
                if (playerScript != null)
                    noise = playerScript.CurrentNoise;
                else if (throwable != null && throwable.CurrentNoise > 0f)
                    noise = throwable.CurrentNoise;

                if (noise == 0f) continue;

                if (noise >= highestNoise)
                {
                    highestNoise = noise;
                    loudestTarget = target;
                }
            }

            return loudestTarget; // null if no distant loud noise detected
        }
        public (GameObject target, DetectionType type) DetectPriorityTarget()
        {
            List<GameObject> visible = GetVisibleTargets();
            if (visible.Count > 0)
                return (visible[0], DetectionType.Vision);

            GameObject noisyClose = GetNoisyTarget();
            if (noisyClose != null)
            {
                if (noisyClose.GetComponent<ThrowableItem>() != null)
                    return (noisyClose, DetectionType.DistantNoise); // → Check
                return (noisyClose, DetectionType.CloseNoise); // → Chase
            }
            GameObject noisyDistant = GetDistantNoisyTarget();
            if (noisyDistant != null)
                return (noisyDistant, DetectionType.DistantNoise);

            return (null, DetectionType.None);
        }
        /*
        =========================
        GIZMOS
        =========================
        */

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            Vector3 pos = transform.position;

            Gizmos.color = new Color(distantNoiseColor.r, distantNoiseColor.g, distantNoiseColor.b, 0.8f);
            Gizmos.DrawSphere(pos, distantNoiseDistance);
            // Noise radius
            Gizmos.color = new Color(noiseColor.r, noiseColor.g, noiseColor.b, 0.5f);
            Gizmos.DrawSphere(pos, noiseDistance);

            // Vision radius
            Gizmos.color = new Color(visionColor.r, visionColor.g, visionColor.b, 0.5f);
            Gizmos.DrawSphere(pos, visionDistance);

            // Vision cone lines
            Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = visionColor;
            Gizmos.DrawRay(pos, left * visionDistance);
            Gizmos.DrawRay(pos, right * visionDistance);

            // Draw raycast lines to visible targets
            List<GameObject> visible = GetVisibleTargets();
            foreach (var target in visible)
            {
                if (target != null)
                {
                    Gizmos.color = visibleTargetColor;
                    Gizmos.DrawLine(pos, target.transform.position);
                }
            }

            // Draw raycast line to priority target
            var (priorityTarget, detectionType) = DetectPriorityTarget();
            if (priorityTarget != null)
            {
                Gizmos.color = priorityTargetColor;
                Gizmos.DrawLine(pos, priorityTarget.transform.position);
            }
        }
    }
}