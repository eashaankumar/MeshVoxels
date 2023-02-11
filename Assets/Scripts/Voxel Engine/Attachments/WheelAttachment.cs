using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAttachment : PoweredAttachment
{
    [SerializeField]
    LineRenderer m_lineRenderer;
    [SerializeField]
    WheelAttachmentProperties m_props;
    [SerializeField]
    LayerMask m_layerMask;
    [SerializeField]
    bool m_ignoreSelf;

    Rigidbody m_parent;
    Ray m_ray; 
    float offset;

    [System.Serializable]
    public struct WheelAttachmentProperties
    {
        public float suspensionRestDis;
        public float rayDist;
        public float springStrength;
        public float springDamper;
        public float tireGripFactor;
        public float tireMass;
        public float torque;
        public float moveSpeed;
        public float wheelFriction;
    }

    public WheelAttachmentProperties Properties
    {
        set
        {
            m_props = value;
        }
        get
        {
            return m_props;
        }
    }

    public Vector3 Top
    {
        get
        {
            return BoundsCenter + transform.up * BoundsSize.y / 2f;
        }
    }

    #region Attachment Overrides
    public override void Configure(VoxelObject obj, Vector3 point)
    {
        hitPoint = point;
        m_parent = obj.Rigidbody;
        transform.SetParent(obj.transform);
        m_lineRenderer.enabled = true;
    }

    public override void Connect(VoxelObject obj)
    {

    }

    public override void AddPower(float power)
    {
        m_powerLevel += power;
    }

    #endregion

    #region Monobehavior
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(m_ray.origin, m_ray.origin + m_ray.direction * m_props.rayDist);
    }

    private void Update()
    {
        m_lineRenderer?.SetPosition(0, Top);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        m_ray = new Ray(Top, -transform.up);
        if (Physics.Raycast(m_ray, out hit, m_props.rayDist, m_layerMask))
        {
            if (hit.collider.gameObject == gameObject && m_ignoreSelf)
            {

            }
            else
            {
                if (m_parent)
                {
                    Vector3 tireWorldVel = m_parent.GetPointVelocity(transform.position);
                    SuspensionForce(hit, tireWorldVel);
                    SteeringForce(hit, tireWorldVel);
                    Acceleration(hit, tireWorldVel);
                }
            } 
        }
        else
        {
            offset = Mathf.Lerp(offset, 0, 0.1f);
        }
        m_lineRenderer?.SetPosition(1, Top - transform.up * (m_props.suspensionRestDis - offset) );
    }
    #endregion

    #region Wheel calculations
    void UpdateGraphics(RaycastHit hit)
    {
        /*if (_graphics != null)
        {
            float distance = Mathf.Min(maxDist, hit.distance);
            Vector3 point = transform.position - transform.up * distance;
            _graphics.transform.position = point + transform.up * tireRadius;
        }*/
    }

    void Acceleration(RaycastHit hit, Vector3 tireWorldVel)
    {
        Vector3 accelDir = transform.forward;
        //if (_vehicleController.Throttle != 0f)
        {
            //m_parent.AddForceAtPosition(accelDir * _torque, transform.position);

            // friction
            float frictionVel = Vector3.Dot(accelDir, tireWorldVel);
            float desiredVelChange = -frictionVel * m_props.wheelFriction;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            m_parent.AddForceAtPosition(accelDir * desiredAccel, transform.position);
            m_parent.velocity = Vector3.ClampMagnitude(m_parent.velocity, m_props.moveSpeed);
        }
    }

    void SteeringForce(RaycastHit hit, Vector3 tireWorldVel)
    {
        Vector3 steeringDir = transform.right;
        float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
        float desiredVelChange = -steeringVel * m_props.tireGripFactor;
        float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
        m_parent.AddForceAtPosition(steeringDir * m_props.tireMass * desiredAccel, transform.position);
    }

    void SuspensionForce(RaycastHit hit, Vector3 tireWorldVel)
    {
        Vector3 springDir = transform.up;
        offset = m_props.suspensionRestDis - hit.distance;
        float vel = Vector3.Dot(springDir, tireWorldVel);
        float force = (offset * m_props.springStrength) - (vel * m_props.springDamper);
        m_parent.AddForceAtPosition(springDir * force, transform.position);
    }
    #endregion
}
