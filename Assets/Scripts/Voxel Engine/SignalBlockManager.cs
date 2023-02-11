using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class SignalBlockManager : MonoBehaviour
{
    [SerializeField]
    float m_waveFrontStep;
    [SerializeField, Tooltip("Number of signal blocks updated per wave")]
    int m_waveFrontWaveSize = 1;

    public static SignalBlockManager Instance;

    Queue<Wave> m_wavefront;
    int waveFrontSize;

    struct Wave
    {
        public List<int3> voxels;
        public List<VoxelObject> voxelObjects;
    }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateWaveFront();
    }

    void Update()
    {
        
    }

    void GenerateWaveFront()
    {
        if (m_wavefront != null && m_wavefront.Count > 0)
        {
            Debug.LogError("Wave front must have finished before starting a new one");
            return;
        }
        if (m_wavefront == null) m_wavefront = new Queue<Wave>();
        VoxelObject[] voxelObjects = FindObjectsOfType<VoxelObject>();
        foreach(VoxelObject voxelObject in voxelObjects)
        {
            List<int3> voxels = new List<int3> ();
            List<VoxelObject> vObjs = new List<VoxelObject> ();
            foreach (int3 localVoxPos in voxelObject.Signals.Keys)
            {
                if (voxels.Count < m_waveFrontWaveSize)
                {
                    voxels.Add (localVoxPos);
                    vObjs.Add (voxelObject);
                }
                else
                {
                    m_wavefront.Enqueue(new Wave { voxels = voxels, voxelObjects = vObjs });
                    voxels.Clear();
                    vObjs.Clear();
                }
            }
            if (voxels.Count > 0)
            {
                m_wavefront.Enqueue(new Wave { voxels = voxels, voxelObjects = vObjs });
            }
        }
        waveFrontSize = m_wavefront.Count;
        DoWave();
    }

    void DoWave()
    {
        if (m_wavefront == null || m_wavefront.Count == 0)
        {
            Invoke("GenerateWaveFront", m_waveFrontStep);
            return;
        }
        Wave wave = m_wavefront.Dequeue();
        // Process wave
        Dictionary<int, VoxelObject> idToObj = new Dictionary<int, VoxelObject>();
        for(int i = 0; i < wave.voxels.Count; i++)
        {
            VoxelObject vGO = wave.voxelObjects[i];
            int3 localVoxelPos = wave.voxels[i];
            if (!vGO.Signals.ContainsKey(localVoxelPos))
            {
                continue;
            }
            SignalData signalData = vGO.Signals[localVoxelPos];
            switch(signalData.type)
            {
                case VoxelType.SIGNAL:
                    if (CheckIfNeighborPowered(in vGO, localVoxelPos))
                    {
                        vGO.TurnSignal(localVoxelPos, on: !vGO.Signals[localVoxelPos].on);
                    }
                    break;
                case VoxelType.SIGNAL_PULSE:
                    if (vGO.Signals[localVoxelPos].on)
                    {
                        // On
                        SendSignalToNeighbors(in vGO, localVoxelPos);
                        vGO.TurnSignal(localVoxelPos, on: false);
                    }
                    else
                    {
                        // Off
                        vGO.TurnSignal(localVoxelPos, on: true);
                    }
                    break;
            }
        }

        // Schedule new wave
        waveFrontSize = m_wavefront.Count;
        Invoke("DoWave", m_waveFrontStep);
    }

    bool CheckIfNeighborPowered(in VoxelObject obj, int3 localGridPos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == y && y == z && z == 0) continue;
                    int3 offset = new int3(x, y, z);
                    int3 neighbor = localGridPos + offset;
                    if (obj.Signals.ContainsKey(neighbor) && obj.Signals[neighbor].on)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void SendSignalToNeighbors(in VoxelObject obj, int3 localGridPos)
    {
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == y && y == z && z == 0) continue;
                    int3 offset = new int3(x, y, z);
                    int3 neighbor = localGridPos + offset;
                    obj.TurnSignal(neighbor, on:true);
                }
            }
        }
    }
}
