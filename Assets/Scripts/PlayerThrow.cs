using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace RowdyRobots
{
    public class PlayerThrow : MonoBehaviour
    {
        [Header("Throw Settings")]
        [SerializeField] private float minThrowRange = 3f;
        [SerializeField] private float maxThrowRange = 10f;
        [SerializeField] private float throwHeight = 3f;
        [SerializeField] private float aimSpeed = 5f;
        [SerializeField] private float throwDuration = 0.5f;
        [SerializeField] private float aimDelay = 0.3f;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [Header("Visuals")]
        [SerializeField] private LineRenderer arcLine;
        [SerializeField] private GameObject landingCircle;
        [SerializeField] private int arcResolution = 20;


        [Header("Line Animation")]
        [SerializeField] private float dashScrollSpeed = 1f;
        private Material m_arcMaterial;
        private ThrowableItem m_heldItem;
        private bool m_isAiming = false;
        private float m_currentRange;
        private Vector3 m_throwDirection;
        private PlayerWalk m_playerWalk;
        private float m_pickupTime = -Mathf.Infinity;
        public bool ThrowingEnabled = true;
        public bool HasItem => m_heldItem != null;

        public bool IsAiming => m_isAiming;

        private void Awake()
        {
            m_playerWalk = GetComponent<PlayerWalk>();
            m_currentRange = minThrowRange;

            if (arcLine != null)
            {
                arcLine.enabled = false;
                m_arcMaterial = arcLine.material;
            }
            if (landingCircle != null)
                landingCircle.SetActive(false);
        }
        private void Update()
        {
            if (!m_isAiming || !HasItem) return;

            // Increase range while holding
            m_currentRange = Mathf.MoveTowards(m_currentRange, maxThrowRange, aimSpeed * Time.deltaTime);

            UpdateThrowDirection();
            UpdateArcVisual();
        }

        private void UpdateThrowDirection()
        {
            Vector3 moveDir = m_playerWalk != null
                ? new Vector3(m_playerWalk.LastMoveInput.x, 0f, m_playerWalk.LastMoveInput.y)
                : transform.forward;

            if (moveDir.sqrMagnitude > 0.01f)
                m_throwDirection = moveDir.normalized;
            else
                m_throwDirection = transform.forward;
        }

        private void UpdateArcVisual()
        {
            if (arcLine == null || landingCircle == null) return;

            Vector3 desiredLanding = transform.position + m_throwDirection * m_currentRange;
            Vector3 actualLanding = desiredLanding;

            // Stop arc at obstacles
            if (Physics.Raycast(transform.position, m_throwDirection, out RaycastHit hit, m_currentRange, obstacleLayer))
                actualLanding = hit.point;

            arcLine.enabled = true;
            arcLine.positionCount = arcResolution;

            for (int i = 0; i < arcResolution; i++)
            {
                float t = (float)i / (arcResolution - 1);
                Vector3 point = CalculateArcPoint(transform.position, actualLanding, throwHeight, t);
                arcLine.SetPosition(i, point);
            }

            if (m_arcMaterial != null)
            {
                float offset = Time.time * dashScrollSpeed;
                m_arcMaterial.SetTextureOffset("_MainTex", new Vector2(-offset, 0f));
            }

            landingCircle.SetActive(true);
            landingCircle.transform.position = actualLanding + Vector3.up * 0.01f;
        }
        private Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float height, float t)
        {
            Vector3 linear = Vector3.Lerp(start, end, t);
            float arc = height * Mathf.Sin(t * Mathf.PI);
            return linear + Vector3.up * arc;
        }

        public void CancelAim()
        {
            m_isAiming = false;
            if (arcLine != null) arcLine.enabled = false;
            if (landingCircle != null) landingCircle.SetActive(false);
        }

        public void PickUpItem(ThrowableItem item)
        {
            m_heldItem = item;
            item.PickUp();
            m_pickupTime = Time.time;
            // Debug.Log($"[PlayerThrow] Picked up {item.ItemName}!");
        }

        public void OnThrow(InputValue value)
        {
            if (!ThrowingEnabled) return;
            if (!HasItem) return;

            // Don't allow aiming immediately after pickup
            if (Time.time - m_pickupTime < aimDelay) return;

            if (value.isPressed)
            {
                m_isAiming = true;
                // Debug.Log("[PlayerThrow] Aiming...");
            }
            else
            {
                if (m_isAiming)
                {
                    m_isAiming = false;
                    Throw();
                }
            }
        }

        private void Throw()
        {
            if (!HasItem) return;

            if (arcLine != null) arcLine.enabled = false;
            if (landingCircle != null) landingCircle.SetActive(false);

            Vector3 landingPos = transform.position + m_throwDirection * m_currentRange;

            m_heldItem.gameObject.SetActive(true);

            Benjathemaker.SimpleGemsAnim anim = m_heldItem.GetComponent<Benjathemaker.SimpleGemsAnim>();
            if (anim != null)
                anim.enabled = false;

            m_heldItem.transform.position = transform.position;
            StartCoroutine(ThrowRoutine(m_heldItem, landingPos));

            m_heldItem = null;
            m_currentRange = minThrowRange;

            // Disable throwing now that throw is complete, if inside task zone
            PlayerTaskInteract taskInteract = GetComponent<PlayerTaskInteract>();
            if (taskInteract != null && taskInteract.IsInTaskZone)
                ThrowingEnabled = false;

            // Debug.Log("[PlayerThrow] Thrown!");
        }

        private IEnumerator ThrowRoutine(ThrowableItem item, Vector3 landingPos)
        {
            float elapsed = 0f;
            Vector3 startPos = item.transform.position;
            Vector3 prevPos = startPos;
            bool hitObstacle = false;
            LayerMask obstacleLayer = item.ItemData != null ? item.ItemData.obstacleLayer : 0;

            while (elapsed < throwDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / throwDuration);
                Vector3 nextPos = CalculateArcPoint(startPos, landingPos, throwHeight, t);

                Vector3 dir = nextPos - prevPos;
                float dist = dir.magnitude;
                if (dist > 0.001f && obstacleLayer != 0)
                {
                    if (Physics.Raycast(prevPos, dir.normalized, out RaycastHit hit, dist, obstacleLayer))
                    {
                        item.transform.position = hit.point;
                        item.OnLand(hit.point);
                        hitObstacle = true;
                        break;
                    }
                }

                item.transform.position = nextPos;
                prevPos = nextPos;
                yield return null;
            }

            if (!hitObstacle)
            {
                item.transform.position = landingPos;
                item.OnLand(landingPos);
            }
        }
    }
}