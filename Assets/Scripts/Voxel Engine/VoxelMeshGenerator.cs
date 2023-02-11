using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelMeshGenerator : MonoBehaviour
{
    public static VoxelMeshGenerator Instance;

    NativeList<float3> vertices;
    NativeList<float3> normals;
    //NativeList<float3> colors;
    NativeList<float2> uvs;
    NativeList<int> indices;
    NativeList<int3> colliders;
    NativeParallelHashMap<int3, VoxelUVData> voxelToUvMap;
    NativeParallelHashMap<int3, SignalData> signals;

    Queue<VoxelObject> m_jobs;
    JobHandle m_currentHandle;
    VoxelObject m_currentVoxObject;

    #region Monobehavior
    private void Awake()
    {
        Instance = this;
        m_jobs = new Queue<VoxelObject>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnDestroy()
    {
        Dispose();
    }

    void Dispose()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (normals.IsCreated) normals.Dispose();
        if (uvs.IsCreated) uvs.Dispose();
        if (indices.IsCreated) indices.Dispose();
        if (colliders.IsCreated) colliders.Dispose();
        if (voxelToUvMap.IsCreated) voxelToUvMap.Dispose();
        if (signals.IsCreated) signals.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentHandle.IsCompleted)
        {
            if (m_currentVoxObject != null)
            {
                m_currentHandle.Complete();
                // clean up
                m_currentVoxObject.Mesh.Clear();
                m_currentVoxObject.Mesh.vertices = float3ToVector3(vertices);
                m_currentVoxObject.Mesh.normals = float3ToVector3(normals);
                //m_currentVoxObject.Mesh.colors = float3ToColor(colors);
                m_currentVoxObject.Mesh.uv = float2ToVector2(uvs);
                m_currentVoxObject.Mesh.triangles = indices.ToArray();

                m_currentVoxObject.SetColliders(colliders.ToArray() );
                m_currentVoxObject.SetUVMap(voxelToUvMap);
                m_currentVoxObject.SetSignals(signals);

                m_currentVoxObject = null;
                Dispose();

            }

            // new job
            if (m_jobs.Count > 0 && m_currentVoxObject == null)
            {
                m_currentVoxObject = m_jobs.Dequeue();
                vertices = new NativeList<float3>(Allocator.Persistent);
                normals = new NativeList<float3>(Allocator.Persistent);
                //colors = new NativeList<float3>(Allocator.Persistent);
                indices = new NativeList<int>(Allocator.Persistent);
                colliders = new NativeList<int3>(Allocator.Persistent);
                uvs = new NativeList<float2>(Allocator.Persistent);
                voxelToUvMap = new NativeParallelHashMap<int3, VoxelUVData>(1, Allocator.Persistent);
                signals = new NativeParallelHashMap<int3, SignalData>(1, Allocator.Persistent);

                foreach (int3 key in m_currentVoxObject.Signals.Keys)
                {
                    signals.Add(key, m_currentVoxObject.Signals[key]);
                }

                VoxelObjectGenJob job = new VoxelObjectGenJob
                {
                    voxels = m_currentVoxObject.Voxels,
                    signals = signals,
                    voxelToUvMap = voxelToUvMap,
                    voxelSize = m_currentVoxObject.VoxelSize,
                    vertices = vertices,
                    normals = normals,
                    uvs = uvs,
                    indices = indices,
                    colliders = colliders,
                };
                m_currentHandle = job.Schedule();
                print("Started Vox Gen Job");
            }
        }
    }
    #endregion

    #region Helpers
    Vector3[] float3ToVector3(NativeList<float3> a)
    {
        Vector3[] b = new Vector3[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            b[i] = a[i];
        }
        return b;
    }
    Vector2[] float2ToVector2(NativeList<float2> a)
    {
        Vector2[] b = new Vector2[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            b[i] = a[i];
        }
        return b;
    }
    Color[] float3ToColor(NativeList<float3> a)
    {
        Color[] b = new Color[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            b[i] = new Color(a[i].x, a[i].y, a[i].z);
        }
        return b;
    }
    #endregion

    #region Actions
    public void EnqueueVoxelObject(VoxelObject o)
    {
        m_jobs.Enqueue(o);
    }

    #endregion

    #region Jobs
    [BurstCompile]
    struct VoxelObjectGenJob : IJob
    {
        [ReadOnly] public float voxelSize;
        [NativeDisableParallelForRestriction] public NativeParallelHashMap<int3, VoxelData> voxels;
        [NativeDisableParallelForRestriction] public NativeParallelHashMap<int3, SignalData> signals;
        [NativeDisableParallelForRestriction] public NativeParallelHashMap<int3, VoxelUVData> voxelToUvMap;
        [NativeDisableParallelForRestriction] public NativeList<float3> vertices;
        [NativeDisableParallelForRestriction] public NativeList<float3> normals;
        [NativeDisableParallelForRestriction] public NativeList<float2> uvs;
        [NativeDisableParallelForRestriction] public NativeList<int> indices;
        [NativeDisableParallelForRestriction] public NativeList<int3> colliders;

        public void Execute()
        {
            NativeArray<int3> localGridPositions = voxels.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < localGridPositions.Length; i++)
            {
                int3 gridPos = localGridPositions[i];
                GenerateCube(gridPos);
            }
            

            localGridPositions.Dispose();
        }

        void GenerateCube(int3 pos)
        {
            float3 A = new float3(pos.x, pos.y, pos.z) * voxelSize;
            float3 B = A + new float3(0, 1, 0) * voxelSize;
            float3 C = B + new float3(1, 0, 0) * voxelSize;
            float3 D = C + new float3(0, -1, 0) * voxelSize;

            float3 E = A + new float3(0, 0, 1) * voxelSize;
            float3 F = B + new float3(0, 0, 1) * voxelSize;
            float3 G = C + new float3(0, 0, 1) * voxelSize;
            float3 H = D + new float3(0, 0, 1) * voxelSize;

            VoxelData voxelData = voxels[pos];
            float2 uv1 = float2.zero, uv2 = float2.zero, uv3 = float2.zero, uv4 = float2.zero;
            int imgWidth = 2048;
            int spriteWidth = 128;
            int spritesPerAxis = imgWidth / spriteWidth;
            int2 coord = int2.zero;

            switch (voxels[pos].type)
            {
                case VoxelType.GRASS:
                    coord = new int2(0, spritesPerAxis - 1);
                    break;
                case VoxelType.DIRT:
                    coord = new int2(1, spritesPerAxis - 1);
                    break;
                case VoxelType.ROCK:
                    coord = new int2(2, spritesPerAxis - 1);
                    break;
                case VoxelType.STEEL:
                    coord = new int2(3, spritesPerAxis - 1);
                    break;
                case VoxelType.FIXED_JOINT:
                    coord = new int2(4, spritesPerAxis - 1);
                    break;
                case VoxelType.SIGNAL:
                    int x = signals[pos].on ? 6 : 5;
                    coord = new int2(x, spritesPerAxis - 1);
                    break;
                case VoxelType.SIGNAL_PULSE:
                    int x2 = signals[pos].on ? 8 : 7;
                    coord = new int2(x2, spritesPerAxis - 1);
                    break;
            }

            uv1 = new float2(coord.x, coord.y) / spritesPerAxis;
            uv2 = new float2(coord.x, coord.y+1) / spritesPerAxis;
            uv3 = new float2(coord.x+1, coord.y+1) / spritesPerAxis;
            uv4 = new float2(coord.x+1, coord.y) / spritesPerAxis;

            VoxelUVData uvData = new VoxelUVData();
            uvData.startIndex = uvs.Length - 1;

            int faces = 0;
            if (!voxels.ContainsKey(pos + new int3(0, 0, -1)))
            {
                faces++;
                Face(A, B, C, D, new float3(0, 0, -1), uv1, uv2, uv3, uv4); // front
            }
            if (!voxels.ContainsKey(pos + new int3(0, 0, 1)))
            {
                faces++;
                Face(H, G, F, E, new float3(0, 0, 1), uv1, uv2, uv3, uv4); // back
            }
            if (!voxels.ContainsKey(pos + new int3(1, 0, 0)))
            {
                faces++;
                Face(D, C, G, H, new float3(1, 0, 0), uv1, uv2, uv3, uv4); // right
            }
            if (!voxels.ContainsKey(pos + new int3(-1, 0, 0)))
            {
                faces++;
                Face(E, F, B, A, new float3(-1, 0, 0), uv1, uv2, uv3, uv4); // left
            }
            if (!voxels.ContainsKey(pos + new int3(0, 1, 0)))
            {
                faces++;
                Face(B, F, G, C, new float3(0, 1, 0), uv1, uv2, uv3, uv4); // top
            }
            if (!voxels.ContainsKey(pos + new int3(0, -1, 0)))
            {
                faces++;
                Face(D, H, E, A, new float3(0, -1, 0), uv1, uv2, uv3, uv4); // down
            }
            uvData.endIndex = uvs.Length - 1;
            voxelToUvMap.Add(pos, uvData);

            if (faces >= 1)
            {
                colliders.Add(pos);
            }
        }

        void Face(float3 A, float3 B, float3 C, float3 D, float3 normal, float2 uv1, float2 uv2, float2 uv3, float2 uv4)
        {
            vertices.Add(A);
            vertices.Add(B);
            vertices.Add(C);
            vertices.Add(D);

            indices.Add(vertices.Length-4);
            indices.Add(vertices.Length -3);
            indices.Add(vertices.Length - 2);

            indices.Add(vertices.Length - 4);
            indices.Add(vertices.Length - 2);
            indices.Add(vertices.Length - 1);

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);
            uvs.Add(uv4);

        }
    }

    #endregion
}
