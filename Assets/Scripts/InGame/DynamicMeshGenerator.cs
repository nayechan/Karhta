using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InGame
{
    public class DynamicMeshGenerator : MonoBehaviour
    {
        public float yScale = 20f;
        public float xzScale = 10.0f;
        private const int chunkSize = 16;

        void Start()
        {
            var seed = Random.Range(0, Int32.MaxValue);
            var heightMap = GenerateHeightmap(seed);
        }

        public float[,] GenerateHeightmap(int seed)
        {
            var result = new float[chunkSize,chunkSize];
            Random.InitState(seed);
        
            for (int z = 0; z < chunkSize; ++z)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    float y = Mathf.PerlinNoise((x / xzScale), (z / xzScale)) * yScale;
                    y = Mathf.Clamp(y, 0, yScale);

                    result[x, z] = y;
                }
            }

            return result;
        }

        Mesh DualContouring(float[,] heightmap)
        {
            int width = heightmap.GetLength(0);
            int height = heightmap.GetLength(1);

            // 여기에 Dual Contouring 알고리즘을 구현합니다.
        
        

            Mesh mesh = new Mesh();
            // 메쉬 생성 코드
            return mesh;
        }

        void SetMesh(Mesh mesh)
        {
            GetComponent<MeshFilter>().mesh = mesh;

            // Mesh의 정점을 재조정하여 표면을 부드럽게 만듦
            mesh.RecalculateNormals();
        }
    }
}
