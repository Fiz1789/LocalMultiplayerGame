using UnityEngine;
using System.Collections;

namespace Benjathemaker
{
    public class SimpleGemsAnim : MonoBehaviour
    {
        [Header("Rotating Axes")]
        public bool isRotating = false;
        public bool rotateX = false;
        public bool rotateY = false;
        public bool rotateZ = false;
        public float rotationSpeed = 90f; // Degrees per second

        [Header("Floating Axes")]
        public bool floatingX = false;
        public bool floatingY = true;
        public bool floatingZ = false;
        public bool isFloating = false;
        public bool useEasingForFloating = false; // Separate toggle for floating ease
        public float floatHeight = 1f; // Max height displacement
        public float floatSpeed = 1f;
        private Vector3 initialPosition;
        private float floatTimer;

        [Header("Scaling Axes")]

        public bool isScaling = false;
        public bool useEasingForScaling = false; // Separate toggle for scaling ease
        public float scaleLerpSpeed = 1f; // Speed of scaling transition
        private float scaleTimer;

        private Vector3 initialScale;
        public Vector3 startScale;
        public Vector3 endScale;

        void Start()
        {
            initialScale = transform.localScale;
            initialPosition = transform.position;

            // Adjust start and end scale based on initial scale
            startScale = initialScale;
            endScale = initialScale * (endScale.magnitude / startScale.magnitude);
        }

        void Update()
        {
            if (isRotating)
            {
                Vector3 rotationVector = new Vector3(
                    rotateX ? 1 : 0,
                    rotateY ? 1 : 0,
                    rotateZ ? 1 : 0
                );
                transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
            }

            if (isFloating)
            {
                floatTimer += Time.deltaTime * floatSpeed;
                float t = Mathf.PingPong(floatTimer, 1f);
                if (useEasingForFloating) t = EaseInOutQuad(t);

                float x = floatingX ? t * floatHeight : 0f;
                float y = floatingY ? t * floatHeight : 0f;
                float z = floatingZ ? t * floatHeight : 0f;
                transform.position = initialPosition + new Vector3(x, y, z);
            }

            if (isScaling)
            {
                scaleTimer += Time.deltaTime * scaleLerpSpeed;
                float t = Mathf.PingPong(scaleTimer, 1f); // Oscillates between 0 and 1

                if (useEasingForScaling)
                {
                    t = EaseInOutQuad(t);
                }

                transform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
        }

        float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }
    }
}

