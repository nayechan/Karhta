using System.Collections.Generic;
using UnityEngine;

public class SpriteToMesh : MonoBehaviour
{
    public Sprite sprite; // Assign your sprite in the inspector
    public Material material; // Material to apply to the mesh
    public float extrusionThickness = 0.1f; // Thickness of extrusion
    public int spriteIndex = 0; // Index of the sprite in the sprite sheet

    void Start()
    {
        var rawTexture = sprite.texture;
        var pixels = rawTexture.GetPixels(
            (int)sprite.rect.x, (int)sprite.rect.y, 
            (int)sprite.rect.width, (int)sprite.rect.height
        );

        var slicedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.Alpha8, true);
        slicedTexture.alphaIsTransparency = true;
        slicedTexture.SetPixels(pixels);
        slicedTexture.Apply();
        
        // Create a silhouette mesh from the sprite's alpha channel
        Mesh silhouetteMesh = CreateSilhouetteMesh(sprite);

        // Extrude the silhouette mesh to give it thickness
        Mesh extrudedMesh = ExtrudeMesh(silhouetteMesh, extrusionThickness);

        // Create a new GameObject and assign the extruded mesh
        GameObject extrudedObject = new GameObject();
        MeshFilter meshFilter = extrudedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = extrudedMesh;
        
        // Apply the sprite texture to the extruded mesh
        MeshRenderer _renderer = extrudedObject.AddComponent<MeshRenderer>();
        _renderer.material = material;
        _renderer.material.mainTexture = slicedTexture; // Move this line before UV calculation
        
        // Get the width and height of the sliced texture
        int textureWidth = slicedTexture.width;
        int textureHeight = slicedTexture.height;

        // Create UVs for the extruded mesh
        Vector2[] uvs = new Vector2[extrudedMesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            // Calculate the normalized UV coordinates based on the sliced texture dimensions
            uvs[i] = new Vector2(
                extrudedMesh.vertices[i].z / sprite.rect.height + (sprite.textureRect.y / textureHeight),
                extrudedMesh.vertices[i].x / sprite.rect.width + (sprite.textureRect.x / textureWidth)
            );
        }
        extrudedMesh.uv = uvs;
    }

    Mesh CreateSilhouetteMesh(Sprite sprite)
    {
        // Get the sprite's texture
        Texture2D tex = sprite.texture;

        // Get the rectangle representing the sprite's texture coordinates
        Rect rect = sprite.textureRect;

        // Initialize empty lists to hold vertices and triangles
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // Loop through each pixel in the sprite's rectangle
        for (int x = (int)rect.xMin; x < rect.xMax; x++)
        {
            for (int y = (int)rect.yMin; y < rect.yMax; y++)
            {
                // Get the color of the pixel
                Color pixelColor = tex.GetPixel(x, y);

                // If the pixel is not fully transparent
                if (pixelColor.a > 0)
                {
                    // Create vertices at the corresponding positions
                    Vector3 vertexBottomLeft = new Vector3(x - rect.center.x, 0, y - rect.center.y);
                    Vector3 vertexBottomRight = new Vector3(x - rect.center.x + 1, 0, y - rect.center.y);
                    Vector3 vertexTopLeft = new Vector3(x - rect.center.x, 0, y - rect.center.y + 1);
                    Vector3 vertexTopRight = new Vector3(x - rect.center.x + 1, 0, y - rect.center.y + 1);

                    // Add the vertices to the list
                    vertices.Add(vertexBottomLeft);
                    vertices.Add(vertexBottomRight);
                    vertices.Add(vertexTopLeft);
                    vertices.Add(vertexTopRight);

                    // Calculate the index offsets
                    int offset = vertices.Count - 4;

                    // Add triangles for the current quad
                    triangles.Add(offset);
                    triangles.Add(offset + 2);
                    triangles.Add(offset + 1);
                    triangles.Add(offset + 2);
                    triangles.Add(offset + 3);
                    triangles.Add(offset + 1);
                }
            }
        }

        // Create a new mesh and assign vertices and triangles
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh ExtrudeMesh(Mesh inputMesh, float thickness)
    {
        // Create a new list to hold the extruded vertices
        List<Vector3> extrudedVertices = new List<Vector3>(inputMesh.vertices);

        // Loop through each vertex and extrude it along its normal
        for (int i = 0; i < inputMesh.vertices.Length; i++)
        {
            Vector3 vertex = inputMesh.vertices[i];
            Vector3 normal = inputMesh.normals[i];

            // Calculate the extruded vertex position
            Vector3 extrudedVertex = vertex + normal * thickness;

            // Add the extruded vertex to the list
            extrudedVertices.Add(extrudedVertex);
        }

        // Create a new list to hold the triangles of the extruded mesh
        List<int> extrudedTriangles = new List<int>(inputMesh.triangles);

        // Loop through each triangle and create its extruded counterpart
        for (int i = 0; i < inputMesh.triangles.Length; i += 3)
        {
            // Get the indices of the triangle vertices
            int index1 = inputMesh.triangles[i];
            int index2 = inputMesh.triangles[i + 1];
            int index3 = inputMesh.triangles[i + 2];

            // Calculate the indices of the corresponding extruded vertices
            int extrudedIndex1 = index1 + inputMesh.vertices.Length;
            int extrudedIndex2 = index2 + inputMesh.vertices.Length;
            int extrudedIndex3 = index3 + inputMesh.vertices.Length;

            // Add the extruded triangles
            extrudedTriangles.Add(extrudedIndex1);
            extrudedTriangles.Add(extrudedIndex2);
            extrudedTriangles.Add(extrudedIndex3);
            extrudedTriangles.Add(index3);
            extrudedTriangles.Add(index2);
            extrudedTriangles.Add(extrudedIndex2);
            extrudedTriangles.Add(index1);
            extrudedTriangles.Add(extrudedIndex1);
            extrudedTriangles.Add(extrudedIndex2);
            extrudedTriangles.Add(extrudedIndex1);
            extrudedTriangles.Add(extrudedIndex3);
            extrudedTriangles.Add(extrudedIndex2);
        }

        // Create a new mesh and assign vertices and triangles
        Mesh extrudedMesh = new Mesh();
        extrudedMesh.vertices = extrudedVertices.ToArray();
        extrudedMesh.triangles = extrudedTriangles.ToArray();

        // Recalculate normals and bounds
        extrudedMesh.RecalculateNormals();
        extrudedMesh.RecalculateBounds();

        return extrudedMesh;
    }
}