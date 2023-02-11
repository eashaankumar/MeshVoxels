using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AttachmentSelector : MonoBehaviour
{
    [SerializeField]
    float m_maxDist;
    [SerializeField]
    Camera m_camera;
    [SerializeField, Tooltip("On which surfaces new voxel objects can be created")]
    LayerMask m_layerMask;
    [SerializeField]
    AttachmentData[] m_attachments;

    Dictionary<AttachmentType, Attachment> m_attachmentDataMap;

    bool m_build;
    Queue<ToolAction> m_toolAction;

    enum ToolAction
    {
        NONE, BUILD, DESTROY, ROTATE
    }

    [System.Serializable]
    struct AttachmentData
    {
        public AttachmentType type;
        public Attachment prefab;
    }

    private void Awake()
    {
        m_attachmentDataMap = new Dictionary<AttachmentType, Attachment>();
        foreach(AttachmentData data in m_attachments)
        {
            if (!m_attachmentDataMap.ContainsKey(data.type))
            {
                m_attachmentDataMap.Add(data.type, data.prefab);
            }
        }
    }

    private void OnEnable()
    {
        m_toolAction = new Queue<ToolAction>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_toolAction.Enqueue(ToolAction.BUILD);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            m_toolAction.Enqueue(ToolAction.DESTROY);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            m_toolAction.Enqueue(ToolAction.ROTATE);
        }

    }

    void FixedUpdate()
    {
        if (m_toolAction.Count > 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            ToolAction action = m_toolAction.Dequeue();
            RaycastHit hit;
            Vector3 dir = m_camera.transform.forward;
            if (Physics.Raycast(m_camera.transform.position, dir, out hit, m_maxDist, m_layerMask))
            {
                Attachment attachment = null;
                switch (action)
                {
                    case ToolAction.BUILD:
                    VoxelObject voxelObject = hit.collider.gameObject.GetComponent<VoxelObject>();

                    if (voxelObject != null)
                    {
                        Attachment prefab = null;
                        if (m_attachmentDataMap.ContainsKey(AttachmentMenu.Singleton.SelectedAttachmentType))
                        {
                            prefab = m_attachmentDataMap[AttachmentMenu.Singleton.SelectedAttachmentType];
                        }
                        if (prefab != null)
                        {
                            Vector3 localBoxCenter = hit.collider.gameObject.transform.InverseTransformPoint(hit.collider.bounds.center);
                            Vector3 localHitPoint = hit.collider.gameObject.transform.InverseTransformPoint(hit.point);
                            Vector3 forward = InteractionHelper.Cube(localHitPoint - localBoxCenter, voxelObject.VoxelSize * Vector3.one);
                            forward = hit.collider.gameObject.transform.TransformDirection(forward);

                            attachment = Instantiate(prefab);
                            attachment.transform.position = hit.point + hit.normal * attachment.BoundsSize.y/2f;
                            attachment.transform.rotation = Quaternion.LookRotation(forward, -hit.normal);                            
                            attachment.Configure(voxelObject, hit.collider.gameObject.transform.InverseTransformPoint(hit.point));
                            voxelObject.OnNewAttachment(attachment);
                        }
                    }
                        break;
                    case ToolAction.DESTROY:
                        attachment = hit.collider.gameObject.GetComponent<Attachment>();
                        if (attachment != null)
                        {
                            Destroy(hit.collider.gameObject);
                        }
                        break;
                    case ToolAction.ROTATE:
                        attachment = hit.collider.gameObject.GetComponent<Attachment>();
                        if (attachment)
                        {
                            print("Rotate Attachment");
                            attachment.transform.Rotate(Vector3.up, 90, Space.Self);
                        }
                        break;
                }
            }
        }
    }
}
