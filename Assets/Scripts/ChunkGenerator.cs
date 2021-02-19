using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	#region Singleton
	public static ChunkGenerator Instance;

    private void Awake()
    {
		if (Instance == null)
			Instance = this;
    }
    #endregion

    public Material material;

	[Range(0, 1)]
	public float noiseStep;
	public int renderDistance;

	public Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

	private void Start()
	{
		StartCoroutine(GenerateChunks());
	}

	public IEnumerator GenerateChunks()
    {
		for (int x = 0; x < renderDistance; x++)
			for (int y = 0; y < renderDistance; y++)
            {
				GenerateChunk(new Vector2Int(x, y));
				yield return new WaitForFixedUpdate();
            }

		for (int x = 0; x < renderDistance; x++)
			for (int y = 0; y < renderDistance; y++)
				chunks[new Vector2Int(x, y)].GenerateMesh();
	}

	public void GenerateChunk(Vector2Int chunckPos)
	{
		if (chunks.ContainsKey(chunckPos))
			return;

		GameObject gameObject = new GameObject();
		gameObject.name = $"Chunk {chunckPos}";
		gameObject.layer = LayerMask.NameToLayer("Jumpable");
		gameObject.transform.localPosition = new Vector3(chunckPos.x * Chunk.SIZE.x, 0, chunckPos.y * Chunk.SIZE.z);

		gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshCollider>();
		gameObject.AddComponent<MeshRenderer>().material = material;
		Chunk chunk = gameObject.AddComponent<Chunk>();
		chunk.chunkPosition = chunckPos;

		for (int x = 0; x < Chunk.SIZE.x; x++)
			for (int z = 0; z < Chunk.SIZE.z; z++)
			{
				int topY = Mathf.FloorToInt(Mathf.PerlinNoise((chunckPos.x * Chunk.SIZE.x + x) * noiseStep, (chunckPos.y * Chunk.SIZE.z + z) * noiseStep) * Chunk.SIZE.y);
				chunk.blocks[x, topY, z].id = BlockID.GRASS_BLOCK;

				for (int y = 0; y < topY; y++)
				{
					chunk.blocks[x, y, z].id = BlockID.DIRT_BLOCK;
				}
			}

		chunks.Add(chunckPos, chunk);
	}
}