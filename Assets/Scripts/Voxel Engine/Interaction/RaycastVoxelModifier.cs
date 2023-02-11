using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastVoxelModifier : MonoBehaviour
{
    [SerializeField]
    float m_maxDist;
    [SerializeField]
    Camera m_camera;
    [SerializeField]
    LayerMask m_layerMask;

    int castRay = 0;
    bool m_build;

    private void OnEnable()
    {
        castRay = 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            m_build = Input.GetMouseButtonDown(0);
            castRay++;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (castRay > 0 && Cursor.lockState == CursorLockMode.Locked)
        {
            castRay--;
            RaycastHit hit;
            Vector3 dir = m_camera.transform.forward;
            if (Physics.Raycast(m_camera.transform.position, dir, out hit, m_maxDist, m_layerMask))
            {
                
                VoxelObject voxelObject = hit.collider.gameObject.GetComponent<VoxelObject>();
                if (voxelObject != null)
                {
                    if (m_build)
                    {
                        if(BlockMenu.Singleton != null) voxelObject.AddVoxel(hit.point - dir * 1e-3f, BlockMenu.Singleton.SelectedVoxelType);
                    }
                    else
                        voxelObject.RemoveVoxel(hit.point + dir * 1e-3f);
                }
            }
        }
    }
}
