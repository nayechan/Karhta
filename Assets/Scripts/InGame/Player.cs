using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player : MonoBehaviour
    {
        public float moveSpeed = 50.0f;
        public float rotationSpeed = 360.0f;

        private Rigidbody rb;

        private void Start()
        {
            // Cache the Rigidbody component
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Rotation based on mouse movement
            float mouseX = Input.GetAxis("Mouse X");
            Quaternion rotation = Quaternion.Euler(0, mouseX * rotationSpeed * Time.deltaTime, 0);
            rb.MoveRotation(rb.rotation * rotation);

            // Forward and backward movement based on keyboard input
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 movement = transform.forward * (verticalInput * moveSpeed * Time.deltaTime);
            
            rb.MovePosition(rb.position + movement);
        }
    }
}