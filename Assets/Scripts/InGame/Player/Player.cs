using System.Collections.Generic;
using System.Linq;
using InGame.Battle;
using InGame.Chunk;
using UnityEngine;

namespace InGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public partial class Player : MonoBehaviour
    {
        public float moveSpeed = 2.0f;
        public float rotationSpeed = 360f;
        public float gravityValue = -9.81f;
        public float jumpHeight = 2f;

        
        public long currentXP = 0;
        public long currentLevel = 1;

        public long CurrentHp { get; private set; }
        public bool isSpawned { get; private set; }
        public long MaxHp => currentLevel * 17 + 4;
        
        public long MaxXp => (long)(Mathf.Pow(currentLevel, 1.4f) * 14) + 15;

        private CharacterController characterController;
        private CameraController cameraController;
        private Animator animator;
        private Vector3 playerVelocity;
        private AnimatorStateInfo stateInfo;
        private List<IDamageable> damageablesInArea;
        
        private float horizontalInput, verticalInput;
        private float speedMultiplier = 1.0f;
        private bool isBlockMovement;
        private int waterTriggersCount = 0;
        private bool isUnderWater;

        [SerializeField] private List<GameObject> listeningGameObjects;
        [SerializeField] private bool isGravityPresent = false;
        [SerializeField] private Vector3 boxCastSize, boxCastOffset;
        
        private static readonly int HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private partial void ApplyGravity();
        private partial void TeleportToPosition(Vector3 newPosition);
        private partial void Move(float horizontalInput, float verticalInput);
        private partial void Jump();
        private partial void Float();
        private partial void RotatePlayer();
        public partial void SetIsGravityPresent(bool state);
        private partial void OnTriggerEnter(Collider other);
        private partial void OnTriggerExit(Collider other);

        private void Awake()
        {
            isSpawned = false;
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            damageablesInArea = new List<IDamageable>();

            CurrentHp = MaxHp;
            currentXP = 0;
        }

        private void Start()
        {
            // Find the CameraController component in the scene
            if (Camera.main != null) cameraController = Camera.main.GetComponent<CameraController>();
            
            Refresh();
        }

        private void Update()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");

            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            
            var stateToBlockMovement = new[]
            {
                "Attack", "Mine", "Gather", 
                "StartChargeMagic", "UseMagic", "EndChargeMagic"
            };

            isBlockMovement = 
                stateToBlockMovement.Aggregate(false, (current, state) => current | stateInfo.IsName(state));

            //isBlockMovement |= animator.IsInTransition(0);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                speedMultiplier = 1.5f;
            }
            else
            {
                speedMultiplier = 1.0f;
            }
            
            if (CanAttack())
            {
                Attack();
            }
            else if (CanJump())
            {
                Jump();
            }
            
            ApplyGravity();
            
            if(!isBlockMovement)
                Move(horizontalInput, verticalInput);
            
            RotatePlayer();
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

        public Vector2Int GetPlayerChunkPos(int chunkSize)
        {
            var playerPos = transform.position;

            // Calculate player's chunk coordinates
            var playerChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
            var playerChunkZ = Mathf.RoundToInt(playerPos.z / chunkSize);

            return new Vector2Int(playerChunkX, playerChunkZ);
        }
        
        

        public void Refresh()
        {
            foreach (var listeningGameObject in listeningGameObjects)
            {
                var components = listeningGameObject.GetComponents<IPlayerListener>();
                foreach (var component in components)
                {
                    component.OnPlayerRefresh(this);
                }
            }
        }

        private bool IsGrounded()
        {
            return Physics.BoxCast(
                transform.position + boxCastOffset,
                boxCastSize,
                -transform.up,
                transform.rotation,
                1,
                1 << LayerMask.NameToLayer("Ground")
            );
        }

        private bool CanJump()
        {
            return Input.GetKeyDown(KeyCode.Space) &&
                   IsGrounded() &&
                   !animator.IsInTransition(0) &&
                   !stateInfo.IsName("Jump") &&
                   !isBlockMovement &&
                   !isUnderWater &&
                   isGravityPresent;
        }
        
        private bool CanAttack()
        {
            return Input.GetMouseButtonDown(0) &&
                   IsGrounded() &&
                   !animator.IsInTransition(0) &&
                   Mathf.Abs(horizontalInput) < 0.1f &&
                   Mathf.Abs(verticalInput) < 0.1f &&
                   !isBlockMovement;
        }
        
        private partial void OnTriggerEnter(Collider other)
        {
            var tag = other.tag;
            if (!isSpawned) return;
            switch (tag)
            {
                case "Water":
                {
                    ++waterTriggersCount;
                    if (waterTriggersCount > 0)
                    {
                        isUnderWater = true;
                    }
                    break;
                }
                case "Mob":
                {
                    var damageable = other.GetComponent<IDamageable>();
                    if(damageable != null)
                        damageablesInArea.Add(damageable);
                    break;
                }
            }
        }

        private partial void OnTriggerExit(Collider other)
        {
            var tag = other.tag;
            if (!isSpawned) return;
            switch (tag)
            {
                case "Water":
                {
                    --waterTriggersCount;
                    if (waterTriggersCount == 0)
                    {
                        isUnderWater = false;
                    }

                    break;
                }
                case "Mob":
                {
                    var damageable = other.GetComponent<IDamageable>();
                    if(damageable != null)
                        damageablesInArea.Remove(damageable);
                    break;
                }
            }
        }
        
        
    }
}
