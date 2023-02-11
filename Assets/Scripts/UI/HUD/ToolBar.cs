using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolBar : MonoBehaviour
{
    [SerializeField]
    Image m_modifierTool;
    [SerializeField]
    Image m_voxGOSpawnerTool;
    [SerializeField]
    Image m_attachmentTool;
    [SerializeField]
    Image m_selector;
    [SerializeField]
    TMP_Text m_voxelSize;

    ToolType m_toolType;
    RectTransform m_target;

    public ToolType SelectedTool
    {
        set
        {
            m_toolType = value;
            
        }
    }

    public float SelectedVoxelSize
    {
        set
        {
            if (m_voxelSize != null) m_voxelSize.text = value + "";
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_toolType)
        {
            case ToolType.VOX_MODIFIER:
                m_target = m_modifierTool.rectTransform;
                m_voxelSize.enabled = false;
                break;
            case ToolType.VOX_OBJ_SPAWNER:
                m_target = m_voxGOSpawnerTool.rectTransform;
                m_voxelSize.enabled = true;
                break;
            case ToolType.ATTACHMENT:
                m_target = m_attachmentTool.rectTransform;
                break;
        }
        m_selector.rectTransform.position = m_target.position;
    }
}
