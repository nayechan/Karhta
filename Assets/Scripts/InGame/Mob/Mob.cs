using System;
using System.Collections.Generic;
using System.Threading;
using AncyUtility.Pathfinding;
using Cysharp.Threading.Tasks;
using InGame.Battle;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Mob
{
    public class Mob : MonoBehaviour, IAttackable, IDamageable
    {
        enum DistanceStatus
        {
            Near,
            Movable,
            Far
        }
        
        private DStarLite pathfinder;
        
        [SerializeField] private Player.Player player;
        [SerializeField] private float minDistance = 1.0f, maxDistance = 32.0f;
        [SerializeField] private float movementCheckRadius = 1.5f;
        [SerializeField] private int speed = 3;
        [SerializeField] private float attackDelay = 0.5f;

        [SerializeField] private long damageAmount = 4;
        [SerializeField] private long xp = 4;

        [SerializeField] private List<Node> path = null;
        
        
        [field: SerializeField]
        public long MaxHp { get; private set; }
       
        [field: SerializeField]
        public long CurrentHp { get; private set; }
        
        private UniTask movementTask;
        private Animator animator;
        private Vector3 lastPlayerPos;
        
        private bool isInitialized = false;
        private bool isMoving = false;
        private bool isAttackStarted = false;
        private bool isPlayerInArea = false;
        
        private CancellationTokenSource cancellationTokenSource;
        private float pathCalculationDelay;
        private float destructionDelay;
        
        private const int maxHeight = 256;

        private void OnEnable()
        {
            CurrentHp = MaxHp;
            
            pathCalculationDelay = 0.0f;
            destructionDelay = 30.0f;

            isInitialized = false;
            isMoving = false;
            isAttackStarted = false;
            
            path = null;
            
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player.Player>();
            animator = GetComponent<Animator>();
            Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            await UniTask.WaitUntil(() => player.isSpawned);
            isInitialized = true;
            cancellationTokenSource = new CancellationTokenSource();
        }

        private void RetrievePath()
        {
            ResetPathfinder();
            try
            {
                path = pathfinder.CalculatePath();
            }
            catch (Exception e)
            {
                path = null;
            }
        }

        private void Update()
        {
            if (isInitialized)
            {
                var distanceStatus = CheckDistance();
                if (distanceStatus == DistanceStatus.Movable)
                {
                    destructionDelay = 30.0f;
                    if (ShouldRetrievePath())
                    {
                        RetrievePath();
                    }
                    
                    if (path != null && path.Count > 0 && !isMoving)
                    {
                        lastPlayerPos = player.transform.position;
                        
                        var targetPosition = path[0].pos;
                        var targetPos = GetPosFromVec2Int(targetPosition);
                        path.RemoveAt(0);

                        FollowPath(targetPos).Forget();
                    }
                    
                    else if (pathCalculationDelay <= 0.0f && path == null)
                    {
                        pathCalculationDelay = 3.0f;
                    }

                }

                else if (distanceStatus == DistanceStatus.Far)
                {
                    if (destructionDelay < 0)
                    {
                        Death();
                    }
                    destructionDelay -= Time.deltaTime;
                    
                    animator.SetFloat("speed", 0.0f);
                }

                else if (distanceStatus == DistanceStatus.Near)
                {
                    destructionDelay = 30.0f;
                    if (!isAttackStarted)
                    {
                        AttackLoop().Forget();
                    }
                }
            }

            if (pathCalculationDelay > 0)
                pathCalculationDelay -= Time.deltaTime;
        }

        private void ResetPathfinder()
        {
            pathfinder = new DStarLite(
                transform.position,
                player.transform.position,
                Mathf.CeilToInt(maxDistance),
                maxHeight
            );
        }

        private bool ShouldRetrievePath()
        {
            if (Vector3.Distance(player.transform.position, lastPlayerPos) > 16.0f)
                return true;

            if (pathCalculationDelay <= 0.0f)
            {
                if (path == null)
                    return true;

                if (path.Count <= 0)
                    return true;
            }

            return false;
        }
        
        
        private float GetHeightFromTerrain(Vector2Int _position)
        {
            var origin = new Vector3(_position.x, maxHeight, _position.y); // Starting from the ground level
            RaycastHit hit;
            
            int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

            // Perform the raycast from the ground up
            if (Physics.Raycast(origin, Vector3.down, out hit, maxHeight, groundLayerMask))
            {
                return hit.point.y;
            }
            
            return 0;
        }

        private Vector3 GetPosFromVec2Int(Vector2Int pos)
        {
            var result = new Vector3(pos.x, GetHeightFromTerrain(pos), pos.y);
            return result;
        }

        private async UniTask FollowPath(Vector3 targetPos)
        {
            isMoving = true;

            const float tolerance = 0.05f;
            while (Mathf.Abs(transform.position.x - targetPos.x) + Mathf.Abs(transform.position.z - targetPos.z) > tolerance)
            {
                var currentPos = transform.position;
                var direction = targetPos - currentPos;

                LookDirection(direction);
                
                var ray = new Ray(transform.position + movementCheckRadius * Vector3.up, direction);

                var offset = transform.localScale.y / 2.0f * Vector3.up * 0.9f;
                
                var layerMask = 1 << LayerMask.NameToLayer("Water");
                layerMask |= 1 << LayerMask.NameToLayer("Obstacle");
                layerMask |= 1 << LayerMask.NameToLayer("Tree");
                
                if (CheckDistance() != DistanceStatus.Movable || 
                    Physics.SphereCast(ray, movementCheckRadius, direction.magnitude, layerMask))
                {
                    isMoving = false;
                    animator.SetFloat("speed", 0.0f);
                    path.Clear();
                    return;
                }
                
                transform.position = 
                    Vector3.MoveTowards(currentPos, targetPos + offset, speed * Time.deltaTime);
                
                animator.SetFloat("speed", 1.0f);
                
                await UniTask.Yield(cancellationTokenSource.Token);
            }

            animator.SetFloat("speed", 0.0f);
            isMoving = false;
        }

        private DistanceStatus CheckDistance()
        {
            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > maxDistance)
            {
                return DistanceStatus.Far;
            }
            
            if (distance > minDistance)
            {
                return DistanceStatus.Movable;
            }

            return DistanceStatus.Near;
        }

        private async UniTaskVoid AttackLoop()
        {
            isAttackStarted = true;
            while (CheckDistance() == DistanceStatus.Near)
            {
                LookDirection(player.transform.position - transform.position);
                animator.SetTrigger("attack");
                await UniTask.WaitForSeconds(attackDelay * 0.3f);
                if (isPlayerInArea)
                {
                    Damage(player);
                }
                await UniTask.WaitForSeconds(attackDelay * 0.7f);
            }
            isAttackStarted = false;
        }

        public void Damage(IDamageable target)
        {
            target.TakeDamage(this, damageAmount);
        }

        public void TakeDamage(IAttackable from, long amount)
        {
            CurrentHp -= amount;
            if (CurrentHp <= 0 && gameObject.activeSelf)
            {
                DelayedDeath(true).Forget();
            }
        }

        public void Death()
        {
            if(gameObject.activeSelf)
                MobPoolManager.Instance.Pool.Release(gameObject);
        }

        public void LookDirection(Vector3 direction)
        {
            var currentRot = transform.rotation.eulerAngles;
            var lookDirection = new Vector3(direction.x, 0, direction.z);

            currentRot.y = Quaternion.LookRotation(lookDirection).eulerAngles.y;

            transform.rotation = Quaternion.Euler(currentRot);
        }


        public async UniTaskVoid DelayedDeath(bool itemDrop)
        {
            animator.SetTrigger("death");
            await UniTask.WaitForSeconds(1.2f);
            if (itemDrop)
            {
                DropItem();
            }
            Death();
        }

        public void DropItem()
        {
            player.GainXP(xp);
            player.Refresh();
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInArea = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInArea = false;
            }
        }
    }
}