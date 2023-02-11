using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastVoxelObjectSpawners : MonoBehaviour
{
    [SerializeField]
    float m_maxDist;
    [SerializeField]
    float m_voxelStepSize = 0.1f;
    [SerializeField]
    Camera m_camera;
    [SerializeField, Tooltip("On which surfaces new voxel objects can be created")]
    LayerMask m_layerMask;
    [SerializeField]
    Material[] m_material;

    Queue<ToolAction> m_toolAction;
    float m_voxelSize;

    enum ToolAction
    {
        NONE, BUILD, DESTROY, ROTATE
    }

    public float SelectedVoxelSize
    {
        get { return m_voxelSize; }
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

        int sign = 0;
        if (Input.mouseScrollDelta.y > 0) sign = 1;
        else if (Input.mouseScrollDelta.y < 0) sign = -1;
        m_voxelSize = Mathf.Clamp(m_voxelSize + sign * m_voxelStepSize, 0.1f, 5f);
        m_voxelSize = (float)System.Math.Round(m_voxelSize, 1);

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_toolAction.Count > 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            ToolAction action = m_toolAction.Dequeue();
            RaycastHit hit;
            Vector3 dir = m_camera.transform.forward;
            if (Physics.Raycast(m_camera.transform.position, dir, out hit, m_maxDist, m_layerMask))
            {
                VoxelObject voxelObject = null;
                Attachment attachment = null;
                switch (action)
                {
                    case ToolAction.BUILD:
                        GameObject gameObject = new GameObject("Voxel Object");
                        gameObject.layer = LayerMask.NameToLayer("Voxel");

                        voxelObject = gameObject.AddComponent<VoxelObject>();

                        voxelObject.VoxelSize = m_voxelSize;

                        foreach (Material m in m_material)
                        {
                            voxelObject.Material = m;
                        }
                        if (BlockMenu.Singleton != null)
                        {
                            voxelObject.AddVoxel(voxelObject.gameObject.transform.position, BlockMenu.Singleton.SelectedVoxelType);
                        }
                        attachment = hit.collider.GetComponent<Attachment>();
                        if (attachment != null)
                        {
                            Vector3 localBoxCenter = hit.collider.gameObject.transform.InverseTransformPoint(hit.collider.bounds.center);
                            Vector3 localHitPoint = hit.collider.gameObject.transform.InverseTransformPoint(hit.point);
                            Vector3 forward = InteractionHelper.Cube(localHitPoint - localBoxCenter, attachment.BoundsSize);
                            forward = hit.collider.gameObject.transform.TransformDirection(forward);

                            gameObject.transform.rotation = Quaternion.LookRotation(forward, -hit.normal);
                            gameObject.transform.position = hit.point + hit.normal * m_voxelSize;
                            gameObject.transform.position -= (gameObject.transform.right + gameObject.transform.forward) * m_voxelSize/2f;

                            attachment.Connect(voxelObject);
                        }
                        else
                        {
                            gameObject.transform.position = hit.point + hit.normal * m_voxelSize/2f - m_voxelSize/2f * Vector3.one;
                            gameObject.transform.rotation = Quaternion.identity;
                            voxelObject.AddRigidbody();
                            print("Adding Rigidbody");
                        }
                        break;
                    case ToolAction.DESTROY:
                        voxelObject = hit.collider.gameObject.GetComponent<VoxelObject>();
                        if (voxelObject) Destroy(hit.collider.gameObject);
                        break;
                    case ToolAction.ROTATE:
                        /*attachment = hit.collider.gameObject.GetComponent<Attachment>();
                        if (attachment)
                        {
                            print("Rotate Attachment");
                            attachment.transform.Rotate(Vector3.up, 90, Space.Self);
                        }*/
                        break;
                }
            }
        }
    }
}
