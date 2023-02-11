using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttachmentElement : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    AttachmentType m_type;

    public AttachmentType Type
    {
        get { return m_type; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (AttachmentMenu.Singleton != null)
        {
            AttachmentMenu.Singleton.OnSelectAttackment(this);
        }
    }
}

public enum AttachmentType
{
    WHEEL, FIXED_JOINT, BATTERY, SIGNAL
}
