using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Tree
{
    public class FallingTree : MonoBehaviour
    {
        [SerializeField] private Item.Item dropItem;
        [SerializeField] private int minCount, maxCount;
        
        public GameObject breakParticlePrefab;
        public float fallForce = 0.5f;  // Adjust the force applied to the tree for falling
        
        // Start is called before the first frame update
        private void Start()
        {
            BreakTree().Forget();
        }
        
        

        private async UniTaskVoid BreakTree()
        {
            var rb = GetComponent<Rigidbody>();

            // Apply a random force to make the tree fall in a random direction
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0.1f,  // Adding a bit of upward force to simulate a natural fall
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDirection * fallForce, ForceMode.Impulse);
            await UniTask.WaitForSeconds(3.0f);

            /*
            var particle = 
                GameObject.Instantiate(breakParticlePrefab, transform.position, Quaternion.identity);

            var particleShape = particle.GetComponent<ParticleSystem>().shape;
            particleShape.shapeType = ParticleSystemShapeType.Mesh;
            particleShape.mesh = GetComponent<MeshFilter>().mesh;
            */

            var dropCount = Random.Range(minCount, maxCount);
            for (int count = 0; count < dropCount; ++count)
            {
                var randomOffset = new Vector3(Random.value - 0.5f, 0.5f, Random.value - 0.5f);
                dropItem.InstantiateDropItem(transform.position + randomOffset);
            }
            
            Destroy(gameObject);
        }
    }
}
