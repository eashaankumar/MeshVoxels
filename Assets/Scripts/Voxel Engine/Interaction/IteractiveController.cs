using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IteractiveController : MonoBehaviour
{
    [SerializeField]
    Camera m_camera;
    [SerializeField]
    LayerMask m_layerMask;
    [SerializeField]
    float m_maxDist;
    [SerializeField]
    MonoBehaviour[] m_objectsToDisable;

    InteractiveMenu m_current;
    public static IteractiveController Instance;

    public bool Interacting
    {
        get { return m_current != null; }
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_current == null && Input.GetKeyDown(KeyCode.E))
        {
            print("Interactive");
            RaycastHit hit;
            Vector3 dir = m_camera.transform.forward;
            if (Physics.Raycast(m_camera.transform.position, dir, out hit, m_maxDist, m_layerMask))
            {
                print(hit.collider.gameObject.name);
                m_current = hit.collider.gameObject.GetComponent<InteractiveMenu>();

                if (m_current != null)
                {
                    m_current.Show();
                    Cursor.lockState = CursorLockMode.None;
                    foreach(MonoBehaviour go in m_objectsToDisable)
                    {
                        go.enabled = false;
                    }
                }
                else
                {
                    Debug.Log("No Interavtive Menu Found");
                }
                    
            }
        }
    }

    public void Hide()
    {
        m_current = null;
        Cursor.lockState = CursorLockMode.Locked;
        foreach (MonoBehaviour go in m_objectsToDisable)
        {
            go.enabled = true;
        }
    }
}
