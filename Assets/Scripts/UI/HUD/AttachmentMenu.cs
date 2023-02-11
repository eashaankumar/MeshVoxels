using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttachmentMenu : MonoBehaviour
{
    [SerializeField]
    Image m_selector;

    AttachmentElement m_attachment;


    public static AttachmentMenu Singleton;

    public AttachmentType SelectedAttachmentType
    {
        get { return m_attachment == null ? AttachmentType.WHEEL : m_attachment.Type; }
    }

    private void Awake()
    {
        Singleton = this;
        m_attachment = GetComponentInChildren<AttachmentElement>();
    }

    private void Update()
    {
        OnSelectAttackment(m_attachment);
    }

    public void OnSelectAttackment(AttachmentElement e)
    {
        if (e == null) return;
        m_attachment = e;
        m_selector.transform.position = m_attachment.transform.position;
    }
}
