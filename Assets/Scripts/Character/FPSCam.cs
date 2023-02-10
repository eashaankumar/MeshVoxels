using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCam : MonoBehaviour
{
    [SerializeField]
    float m_xSensitivity;
    [SerializeField]
    float m_ySensitivity;
    [SerializeField]
    Camera m_camera;
    [SerializeField]
    float m_maxPitch;

    Vector2 delta;
    float pitch;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void FixedUpdate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.Rotate(Vector3.up * delta.x * m_xSensitivity * Time.fixedDeltaTime);

        pitch += delta.y * m_ySensitivity * Time.fixedDeltaTime;
        pitch = Mathf.Clamp( pitch, -m_maxPitch, m_maxPitch);
        m_camera.transform.localEulerAngles = new Vector3(-pitch, 0, 0);
    }
}
