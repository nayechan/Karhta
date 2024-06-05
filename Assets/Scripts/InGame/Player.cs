using System;
using System.Linq;
using InGame.Chunk;
using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        public float moveSpeed = 2.0f;
        public float rotationSpeed = 360f;
        public float gravityValue = -9.81f;
        public float jumpHeight = 2f;

        private CharacterController characterController;
        private CameraController cameraController;
        private Animator animator;
        private Vector3 playerVelocity;
        private float speedMultiplier = 1.0f;
        
        [SerializeField] private bool isSpawned = false;
        [SerializeField] private bool isGravityPresent = false;
        
        private static readonly int HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private void Awake()
        {
            isSpawned = false;
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // Find the CameraController component in the scene
            if (Camera.main != null) cameraController = Camera.main.GetComponent<CameraController>();
        }

        private void Update()
        {
            var horizontalInput = Input.GetAxis("Horizontal");
            var verticalInput = Input.GetAxis("Vertical");
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            
            var stateToBlockMovement = new[]
            {
                "Attack", "Mine", "Gather", 
                "StartChargeMagic", "UseMagic", "EndChargeMagic"
            };

            var isBlockMovement = 
                stateToBlockMovement.Aggregate(false, (current, state) => current | stateInfo.IsName(state));

            if (Input.GetKey(KeyCode.LeftControl))
            {
                speedMultiplier = 2.0f;
            }
            else
            {
                speedMultiplier = 1.0f;
            }
            
            if(!isBlockMovement)
                Move(horizontalInput, verticalInput);
            
            if (Input.GetKeyDown(KeyCode.Space) && 
                characterController.isGrounded &&
                !stateInfo.IsName("Jump") &&
                !isBlockMovement &&
                isGravityPresent)
            {
                Jump();
            }

            if (Input.GetMouseButtonDown(0) &&
                Mathf.Abs(horizontalInput) < 0.1f &&
                Mathf.Abs(verticalInput) < 0.1f &&
                !isBlockMovement)
            {
                Attack();
            }
            
            RotatePlayer();
        }

        private void FixedUpdate()
        {
            // Grounded check and gravity application should be done in FixedUpdate
            if ((characterController.isGrounded && playerVelocity.y < 0) || 
                !isGravityPresent)
            {
                playerVelocity.y = 0f;
            }

            // Apply gravity
            if(isGravityPresent)
                playerVelocity.y += gravityValue * Time.deltaTime;
        }

        public void Spawn(
            Vector2Int chunkPosition, Vector2Int blockPosition, float height, ChunkGenerationConfiguration config)
        {
            if (isSpawned) return;
            isSpawned = true;

            var xzPosition = chunkPosition * config.chunkSize + blockPosition;
            var yPosition = height * config.height + 1.0f;

            TeleportToPosition(new Vector3(xzPosition.x, yPosition, xzPosition.y));
        }
        
        private void TeleportToPosition(Vector3 newPosition)
        {
            characterController.enabled = false;
            transform.position = newPosition;
            characterController.enabled = true;
        }

        private void Move(float horizontalInput, float verticalInput)
        {
            var horizontalMovement = horizontalInput * moveSpeed;
            var verticalMovement = verticalInput * moveSpeed;

            if (horizontalMovement > 0)
                horizontalMovement *= speedMultiplier;

            if (verticalMovement > 0)
                verticalMovement *= speedMultiplier;

            Vector3 movement = horizontalMovement * transform.right;
            movement += verticalMovement * transform.forward;

            // Move the character controller
            characterController.Move(movement * Time.deltaTime + playerVelocity * Time.deltaTime);
            animator.SetFloat(HorizontalSpeed, horizontalInput);
            animator.SetFloat(VerticalSpeed, verticalInput / 3.0f * speedMultiplier);
        }

        private void Jump()
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.SetTrigger(JumpTrigger);
        }

        private void Attack()
        {
            animator.SetTrigger(AttackTrigger);
        }
        
        private void RotatePlayer()
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;

            transform.Rotate(Vector3.up * mouseX, Space.Self);
            cameraController.RotateCamera(rotationSpeed);
        }

        public Vector2Int GetPlayerChunkPos(int chunkSize)
        {
            var playerPos = transform.position;

            // Calculate player's chunk coordinates
            var playerChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
            var playerChunkZ = Mathf.RoundToInt(playerPos.z / chunkSize);

            return new Vector2Int(playerChunkX, playerChunkZ);
        }

        public void SetIsGravityPresent(bool state)
        {
            isGravityPresent = state;
        }
    }
}
