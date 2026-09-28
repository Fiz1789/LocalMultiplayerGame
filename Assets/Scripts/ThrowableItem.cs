using Benjathemaker;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;

namespace RowdyRobots
{
    public class ThrowableItem : MonoBehaviour
    {
        [Header ("Item Data")]

        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private ThrowableItemData itemData;

        [SerializeField] private SimpleGemsAnim m_gemsAnim;

        private bool m_isPickedUp = false;

        // Noise properties for AlienVisionCone to detect
        public float CurrentNoise { get; private set; }
        public Vector3 NoisePosition => transform.position;

        public bool IsPickedUp => m_isPickedUp;
        public string ItemName => itemData != null ? itemData.itemName : "Item";
        public ThrowableItemData ItemData => itemData;

        private void OnTriggerEnter(Collider other)
        {
            if (m_isPickedUp) return;

            PlayerThrow playerThrow = other.GetComponent<PlayerThrow>();
            if (playerThrow != null && !playerThrow.HasItem)
                playerThrow.PickUpItem(this);
        }

        public void PickUp()
        {
            m_isPickedUp = true;
            if (m_gemsAnim != null)
                m_gemsAnim.enabled = false;
            gameObject.SetActive(false);
        }

        public virtual void OnLand(Vector3 position)
        {
            Debug.Log($"[ThrowableItem] {ItemName} landed at {position}!");

            // Hide mesh renderer(s) to make object invisible but still present for noise
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = false;

            // Trigger the ScriptableObject effect (electrify, freeze, knockback, etc.)
            if (itemData != null && itemData.effect != null)
                itemData.effect.Apply(position, this);

            if (itemData != null)
            {
                CurrentNoise = itemData.noiseAmount;
                Invoke(nameof(DestroySelf), itemData.noiseDuration);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // private void ClearNoise()
        // {
        //     CurrentNoise = 0f;
        // }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}