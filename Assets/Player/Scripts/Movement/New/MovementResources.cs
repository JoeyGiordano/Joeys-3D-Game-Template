using UnityEngine;

/// <summary>
/// 
/// this class...
///  - stores and updates variables that describe the play (eg grounded)
///  - provides methods that can be called from MovementState children to apply certain movements to the player
/// 
/// </summary>
public class MovementResources : MonoBehaviour
{
    //player stuffs
    public Transform topOfPlayer, bottomOfPlayer, centerOfPlayer;
    AWSDMovement awsd;
    Rigidbody rb;
    CapsuleCollider coll;

    public Vector3 vel;
    public float speed;

    //ground related
    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    public RaycastHit groundHit;    //what the grounding raycast is hitting, only meaningful when grounded

    //gravity
    public Vector3 normalGravity = new Vector3(0, -9.81f, 0);
    public Vector3 gravity;

    //layermasks
    public LayerMask groundLayer;

    void Start()
    {
        awsd = GetComponent<AWSDMovement>();
        rb = GetComponent<Rigidbody>();
        coll = GetComponentInChildren<CapsuleCollider>();
        gravity = normalGravity;
    }

    void Update()
    {
        GroundCheck();
        vel = rb.velocity;
        speed = vel.magnitude;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    void GroundCheck()
    {
        //stores the resulting RaycastHit in groundHit
        Vector3 castFrom = bottomOfPlayer.position + new Vector3(0,coll.radius,0);
        grounded = Physics.SphereCast(castFrom, 0.98f * coll.radius, Vector3.down, out groundHit, 0.1f, groundLayer);
    }

    void ApplyGravity()
    {
        rb.AddForce(gravity, ForceMode.Force);
    }

    //Gravity
    public void SetGravity(Vector3 gravity)
    {
        this.gravity = gravity;
    }
    public void ResetGravity()
    {
        gravity = normalGravity;
    }

    //AWSD Movement
    public void ActivateAWSD()
    {
        awsd.Activate();
    }
    public void DeactivateAWSD()
    {
        awsd.Deactivate();
    }

    //Get velocity components
    public Vector3 XZvelocity()
    {
        return new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }
    public Vector3 Yvelocity()
    {
        return new Vector3(0, rb.velocity.y, 0);
    }

    //Kill Velocity
    public void KillNearZeroVelocity(float minSpeed)
    {
        if (rb.velocity.magnitude < minSpeed)
            rb.velocity = Vector3.zero;
    }
    public void KillNearZeroXZvelocity(float minSpeed)
    {
        if (XZvelocity().magnitude < minSpeed)
            rb.velocity = Yvelocity();
    }
    public void KillNearZeroYvelocity(float minSpeed)
    {
        if (rb.velocity.y < minSpeed)
            rb.velocity = XZvelocity();
    }

    //Drag
    public void ApplyGroundDrag(float drag)
    {
        rb.velocity -= drag * XZvelocity();
    }
    public void ApplyAirDrag(float drag)
    {
        rb.velocity -= drag * rb.velocity.magnitude * rb.velocity;
    }
    public void ApplyPowDrag(float drag, float pow, bool justXZ)
    {
        if (justXZ)
            rb.velocity -= drag * Mathf.Pow(rb.velocity.sqrMagnitude, (pow - 1f) / 2f) * XZvelocity();
        else
            rb.velocity -= drag * Mathf.Pow(rb.velocity.sqrMagnitude, (pow - 1f) / 2f) * rb.velocity;
    }

    //Ground Results
    public Vector3 ProjectOnGround(Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, groundHit.normal);
    }
    public Vector3 GroundNormal()
    {
        return groundHit.normal;
    }
    public float GroundSlopeDeg()
    {
        return Vector3.Angle(groundHit.normal, Vector3.up);
    }
    public float GroundSlopeRad()
    {
        return Mathf.Deg2Rad * Vector3.Angle(groundHit.normal, Vector3.up);
    }
    public GameObject GroundHitObj()
    {
        return groundHit.collider.gameObject;
    }
}
