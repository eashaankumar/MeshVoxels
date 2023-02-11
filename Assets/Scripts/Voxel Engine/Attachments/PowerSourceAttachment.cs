using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowerSourceAttachment : Attachment
{
    [SerializeField]
    LineRenderer m_lineRenderer;

    UnityAction<Attachment> m_newAttachment;
    VoxelObject m_parent;

    public override void Configure(VoxelObject obj, Vector3 point)
    {
        m_parent = obj;
        transform.SetParent(obj.transform);
        m_newAttachment = OnNewAttachmentListner;
        obj.AddOnNewAttachmentListner(m_newAttachment);
    }

    public override void Connect(VoxelObject obj)
    {
    } 

    public void OnNewAttachmentListner(Attachment a)
    {
        print("Search for new paths!");
    }

    private void OnDestroy()
    {
        m_parent.RemoveOnNewAttachmentListner(m_newAttachment);
    }
}
