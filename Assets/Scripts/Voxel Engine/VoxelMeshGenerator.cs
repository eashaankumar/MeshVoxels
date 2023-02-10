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
    NativeList<float3> colors;
    NativeList<int> indices;

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
                m_currentVoxObject.Mesh.colors = float3ToColor(colors);
                m_currentVoxObject.Mesh.triangles = indices.ToArray();

                print(vertices.Length);

                print("Finished Vox Gen Job");
                m_currentVoxObject = null;
                if (vertices.IsCreated) vertices.Dispose();
                if (normals.IsCreated) normals.Dispose();
                if (colors.IsCreated) colors.Dispose();
                if (indices.IsCreated) indices.Dispose();

            }

            // new job
            if (m_jobs.Count > 0 && m_currentVoxObject == null)
            {
                m_currentVoxObject = m_jobs.Dequeue();
                vertices = new NativeList<float3>(Allocator.Persistent);
                normals = new NativeList<float3>(Allocator.Persistent);
                colors = new NativeList<float3>(Allocator.Persistent);
                indices = new NativeList<int>(Allocator.Persistent);

                VoxelObjectGenJob job = new VoxelObjectGenJob
                {
                    voxels = m_currentVoxObject.Voxels,
                    voxelSize = m_currentVoxObject.VoxelSize,
                    vertices = vertices,
                    normals = normals,
                    colors = colors,
                    indices = indices,
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
        [NativeDisableParallelForRestriction] public NativeList<float3> vertices;
        [NativeDisableParallelForRestriction] public NativeList<float3> normals;
        [NativeDisableParallelForRestriction] public NativeList<float3> colors;
        [NativeDisableParallelForRestriction] public NativeList<int> indices;

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
            float3 color = float3.zero;
            switch (voxels[pos].type)
            {
                case VoxelType.GRASS:
                    color = new float3(0, 1, 0);
                    break;
            }

            Face(A, B, C, D, new float3(0, 0, -1), color); // front
            Face(H, G, F, E, new float3(0, 0, 1), color); // back


        }

        void Face(float3 A, float3 B, float3 C, float3 D, float3 normal, float3 color)
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

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

        }
    }

    #endregion
}
