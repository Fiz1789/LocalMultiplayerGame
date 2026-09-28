using UnityEngine;

namespace RowdyRobots
{
    public class SpotlightRotator : MonoBehaviour
    {
        [SerializeField] private float rotationX = 0f;
        [SerializeField] private float rotationY = 45f;
        [SerializeField] private float rotationZ = 0f;
        [SerializeField] private float rotationSpeed = 1f;

        void Update()
        {
            transform.Rotate(rotationX * rotationSpeed * Time.deltaTime, rotationY * rotationSpeed * Time.deltaTime, rotationZ * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
