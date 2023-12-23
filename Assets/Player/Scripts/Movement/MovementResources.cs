using TMPro;
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
    //references
    Rigidbody rb;
    CapsuleCollider coll;
    Gravity grav;
    AWSDMovement awsd;

    [Header("Locations")]
    public Transform orientation;
    public Transform topOfPlayer;
    public Transform bottomOfPlayer;
    public Transform centerOfPlayer;

    [Header("Grounding")]
    public float maxSlopeAngleConsideredGround = 50f;
    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    public RaycastHit groundHit;    //what the grounding raycast is hitting, only meaningful when grounded

    [Header("Layer Masks")]
    public LayerMask groundLayer;

    [Header("Other")]
    public TextMeshProUGUI text;

    void Start()
    {
        awsd = GetComponent<AWSDMovement>();
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        grav = GetComponent<Gravity>();
    }

    void Update()
    {
        GroundCheck();
        UI();
    }

    private void FixedUpdate()
    {
        //noise kill
        KillVelocity(0.001f);
    }

    private void UI()
    {
        text.text = ((int)(rb.velocity.magnitude*1000)/1000).ToString();
    }

    private void GroundCheck()
    {
        //stores the resulting RaycastHit in groundHit
        Vector3 castFrom = bottomOfPlayer.position + new Vector3(0,coll.radius,0);
        grounded = Physics.SphereCast(castFrom, 0.98f * coll.radius, Vector3.down, out groundHit, 0.1f, groundLayer);
        grounded = grounded && GroundAngleDeg() < maxSlopeAngleConsideredGround;
    }

    //Gravity
    public Gravity getGravity()
    {
        return grav;
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
    public AWSDMovement GetAWSD()
    {
        return awsd;
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
    public void KillVelocity(float minSpeed)
    {
        if (rb.velocity.magnitude < minSpeed)
            rb.velocity = Vector3.zero;
    }
    public void KillXZvelocity(float minSpeed)
    {
        if (XZvelocity().magnitude < minSpeed)
            rb.velocity = Yvelocity();
    }
    public void KillYvelocity(float minSpeed)
    {
        if (rb.velocity.y < minSpeed)
            rb.velocity = XZvelocity();
    }

    //Cap Velocity
    public void CapVelocity(float maxSpeed)
    {
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }
    public void CapXZVelocity(float maxSpeed)
    {
        Vector3 xz = XZvelocity();
        if (xz.magnitude > maxSpeed)
            rb.velocity = Yvelocity() + xz.normalized * maxSpeed;
    }
    public void CapYVelocity(float maxSpeed)
    {
        if (rb.velocity.y > maxSpeed)
            rb.velocity = XZvelocity() + maxSpeed * Vector3.up;
    }

    //Drag
    public void ApplyGroundDrag(float drag)
    {
        rb.velocity -= drag * XZvelocity();
    }
    public void ApplyXYZGroundDrag(float drag)
    {
        rb.velocity -= drag * rb.velocity;
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
    public Vector3 ProjectOnFlat(Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }
    public Vector3 GroundNormal()
    {
        return groundHit.normal;
    }
    public float GroundAngleDeg()
    {
        return Vector3.Angle(groundHit.normal, Vector3.up);
    }
    public float GroundAngleRad()
    {
        return Mathf.Deg2Rad * Vector3.Angle(groundHit.normal, Vector3.up);
    }
    public GameObject GroundHitObj()
    {
        return groundHit.collider.gameObject;
    }
}
