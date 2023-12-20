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

    //grounding
    [HideInInspector]
    public bool grounded = false;

    //gravity
    public Vector3 normalGravity = new Vector3(0, -9.81f, 0);
    [SerializeField] private Vector3 gravity;

    //layermasks
    public LayerMask groundLayer;

    void Start()
    {
        awsd = GetComponent<AWSDMovement>();
        rb = GetComponent<Rigidbody>();
        gravity = normalGravity;
    }

    void Update()
    {
        GroundCheck();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(bottomOfPlayer.position, Vector3.down, 0.2f, groundLayer);
    }

    void ApplyGravity()
    {
        rb.AddForce(gravity);
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
        awsd.activate();
    }
    public void DeactivateAWSD()
    {
        awsd.deactivate();
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
}
