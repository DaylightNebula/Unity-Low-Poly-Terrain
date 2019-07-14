using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPTerrainMeshGenerator : MonoBehaviour
{
    float[,] noisemap;
    Mesh mesh;

    public int width;
    public int height;
    public float scale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public float heightscale;
    public float tilewidth;
    public float heightpower;

    public TerrainRegion[] regions;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
        UpdateMesh();
        gameObject.AddComponent<MeshCollider>();
    }

    void Update()
    {
        CreateMesh();
        UpdateMesh();
    }

    Vector3[] verticies;
    int[] triangles;
    Color[] colors;
    public void CreateMesh()
    {
        noisemap = Noise.GenerateNoiseMap(width + 1, height + 1, scale, seed, octaves, persistence, lacunarity, offset);

        verticies = new Vector3[(width + 1) * (height + 1) * 6];
        triangles = new int[width * height * 6];
        colors = new Color[verticies.Length];

        for (int i = 0, z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Set verticies to two triangles to form a quad
                verticies[i] = new Vector3(x * tilewidth, Mathf.Pow(noisemap[x, z] * heightscale, heightpower), z * tilewidth);
                verticies[i + 1] = new Vector3(x * tilewidth, Mathf.Pow(noisemap[x, z + 1] * heightscale, heightpower), z + 1 * tilewidth);
                verticies[i + 2] = new Vector3(x + 1 * tilewidth, Mathf.Pow(noisemap[x + 1, z] * heightscale, heightpower), z * tilewidth);
                verticies[i + 3] = new Vector3(x * tilewidth, Mathf.Pow(noisemap[x, z + 1] * heightscale, heightpower), z + 1 * tilewidth);
                verticies[i + 4] = new Vector3(x + 1 * tilewidth, Mathf.Pow(noisemap[x + 1, z + 1] * heightscale, heightpower), z + 1 * tilewidth);
                verticies[i + 5] = new Vector3(x + 1 * tilewidth, Mathf.Pow(noisemap[x + 1, z] * heightscale, heightpower), z * tilewidth);

                // Create those two triangles previously mentioned
                triangles[i] = i;
                triangles[i + 1] = i + 1;
                triangles[i + 2] = i + 2;
                triangles[i + 3] = i + 3;
                triangles[i + 4] = i + 4;
                triangles[i + 5] = i + 5;

                // Set colors for each vertex
                colors[i + 0] = getColor(noisemap[x, z]);
                colors[i + 1] = getColor(noisemap[x, z + 1]);
                colors[i + 2] = getColor(noisemap[x + 1, z]);
                colors[i + 3] = getColor(noisemap[x, z + 1]);
                colors[i + 4] = getColor(noisemap[x + 1, z + 1]);
                colors[i + 5] = getColor(noisemap[x + 1, z]);

                i += 6;
            }
        }
    }

    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    public Color getColor(float height)
    {
        for (int j = 0; j < regions.Length; j++)
            if (regions[j].height > height)
                return regions[j].color;
        return Color.black;
    }

    public void OnDrawGizmos()
    {
        if (verticies == null)
            return;

        for (int i = 0; i < verticies.Length; i++)
        {
            Gizmos.DrawSphere(verticies[i], .1f);
        }
    }
}

[System.Serializable]
public struct TerrainRegion
{
    public float height;
    public Color color;
}
