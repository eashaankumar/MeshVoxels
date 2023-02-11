using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public abstract class Attachment : MonoBehaviour
{
    [SerializeField]
    protected AttachmentType m_type;
    [SerializeField]
    protected Bounds m_bounds;

    protected Vector3 hitPoint;

    public Vector3 BoundsCenter
    {
        get
        {
            return m_bounds.center + transform.position;
        }
    }

    public Vector3 BoundsSize
    {
        get { return m_bounds.size; }
    }

    public Vector3 HitPoint
    {
        get
        {
            return hitPoint; 
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(BoundsCenter, m_bounds.size);
    }

    public abstract void Configure(VoxelObject obj, Vector3 point);

    public abstract void Connect(VoxelObject obj);
}
