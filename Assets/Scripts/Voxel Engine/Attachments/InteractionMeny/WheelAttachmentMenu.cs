using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WheelAttachmentMenu : InteractiveMenu
{
    [SerializeField]
    WheelAttachment m_attachment;
    [SerializeField]
    TMP_InputField m_restDistance;
    [SerializeField]
    TMP_InputField m_stretchDistance;
    [SerializeField]
    TMP_InputField m_suspension;
    [SerializeField]
    TMP_InputField m_damping;
    [SerializeField]
    TMP_InputField m_tireGripFactor;
    [SerializeField]
    TMP_InputField m_torque;
    [SerializeField]
    TMP_InputField m_moveSpeed;
    [SerializeField]
    Canvas m_canvas;


    private void Awake()
    {
        Hide();
    }

    string Text(TMP_InputField f)
    {
        return f.text == "" ? "0" : f.text;
    }

    public override void OnValueChanged()
    {
        if (!m_canvas.gameObject.activeSelf) return;
        print("OnValueChanged");
        WheelAttachment.WheelAttachmentProperties props = m_attachment.Properties;
        props.suspensionRestDis = float.Parse(Text(m_restDistance));
        props.rayDist = float.Parse(Text(m_stretchDistance));
        props.springStrength = float.Parse(Text(m_suspension));
        props.springDamper = float.Parse(Text(m_damping));
        props.tireGripFactor = float.Parse(Text(m_tireGripFactor));
        props.torque = float.Parse(Text(m_torque));
        props.moveSpeed = float.Parse(Text(m_moveSpeed));
        m_attachment.Properties = props;
    }

    public override void Show()
    {
        print("Show");
        m_restDistance.text = m_attachment.Properties.suspensionRestDis + "";
        m_stretchDistance.text = m_attachment.Properties.rayDist + "";
        m_suspension.text = m_attachment.Properties.springStrength + "";
        m_damping.text = m_attachment.Properties.springDamper + "";
        m_tireGripFactor.text = m_attachment.Properties.tireGripFactor + "";
        m_torque.text = m_attachment.Properties.torque + "";
        m_moveSpeed.text = m_attachment.Properties.moveSpeed + "";

        m_canvas.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        m_canvas.gameObject.SetActive(false);
    }
}
