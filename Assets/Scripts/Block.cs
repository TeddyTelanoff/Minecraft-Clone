using UnityEngine;

public class Block : MonoBehaviour
{
    public Chunk chunk;
    public Vector2Int chunkPos;
    public Vector3Int positionInChunk;
    public BlockID id = BlockID.AIR;

    public Vector3Int worldPosition
    {
        get
        {
            return positionInChunk + new Vector3Int(chunkPos.x * Chunk.SIZE.x, 0, chunkPos.y * Chunk.SIZE.z);
        }
    }
    
    public static Block FromWorldPos(Vector3Int worldPos)
    {
        Vector2Int chunkPos = Chunk.WorldPosToChunkPos(worldPos);
        Vector3Int positionInChunk = new Vector3Int(worldPos.x % Chunk.SIZE.x, worldPos.y, worldPos.z % Chunk.SIZE.z);

        if (ChunkGenerator.Instance.chunks.ContainsKey(chunkPos) &&
            (positionInChunk.x < Chunk.SIZE.x && positionInChunk.y < Chunk.SIZE.y && positionInChunk.z < Chunk.SIZE.z) &&
            (positionInChunk.x >= 0 && positionInChunk.y >= 0 && positionInChunk.z >= 0))
            return ChunkGenerator.Instance.chunks[chunkPos].blocks[positionInChunk.x, positionInChunk.y, positionInChunk.z];
        return null;
    }

    public bool[] GetSides()
    {
        bool[] sides = new bool[6];

        sides[0] = (FromWorldPos(worldPosition + new Vector3Int( 0,  0,  1)) == null || FromWorldPos(worldPosition + new Vector3Int( 0,  0,  1)).id == BlockID.AIR);
        sides[1] = (FromWorldPos(worldPosition + new Vector3Int( 1,  0,  0)) == null || FromWorldPos(worldPosition + new Vector3Int( 1,  0,  0)).id == BlockID.AIR);
        sides[2] = (FromWorldPos(worldPosition + new Vector3Int( 0,  1,  0)) == null || FromWorldPos(worldPosition + new Vector3Int( 0,  1,  0)).id == BlockID.AIR);

        sides[3] = (FromWorldPos(worldPosition + new Vector3Int( 0,  0, -1)) == null || FromWorldPos(worldPosition + new Vector3Int( 0,  0, -1)).id == BlockID.AIR);
        sides[4] = (FromWorldPos(worldPosition + new Vector3Int(-1,  0,  0)) == null || FromWorldPos(worldPosition + new Vector3Int(-1,  0,  0)).id == BlockID.AIR);
        sides[5] = (FromWorldPos(worldPosition + new Vector3Int( 0, -1,  0)) == null || FromWorldPos(worldPosition + new Vector3Int( 0, -1,  0)).id == BlockID.AIR);

        return sides;
    }
}

public enum BlockID
{
    AIR,
    GRASS_BLOCK,
    DIRT_BLOCK,
}