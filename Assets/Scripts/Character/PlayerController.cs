using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    CharacterController m_controller;
    [SerializeField]
    float m_speed;

    float forwardInput;
    float rightInput;
    Vector3 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        forwardInput = Input.GetAxis("Vertical");
        rightInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (m_controller != null)
        {
            moveDir = (transform.forward * forwardInput + transform.right * rightInput).normalized;
            m_controller.Move(moveDir * m_speed * Time.fixedDeltaTime);
        }
    }
}
