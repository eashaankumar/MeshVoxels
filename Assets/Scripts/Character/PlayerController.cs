using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    float m_speed;
    [SerializeField]
    float m_jumpSpeed;
    [SerializeField]
    Rigidbody m_rb;

    float forwardInput;
    float rightInput;
    int jumpInput;
    Vector3 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        m_rb.isKinematic = false;
        m_rb.constraints = RigidbodyConstraints.FreezeRotation;
        m_rb.useGravity = true;
    }

    // Update is called once per frame
    void Update()
    {
        forwardInput = Input.GetAxis("Vertical");
        rightInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput++;
        }
    }

    private void FixedUpdate()
    {
        if (m_rb != null)
        {
            Vector3 vel = Vector3.zero;
            if (jumpInput > 0)
            {
                jumpInput--;
                moveDir = transform.up * m_jumpSpeed;
                vel = moveDir;
            }
            else
            {
                moveDir = (transform.forward * forwardInput + transform.right * rightInput).normalized;
                vel = moveDir * m_speed;
                vel.y = m_rb.velocity.y;
            }
            m_rb.velocity = vel;
        }
    }
}
