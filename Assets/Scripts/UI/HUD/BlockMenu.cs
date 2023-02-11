using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockMenu : MonoBehaviour
{
    [SerializeField]
    Image m_selector;
    
    BlockElement m_block;


    public static BlockMenu Singleton;

    public VoxelType SelectedVoxelType
    {
        get { return m_block == null ? VoxelType.GRASS : m_block.Type; }
    }

    private void Awake()
    {
        Singleton = this;
        m_block = GetComponentInChildren<BlockElement>();
    }

    private void Update()
    {
        OnSelectBlock(m_block);
    }

    public void OnSelectBlock(BlockElement e)
    {
        if (e == null) return;
        m_block = e;
        m_selector.transform.position = m_block.transform.position;
    }
}
