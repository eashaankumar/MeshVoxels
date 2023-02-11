using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelToolSelector : MonoBehaviour
{
    [SerializeField]
    RaycastVoxelModifier m_voxModifier;
    [SerializeField]
    RaycastVoxelObjectSpawners m_voxObjSpawner;
    [SerializeField]
    AttachmentSelector m_attachmentSelector;

    ToolType m_currentTool;
    ToolBar m_toolbar;

    // Start is called before the first frame update
    void Start()
    {
        m_toolbar = GameObject.FindObjectOfType<ToolBar>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_currentTool = ToolType.VOX_MODIFIER;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_currentTool = ToolType.VOX_OBJ_SPAWNER;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            m_currentTool = ToolType.ATTACHMENT;
        }
        SetTool();
    }

    void SetTool()
    {
        m_voxModifier.enabled = m_currentTool == ToolType.VOX_MODIFIER;
        m_voxObjSpawner.enabled = m_currentTool == ToolType.VOX_OBJ_SPAWNER;
        m_attachmentSelector.enabled = m_currentTool == ToolType.ATTACHMENT;

        switch (m_currentTool)
        {
            case ToolType.VOX_MODIFIER:
                break;
            case ToolType.VOX_OBJ_SPAWNER:
                if (m_toolbar)
                {
                    m_toolbar.SelectedVoxelSize = m_voxObjSpawner.SelectedVoxelSize;
                }
                break;

        }
        if (m_toolbar)
        {
            m_toolbar.SelectedTool = m_currentTool;
        }
    }
}

public enum ToolType
{
    VOX_MODIFIER, VOX_OBJ_SPAWNER, ATTACHMENT
}
