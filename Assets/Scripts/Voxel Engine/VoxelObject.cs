using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class VoxelObject : MonoBehaviour
{
    [SerializeField]
    protected float m_voxelSize;
    [SerializeField]
    protected List<Material> m_materials;


    protected MeshFilter m_filter;
    protected MeshRenderer m_renderer;
    protected Rigidbody m_rb;

    protected NativeParallelHashMap<int3, VoxelData> m_voxels;
    protected Dictionary<int3, SignalData> m_signals; // on or off
    protected Dictionary<int3, VoxelUVData> m_voxUvMap;
    protected Mesh m_mesh;

    protected Outline m_outline;

    protected Dictionary<int3, BoxCollider> m_boxColliders;

    protected UnityEvent<VoxelType, int3> m_OnVoxelAdd;
    protected UnityEvent<Attachment> m_onNewAttachment;

    #region Get/Set
    public Material Material
    {
        set
        {
            if (m_materials == null) m_materials = new List<Material>();
            m_materials.Add(value);
            if (m_renderer) m_renderer.SetMaterials(m_materials);
        }
    }
    public Mesh Mesh
    {
        get { return m_mesh; }
    }
    public NativeParallelHashMap<int3, VoxelData> Voxels
    {
        get { return m_voxels; }
    }

    public Dictionary<int3, SignalData> Signals
    {
        get { return m_signals; }
    }

    public Rigidbody Rigidbody
    {
        get { return m_rb; }
    }

    public void RemoveRigidbody()
    {
        if (Rigidbody != null)
        {
            Destroy(Rigidbody);
        }
    }

    public void AddRigidbody()
    {
        if (Rigidbody == null)
        {
            m_rb = gameObject.AddComponent<Rigidbody>();
            CalculateMass();
        }
    }

    public void TurnSignal(int3 pos, bool on=true)
    {
        if (m_signals.ContainsKey(pos))
        {
            SignalData signal = m_signals[pos];
            signal.on = on;
            m_signals[pos] = signal;
            int imgWidth = 2048;
            int spriteWidth = 128;
            int spritesPerAxis = imgWidth / spriteWidth;
            if (m_voxUvMap.ContainsKey(pos))
            {
                VoxelUVData uvData = m_voxUvMap[pos];
                for(int i = uvData.startIndex; i <= uvData.endIndex; i++)
                {
                    Vector2[] itsUVs = Mesh.uv;
                    if (i >= 0 && i < itsUVs.Length)
                    {
                        int spriteId = 0;
                        if (signal.type == VoxelType.SIGNAL) spriteId = signal.on ? 6 : 5;
                        if (signal.type == VoxelType.SIGNAL_PULSE) spriteId = signal.on ? 8 : 7;
                        itsUVs[i] = new Vector2(spriteId, spritesPerAxis - 1) / spritesPerAxis;
                    }
                    Mesh.uv = itsUVs;
                }
            }
        }
    }

    public float VoxelSize
    {
        get { return m_voxelSize; }
        set { m_voxelSize = value; }
    }
    #endregion

    #region Monobehavior
    protected virtual void Awake()
    {
        m_filter = gameObject.AddComponent<MeshFilter>();
        m_renderer = gameObject.AddComponent<MeshRenderer>();
        if (m_materials != null) m_renderer.SetMaterials(m_materials);
        m_voxels = new NativeParallelHashMap<int3, VoxelData>(1, Allocator.Persistent);
        m_signals = new Dictionary<int3, SignalData>();
        m_voxUvMap = new Dictionary<int3, VoxelUVData>();
        if (m_filter != null) m_mesh = m_filter.mesh;
        m_OnVoxelAdd = new UnityEvent<VoxelType, int3>();
        m_boxColliders = new Dictionary<int3, BoxCollider>();
    }

    protected virtual void Start()
    {
        /*m_outline = gameObject.AddComponent<Outline>();
        m_outline.outlineColor = Color.black;
        m_outline.OutlineMode = Outline.Mode.OutlineAll;
        m_outline.OutlineWidth = 10f;*/
        
    }

    protected virtual void OnDestroy()
    {
        if (m_voxels.IsCreated) m_voxels.Dispose();
    }
    #endregion

    #region Actions
    public void AddOnAddVoxelListner(UnityAction<VoxelType, int3> a)
    {
        m_OnVoxelAdd.AddListener(a);
    }

    public void RemoveOnAddVoxelListner(UnityAction<VoxelType, int3> a)
    {
        m_OnVoxelAdd.RemoveListener(a);
    }

    public void AddOnNewAttachmentListner(UnityAction<Attachment> a)
    {
        m_onNewAttachment.AddListener(a);
    }

    public void RemoveOnNewAttachmentListner(UnityAction<Attachment> a)
    {
        m_onNewAttachment.RemoveListener(a);
    }

    public void OnNewAttachment(Attachment a)
    {
        m_onNewAttachment.Invoke(a);
    }

    public void SetColliders(int3[] centers)
    {
        /*BoxCollider[] colliders = GetComponents<BoxCollider>();
        for(int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
        if (m_boxColliders == null) m_boxColliders = new Dictionary<int3, BoxCollider>();
        foreach (int3 center in centers)
        {
            if (!m_boxColliders.ContainsKey(center))
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.center = center * new float3(1, 1, 1) * m_voxelSize + m_voxelSize / 2f;
                bc.size = new float3(1, 1, 1) * m_voxelSize;
            }
        }*/
    }

    void CreateCollider(int3 pos)
    {
        if (m_boxColliders == null) m_boxColliders = new Dictionary<int3, BoxCollider>();
        if (!m_boxColliders.ContainsKey(pos))
        {
            BoxCollider bc = GetCollider(pos, m_voxelSize, gameObject);
            m_boxColliders[pos] = bc;
        }
    }

    BoxCollider GetCollider(int3 pos, float voxelSize, GameObject gO)
    {
        BoxCollider bc = gO.AddComponent<BoxCollider>();
        bc.center = pos * new float3(1, 1, 1) * voxelSize + voxelSize / 2f;
        bc.size = new float3(1, 1, 1) * voxelSize;
        return bc;
    }

    void RemoveCollider(int3 pos)
    {
        if (m_boxColliders != null && m_boxColliders.ContainsKey(pos))
        {
            Destroy(m_boxColliders[pos]);
            m_boxColliders.Remove(pos);
        }
    }

    public void SetUVMap(NativeParallelHashMap<int3, VoxelUVData> d)
    {
        m_voxUvMap.Clear();
        foreach(int3 key in d.GetKeyArray(Allocator.Temp))
        {
            m_voxUvMap.Add(key, d[key]);
        }
    }

    public void SetSignals(NativeParallelHashMap<int3, SignalData> signals)
    {
        m_signals.Clear();
        foreach (int3 key in signals.GetKeyArray(Allocator.Temp))
        {
            m_signals.Add(key, signals[key]);
        }
    }

    public virtual void AddVoxel(Vector3 worldPos, VoxelType t)
    {
        int3 gridPos = GridPos(transform.InverseTransformPoint(worldPos));
        if (m_voxels.ContainsKey(gridPos)) return;
        m_voxels.Add(gridPos, new VoxelData { type = t });
        CreateCollider(gridPos);
        if (t == VoxelType.SIGNAL || t == VoxelType.SIGNAL_PULSE)
        {
            m_signals.Add(gridPos, new SignalData { type=t, on=false});
        }
        if (VoxelMeshGenerator.Instance != null)
        {
            VoxelMeshGenerator.Instance.EnqueueVoxelObject(this);
        }
        CalculateMass();
        if (m_OnVoxelAdd != null) m_OnVoxelAdd.Invoke(t, gridPos);
    }

    public virtual void RemoveVoxel(Vector3 worldPos)
    {
        int3 gridPos = GridPos(transform.InverseTransformPoint(worldPos));
        if (m_voxels.ContainsKey(gridPos))
        {
            VoxelData data = m_voxels[gridPos];
            if (data.type == VoxelType.SIGNAL || data.type == VoxelType.SIGNAL_PULSE)
            {
                m_signals.Remove(gridPos);
            }
        }
        RemoveCollider(gridPos);
        m_voxels.Remove(gridPos);
        if (m_voxels.Count() == 0)
        {
            Destroy(gameObject);
        }
        else if (VoxelMeshGenerator.Instance != null)
        {
            VoxelMeshGenerator.Instance.EnqueueVoxelObject(this);
        }
        if (m_rb) m_rb.mass = m_voxels.Count() * VoxelSize;
    }
    #endregion

    #region Helpers
    void CalculateMass()
    {
        if (Rigidbody)
        {
            Rigidbody.mass = m_voxels.Count() * VoxelSize;
        }
    }
    public int3 GridPos(Vector3 pos)
    {
        return new int3(Mathf.FloorToInt(pos.x / m_voxelSize), Mathf.FloorToInt(pos.y / m_voxelSize), Mathf.FloorToInt(pos.z / m_voxelSize));
    }
    #endregion

}


public enum VoxelType
{
    GRASS, DIRT, ROCK, STEEL, FIXED_JOINT, SIGNAL, SIGNAL_PULSE
}

public struct VoxelData
{
    public VoxelType type;
}

public struct VoxelUVData
{
    public int startIndex; // pos of 1st uv in mesh.uv array
    public int endIndex;
}

public struct SignalData
{
    public bool on;
    public VoxelType type;
}