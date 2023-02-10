using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class VoxelObject : MonoBehaviour
{
    [SerializeField]
    int m_maxVoxels;
    [SerializeField]
    float m_voxelSize;
    [SerializeField]
    MeshFilter m_filter;


    NativeParallelHashMap<int3, VoxelData> m_voxels;
    Mesh m_mesh;


    #region Get/Set
    public Mesh Mesh
    {
        get { return m_mesh; }
    }
    public NativeParallelHashMap<int3, VoxelData> Voxels
    {
        get { return m_voxels; }
    }

    public float VoxelSize
    {
        get { return m_voxelSize; }
    }
    #endregion

    #region Monobehavior
    private void Awake()
    {
        m_voxels = new NativeParallelHashMap<int3, VoxelData>(m_maxVoxels, Allocator.Persistent);
        if (m_filter != null) m_mesh = m_filter.mesh;
    }

    private void Start()
    {
        AddVoxel(transform.position, VoxelType.GRASS);
    }

    private void OnDestroy()
    {
        if (m_voxels.IsCreated) m_voxels.Dispose();
    }
    #endregion

    #region Actions
    public void AddVoxel(Vector3 worldPos, VoxelType t)
    {
        int3 gridPos = GridPos(transform.InverseTransformPoint(worldPos));
        m_voxels.Add(gridPos, new VoxelData { type = t });
        if (VoxelMeshGenerator.Instance != null)
        {
            VoxelMeshGenerator.Instance.EnqueueVoxelObject(this);
        }
    }
    #endregion

    #region Helpers
    public int3 GridPos(Vector3 pos)
    {
        return new int3(Mathf.FloorToInt(pos.x / m_voxelSize), Mathf.FloorToInt(pos.y / m_voxelSize), Mathf.FloorToInt(pos.z / m_voxelSize));
    }
    #endregion

}


public enum VoxelType
{
    GRASS, DIRT
}

public struct VoxelData
{
    public VoxelType type;
}