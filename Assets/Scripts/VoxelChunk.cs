using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunk : MonoBehaviour
{
    int[,,] chunkData;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> tri = new List<int>();
    Mesh mesh;

    MeshFilter mf;
    MeshRenderer mr;

    public int width, height, depth;
    public int chunkX, chunkY;

    public VoxelChunk[] adjChunks = new VoxelChunk[8];

    public Material defaultMaterial;

    public bool pendingUpdate = false;
    // Number of adjacent chunks
    public int adjLen = 0; 

    int vertexId = 0;

    // Set up variables and components
    public void SetupChunk()
    {
        // Create new empty chunk data
        chunkData = new int[width, height, depth];

        // Setup mesh filter component or add new one if needed
        mf = GetComponent<MeshFilter>();
        if (!mf)
            mf = gameObject.AddComponent<MeshFilter>();

        mr = GetComponent<MeshRenderer>();
        if (!mr)
            mr = gameObject.AddComponent<MeshRenderer>();

        mr.material = defaultMaterial;

        mesh = new Mesh();

        GenerateVoxels();

    }

    private void ViewChunkCorners()
    {
        Debug.DrawLine(new Vector3(chunkX * width, 0, chunkY * depth), new Vector3(chunkX * width, 50, chunkY * depth), Color.red, 0f, true);
        Debug.DrawLine(new Vector3(chunkX * width + width, 0, chunkY * depth), new Vector3(chunkX * width + width, 50, chunkY * depth), Color.red, 0f, true);
        Debug.DrawLine(new Vector3(chunkX * width, 0, chunkY * depth + depth), new Vector3(chunkX * width, 50, chunkY * depth + depth), Color.red, 0f, true);
        Debug.DrawLine(new Vector3(chunkX * width + width, 0, chunkY * depth + depth), new Vector3(chunkX * width + width, 50, chunkY * depth + depth), Color.red, 0f, true);
    }

    private void Update()
    {
        if (pendingUpdate)
        {
            Debug.Log("Updating " + gameObject.name);
            GenerateMesh();
        }

        ViewChunkCorners();

        int aux = 0;
        for (int i = 0; i < 8; i++)
            if (adjChunks[i])
                aux++;

        if (adjLen != aux)
            adjLen = aux;
    }

    // Generate voxel data
    public void GenerateVoxels()
    {
        for(int i = 0; i < chunkData.GetLength(0); i++)
        {
            for (int k = 0; k < chunkData.GetLength(2); k++)
            {
                for (int j = 0; j < (int)(Mathf.Min(Mathf.PerlinNoise((chunkX * width + i) / 10.0f, (chunkY * depth + k) / 10.0f) * height, height)); j++)
                {
                    chunkData[i, j, k] = 1;
                }
            }
        }
    }
    // Generate mesh based on chunkData
    public void GenerateMesh()
    {
        int voxelId = 0;
        vertexId = 0;

        pendingUpdate = false;

        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uv = new List<Vector2>();
        tri = new List<int>();
        mesh = new Mesh();

        for (int x = 0; x < chunkData.GetLength(0); x++)
        {
            for (int y = 0; y < chunkData.GetLength(1); y++)
            {
                for (int z = 0; z < chunkData.GetLength(2); z++)
                {
                    GenerateVoxelMesh(x, y, z, voxelId);
                    voxelId++;
                }
            }
        }

        UpdateMeshFilter();
    }
    // Add mesh vertices, normals and uvs for single voxel
    private void GenerateVoxelMesh(int x, int y, int z, int id)
    {
        if (chunkData[x, y, z] == 1)
        {
            // TOP
            if(y < height - 1)
            {
                if (chunkData[x, y + 1, z] == 0)
                {
                    vertices.Add(new Vector3(x, 1 + y, z));
                    vertices.Add(new Vector3(x + 1, 1 + y, z));
                    vertices.Add(new Vector3(x, 1 + y, z + 1));
                    vertices.Add(new Vector3(x + 1, 1 + y, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            // BOTTOM
            if(y > 0)
            {
                if(chunkData[x, y - 1, z] == 0)
                {
                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x, y, z + 1));
                    vertices.Add(new Vector3(x + 1, y, z));
                    vertices.Add(new Vector3(x + 1, y, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(-Vector3.up);
                    normals.Add(-Vector3.up);
                    normals.Add(-Vector3.up);
                    normals.Add(-Vector3.up);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            // RIGHT
            if(x < width - 1)
            {
                if (chunkData[x + 1, y, z] == 0)
                {
                    vertices.Add(new Vector3(1 + x, y, z));
                    vertices.Add(new Vector3(1 + x, y, z + 1));
                    vertices.Add(new Vector3(1 + x, y + 1, z));
                    vertices.Add(new Vector3(1 + x, y + 1, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            else if (adjChunks[5] != null)
            {
                if (adjChunks[5].chunkData[0, y, z] == 0)
                {
                    vertices.Add(new Vector3(1 + x, y, z));
                    vertices.Add(new Vector3(1 + x, y, z + 1));
                    vertices.Add(new Vector3(1 + x, y + 1, z));
                    vertices.Add(new Vector3(1 + x, y + 1, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);
                    normals.Add(Vector3.right);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            // FRONT
            if (z < depth - 1)
            {
                if (chunkData[x, y, z + 1] == 0)
                {
                    vertices.Add(new Vector3(x, y, z + 1));
                    vertices.Add(new Vector3(x + 1, y, z + 1));
                    vertices.Add(new Vector3(x, y + 1, z + 1));
                    vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId);

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 2);

                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            else if (adjChunks[3] != null)
            {
                if(adjChunks[3].chunkData[x, y, 0] == 0)
                {
                    vertices.Add(new Vector3(x, y, z + 1));
                    vertices.Add(new Vector3(x + 1, y, z + 1));
                    vertices.Add(new Vector3(x, y + 1, z + 1));
                    vertices.Add(new Vector3(x + 1, y + 1, z + 1));

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId);

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 2);

                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);
                    normals.Add(Vector3.forward);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            // LEFT
            if(x > 0)
            {
                if (chunkData[x - 1, y, z] == 0)
                {
                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x, y + 1, z));
                    vertices.Add(new Vector3(x, y, z + 1));
                    vertices.Add(new Vector3(x, y + 1, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            else if(adjChunks[1] != null)
            {
                if(adjChunks[1].chunkData[width - 1, y, z] == 0)
                {
                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x, y + 1, z));
                    vertices.Add(new Vector3(x, y, z + 1));
                    vertices.Add(new Vector3(x, y + 1, z + 1));

                    tri.Add(vertexId);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 1);

                    tri.Add(vertexId + 2);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 1);

                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);
                    normals.Add(-Vector3.right);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            // BACK
            if (z > 0)
            {
                if (chunkData[x, y, z - 1] == 0)
                {
                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x, y + 1, z));
                    vertices.Add(new Vector3(x + 1, y, z));
                    vertices.Add(new Vector3(x + 1, y + 1, z));

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId);

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 2);

                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
            else if(adjChunks[7] != null)
            {
                if(adjChunks[7].chunkData[x, y, depth - 1] == 0)
                {
                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x, y + 1, z));
                    vertices.Add(new Vector3(x + 1, y, z));
                    vertices.Add(new Vector3(x + 1, y + 1, z));

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 2);
                    tri.Add(vertexId);

                    tri.Add(vertexId + 1);
                    tri.Add(vertexId + 3);
                    tri.Add(vertexId + 2);

                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);
                    normals.Add(-Vector3.forward);

                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(0, 1));
                    uv.Add(new Vector2(1, 1));

                    vertexId += 4;
                }
            }
        }
            
    }

    void UpdateMeshFilter()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tri.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();

        mf.mesh = mesh;

        if (!gameObject.GetComponent<MeshCollider>())
            gameObject.AddComponent<MeshCollider>();
        else
            gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // Returns the position of the voxel urrently being interated with, based on collisionn point and forward facing direction of player and the hit angle
    public Vector3Int GetRemoveAtPosition(Vector3 hitPoint, Vector3 dir, RaycastHit hit)
    {
        Vector3Int target = new Vector3Int(0, 0, 0);

        Vector3 normalised = hitPoint - new Vector3(chunkX * width, 0, chunkY * depth);

        Vector3Int voxelPosition = new Vector3Int(Mathf.FloorToInt(normalised.x), Mathf.FloorToInt(normalised.y), Mathf.FloorToInt(normalised.z));

        Vector3Int offset = new Vector3Int();

        if (hit.normal == Vector3.up)
        {
            offset = new Vector3Int(0, -1, 0);
        }
        else if (hit.normal == Vector3.down)
        {
            offset = new Vector3Int(0, 0, 0);
        }
        else if (hit.normal == Vector3.forward)
        {
            offset = new Vector3Int(0, 0, -1);
        }
        else if (hit.normal == Vector3.back)
        {
            offset = new Vector3Int(0, 0, 0);
        }
        else if (hit.normal == Vector3.right)
        {
            offset = new Vector3Int(-1, 0, 0);
        }
        else if (hit.normal == Vector3.left)
        {
            offset = new Vector3Int(0, 0, 0);
        }

        return voxelPosition + offset;
    }

    public Vector3Int GetAddAtPosition(Vector3 hitPoint, Vector3 dir, RaycastHit hit)
    {
        Vector3Int target = new Vector3Int();

        Vector3 normalised = hitPoint - new Vector3(chunkX * width, 0, chunkY * depth);

        Vector3Int voxelPosition = new Vector3Int(Mathf.FloorToInt(normalised.x), Mathf.FloorToInt(normalised.y), Mathf.FloorToInt(normalised.z));

        Vector3Int offset = new Vector3Int();

        if (hit.normal == Vector3.up)
        {
            offset = new Vector3Int(0, 0, 0);
        }
        else if (hit.normal == Vector3.down)
        {
            offset = new Vector3Int(0, -1, 0);
        }
        else if (hit.normal == Vector3.forward)
        {
            offset = new Vector3Int(0, 0, 0);
        }
        else if (hit.normal == Vector3.back)
        {
            offset = new Vector3Int(0, 0, -1);
        }
        else if (hit.normal == Vector3.right)
        {
            offset = new Vector3Int(0, 0, 0);
        }
        else if (hit.normal == Vector3.left)
        {
            offset = new Vector3Int(-1, 0, 0);
        }

        return voxelPosition + offset;

        return target;
    }
    // TODO: Make both of these STATIC
    public void RemoveVoxel(Vector3Int at)
    {
        if (chunkData[at.x, at.y, at.z] == 1)
        {
            Debug.Log("Removing voxel at " + at);
            pendingUpdate = true;
            chunkData[at.x, at.y, at.z] = 0;

            if (at.x == 0)
            {
                if (adjChunks[1])
                {
                    adjChunks[1].pendingUpdate = true;
                }
            }
            else if (at.x == width - 1 && adjChunks[5])
            {
                if (adjChunks[5])
                {
                    adjChunks[5].pendingUpdate = true;
                }
            }

            if (at.z == 0)
            {
                if (adjChunks[7])
                {
                    adjChunks[7].pendingUpdate = true;
                }
            }
            else if (at.z == depth - 1 && adjChunks[3])
            {
                if (adjChunks[3])
                {
                    adjChunks[3].pendingUpdate = true;
                }
            }
        }
        else
        {
            Debug.Log("No voxel at " + at);
        }
    }

    public void AddVoxel(Vector3Int at)
    {
        Debug.Log(at);

        // Place in separate chunk if neccessary
        VoxelChunk auxChunk = null;

        if(at.x == -1)
        {
            if (at.z == -1)
            { auxChunk = adjChunks[0]; at = new Vector3Int(width - 1, at.y, depth - 1); }
            else if (at.z == depth)
            { auxChunk = adjChunks[2]; at = new Vector3Int(width - 1, at.y, 0); }
            else
            { auxChunk = adjChunks[1]; at = new Vector3Int(width - 1, at.y, at.z); }
        }
        else if(at.x == width)
        {
            if (at.z == -1)
            { auxChunk = adjChunks[6]; at = new Vector3Int(0, at.y, depth - 1); }
            else if (at.z == depth)
            { auxChunk = adjChunks[4]; at = new Vector3Int(0, at.y, 0); }
            else
            { auxChunk = adjChunks[5]; at = new Vector3Int(0, at.y, at.z); }
        }
        else
        {
            if (at.z == -1)
            { auxChunk = adjChunks[7]; at = new Vector3Int(at.x, at.y, depth - 1); }
            else if (at.z == depth)
            { auxChunk = adjChunks[3]; at = new Vector3Int(at.x, at.y, 0); }
        }

        Debug.Log("New position is " + at);

        if (auxChunk == null)
            auxChunk = this;

        if (auxChunk.chunkData[at.x, at.y, at.z] == 0)
        {
            Debug.Log("Adding voxel at " + at);
            auxChunk.pendingUpdate = true;
            auxChunk.chunkData[at.x, at.y, at.z] = 1;

            if (at.x == 0)
            {
                if (auxChunk.adjChunks[1])
                {
                    auxChunk.adjChunks[1].pendingUpdate = true;
                }
            }
            else if (at.x == width - 1 && auxChunk.adjChunks[5])
            {
                if (auxChunk.adjChunks[5])
                {
                    auxChunk.adjChunks[5].pendingUpdate = true;
                }
            }

            if (at.z == 0)
            {
                if (auxChunk.adjChunks[7])
                {
                    auxChunk.adjChunks[7].pendingUpdate = true;
                }
            }
            else if (at.z == depth - 1 && auxChunk.adjChunks[3])
            {
                if (auxChunk.adjChunks[3])
                {
                    auxChunk.adjChunks[3].pendingUpdate = true;
                }
            }
        }
        else
        {
            Debug.Log("Voxel already at " + at);
        }
    }
}
