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
            
            rb.isKinematic = false;
        }
    }
}