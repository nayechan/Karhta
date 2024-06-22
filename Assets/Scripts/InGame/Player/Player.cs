using System.Collections.Generic;
using System.Linq;
using InGame.Battle;
using InGame.Chunk;
using InGame.Item;
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

        
        public long currentLevel = 1;

        public long CurrentXP { get; private set; }
        public long CurrentHp { get; private set; }
        public bool isSpawned { get; private set; }
        public long MaxHp => currentLevel * 17 + 4 + Mathf.RoundToInt(allStatMultiplier * PlayerStatData.StatHp);
        
        public long MaxXp => (long)(Mathf.Pow(currentLevel, 1.4f) * 14) + 15;

        private CharacterController characterController;
        private CameraController cameraController;
        private Animator animator;
        private Vector3 playerVelocity;
        private AnimatorStateInfo stateInfo;
        private List<IDamageable> damageablesInArea;
        private List<IPlayerListener> playerListeners;
        private Weapon currentWeapon;

        public ItemData PlayerItemData { get; private set; }
        public QuickSlotData PlayerQuickSlotData { get; private set; }
        public StatData PlayerStatData { get; private set; }
        
        private float horizontalInput, verticalInput;
        private float speedMultiplier = 1.0f;
        private bool isBlockMovement;
        private int waterTriggersCount = 0;
        private bool isUnderWater;
        public float xpMultiplier { get; private set; }
        public float allStatMultiplier { get; private set; }

        [SerializeField] private bool isGravityPresent = false;
        [SerializeField] private Vector3 boxCastSize, boxCastOffset;
        [SerializeField] private List<Transform> playerListenerScanTransforms;
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private QuickSlotInputSetting inputSetting;
        [SerializeField] private AttackAnimationController attackAnimationController;
        [SerializeField] private AnimationClip defaultAttackAnimation;
        [SerializeField] private GameOverController gameOverController;
        
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

            playerListeners = new List<IPlayerListener>();

            foreach (var playerListenerScanTransform in playerListenerScanTransforms)
            {
                playerListeners.AddRange(
                    playerListenerScanTransform.GetComponentsInChildren<IPlayerListener>(true)
                        .ToList());
            }

            damageablesInArea = new List<IDamageable>();

            PlayerItemData = new ItemData();

            PlayerQuickSlotData = new QuickSlotData();
            PlayerStatData = new StatData();

            CurrentHp = MaxHp;
            CurrentXP = 0;
            currentWeapon = null;
            
            attackAnimationController.ChangeAttackAnimation(defaultAttackAnimation);
        }

        private void Start()
        {
            // Find the CameraController component in the scene
            if (Camera.main != null) cameraController = Camera.main.GetComponent<CameraController>();

            GameStateController.Instance.OnGameLoadFinish += OnInit;
        }

        private void OnInit()
        {
            xpMultiplier = PlayerPrefs.GetFloat("currentXpMultiplier", 1);
            allStatMultiplier = PlayerPrefs.GetFloat("currentAllStatMultiplier", 1);
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

            QuickSlotAction();
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
            foreach (var playerListener in playerListeners)
            {
                playerListener.OnPlayerRefresh(this);
            }
        }

        public void AddPlayerListener(IPlayerListener listener)
        {
            playerListeners.Add(listener);
        }

        public void RemovePlayerListener(IPlayerListener listener)
        {
            playerListeners.Remove(listener);
        }

        public string GetKeyBind(int index)
        {
            if (inputSetting.KeyBind.Length <= index) return "";
            var keyBind = inputSetting.KeyBind[index];

            return keyBind.ToString();
        }

        private bool IsGrounded()
        {
            LayerMask layermask = 0;
            layermask |= 1 << LayerMask.NameToLayer("Ground");
            layermask |= 1 << LayerMask.NameToLayer("Tree");
            
            return Physics.BoxCast(
                transform.position + boxCastOffset,
                boxCastSize,
                -transform.up,
                transform.rotation,
                1,
                layermask
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
                case "Tree": 
                case "Mineral":
                {
                    var damageable = other.GetComponent<IDamageable>();
                    if(damageable != null)
                        damageablesInArea.Add(damageable);
                    break;
                }
                case "Dropitem":
                {
                    var dropItem = other.GetComponent<Dropitem>();
                    if (dropItem != null)
                    {
                        PlayerItemData.AddCount(dropItem.itemId, 1);
                        Destroy(other.gameObject);
                        Refresh();
                    }
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
                case "Tree": 
                case "Mineral":
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
