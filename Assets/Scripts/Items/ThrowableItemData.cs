using UnityEngine;

namespace RowdyRobots
{
    [CreateAssetMenu(fileName = "ThrowableItemData", menuName = "RowdyRobots/ThrowableItemData")]
    public class ThrowableItemData : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName = "Item";

        [Header("Throw Settings")]
        public float throwDuration = 0.5f;
        public float minThrowRange = 3f;
        public float maxThrowRange = 10f;
        public float throwHeight = 3f;

        [Header("Noise Settings")]
        public float noiseAmount = 5f;
        public float noiseDuration = 1f;

        [Header("Landing Settings")]
        public float landingRadius = 2f;

        [Header("Obstacle Settings")]
        public LayerMask obstacleLayer;

        [Header("On-Land Effect")]
        public ThrowableItemEffect effect;
    }
}