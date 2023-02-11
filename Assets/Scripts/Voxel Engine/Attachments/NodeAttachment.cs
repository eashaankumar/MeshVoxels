using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeAttachment : SignalNode
{
    public override void Configure(VoxelObject obj, Vector3 point)
    {
        transform.SetParent(obj.transform);
    }

    public override void Connect(VoxelObject obj)
    {

    }
}

public abstract class SignalNode: Attachment
{

}
