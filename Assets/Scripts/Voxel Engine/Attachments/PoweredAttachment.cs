using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoweredAttachment : Attachment
{
    [SerializeField]
    protected float m_powerLevel;
    public abstract void AddPower(float power);

}
