using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public static readonly Vector3Int SIZE = new Vector3Int(16, 16, 16);
    public const float uvWH = 0.25f;

    public Vector2Int chunkPosition;
    public Block[,,] blocks = new Block[SIZE.x, SIZE.y, SIZE.z];

    public List<Vector3> vertices;
    public List<Vector2> uvs;
    public List<int> triangles;

    public Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        for (int x = 0; x < SIZE.x; x++)
            for (int y = 0; y < SIZE.y; y++)
                for (int z = 0; z < SIZE.z; z++)
                {
                    Vector3Int blockPositionInChunk = new Vector3Int(x, y, z);

                    GameObject gameObject = new GameObject();
                    gameObject.name = $"Block {blockPositionInChunk}";
                    gameObject.transform.parent = transform;
                    gameObject.transform.localPosition = blockPositionInChunk;
                    gameObject.AddComponent<Block>().chunk = this;
                    blocks[x, y, z] = gameObject.GetComponent<Block>();
                    blocks[x, y, z].positionInChunk = blockPositionInChunk;
                    blocks[x, y, z].chunkPos = chunkPosition;
                }
    }

    public static Vector2Int WorldPosToChunkPos(Vector3Int worldPos)
    {
        int chunkX = worldPos.x / SIZE.x - worldPos.x % SIZE.x / SIZE.x;
        int chunkY = worldPos.z / SIZE.z - worldPos.z % SIZE.z / SIZE.z;

        return new Vector2Int(chunkX, chunkY);
    }

    public void GenerateMesh()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();

        for (int i = 0, x = 0; x < SIZE.x; x++)
            for (int y = 0; y < SIZE.y; y++)
                for (int z = 0; z < SIZE.z; z++, i++)
                {
                    Vector3Int blockPositionInChunk = new Vector3Int(x, y, z);
                    bool[] sides = blocks[x, y, z].GetSides();

                    if (blocks[x, y, z].id != BlockID.AIR)
                    {
                        // Front
                        if (sides[0])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk + new Vector3 { z = 1 }, Vector3.up, Vector3.right, false);
                        }

                        // Left
                        if (sides[1])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk, Vector3.up, Vector3.forward, false);
                        }

                        // Top
                        if (sides[2])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk + new Vector3 { y = 1 }, Vector3.forward, Vector3.right, true);
                        }

                        // Back
                        if (sides[3])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk, Vector3.up, Vector3.right, true);
                        }

                        // Right
                        if (sides[4])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk + new Vector3 { x = 1 }, Vector3.up, Vector3.forward, true);
                        }

                        // Bottom
                        if (sides[5])
                        {
                            GenerateSide(blockPositionInChunk, blockPositionInChunk, Vector3.forward, Vector3.right, false);
                        }
                    }
                }

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void GenerateSide(Vector3Int positionInChunk, Vector3 center, Vector3 up, Vector3 right, bool reversed)
    {
        int index = vertices.Count;

        Vector3 corner = center - Vector3.one / 2;
        vertices.Add(corner);
        vertices.Add(corner + up);
        vertices.Add(corner + up + right);
        vertices.Add(corner + right);

        Vector2 uvCorner = new Vector2(uvWH * (int) blocks[positionInChunk.x, positionInChunk.y, positionInChunk.z].id, uvWH * 3);

        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWH));
        uvs.Add(new Vector2(uvCorner.x + uvWH, uvCorner.y + uvWH));
        uvs.Add(new Vector2(uvCorner.x + uvWH, uvCorner.y));

        if (reversed)
        {
            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 0);
        }
        else
        {
            triangles.Add(index + 1);
            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
            triangles.Add(index + 2);
            triangles.Add(index + 0);
    }
}
}