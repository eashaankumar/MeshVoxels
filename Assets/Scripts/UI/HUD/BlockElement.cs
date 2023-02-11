using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockElement : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    VoxelType m_type;

    public VoxelType Type
    {
        get { return m_type; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (BlockMenu.Singleton != null)
        {
            BlockMenu.Singleton.OnSelectBlock(this);
        }
    }
}
