using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedJointAttachment : Attachment
{
    FixedJoint m_joint;
    VoxelObject m_owner, m_partner;
    public override void Configure(VoxelObject owner, Vector3 point)
    {
        m_owner = owner;
        transform.SetParent(owner.transform);
    }

    public override void Connect(VoxelObject partner)
    {
        partner.AddRigidbody();
        m_partner = partner;
        m_joint = m_owner.gameObject.AddComponent<FixedJoint>();
        m_joint.connectedBody = m_partner.Rigidbody;
    }
}
