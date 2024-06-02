using System;
using InGame.Chunk;
using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class Player : MonoBehaviour
    {
        public float moveSpeed = 50.0f;
        public float rotationSpeed = 360.0f;

        private Rigidbody rb;
        private bool isSpawned;

        private void Awake()
        {
            isSpawned = false;
        }

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
            // Perform collision detection and response
            DetectAndHandleCollisions();
        }

        public Vector2Int GetPlayerChunkPos(int chunkSize)
        {
            var playerPos = transform.position; 
            
            // Calculate player's chunk coordinates
            int playerChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
            int playerChunkZ = Mathf.RoundToInt(playerPos.z / chunkSize);

            return new Vector2Int(playerChunkX, playerChunkZ);
        }

        public void Spawn(
            Vector2Int chunkPosition, Vector2Int blockPosition, float height, ChunkGenerationConfiguration config)
        {
            if (isSpawned) return;
            isSpawned = true;
            
            var xzPosition = chunkPosition * config.chunkSize + blockPosition;
            var yPosition = height * config.height + 1.0f;
            transform.position = new Vector3(xzPosition.x, yPosition, xzPosition.y);
        }

        public void SetIsKinematic(bool _isKinematic)
        {
            rb.isKinematic = _isKinematic;
        }

        private void DetectAndHandleCollisions()
        {
            // Perform collision detection using Raycast or OnCollisionEnter, depending on your needs
            // For simplicity, let's assume you're using a capsule collider for the player
            CapsuleCollider collider = GetComponent<CapsuleCollider>();

            // Perform a downward raycast to check if the player is above water or terrain
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, collider.height / 2 + 0.1f))
            {
                // Check if the hit object is tagged as water or terrain
                if (hit.collider.CompareTag("Water") || hit.collider.CompareTag("Terrain"))
                {
                    // Prevent further downward movement
                    rb.velocity = Vector3.zero;
                    // Optionally, you can push the player upwards or apply other actions
                    // Example: transform.position += hit.normal * 0.1f; // Move up along the collision normal
                }
            }
        }
    }
}