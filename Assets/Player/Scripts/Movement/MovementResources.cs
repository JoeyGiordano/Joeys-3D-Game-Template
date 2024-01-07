using UnityEngine;

/// <summary>
/// 
/// this class...
///  - stores references to useful components, scripts, and transforms
///  - stores and updates the values of useful flags (eg grounded, leftWall)
///  - provides MANY useful resource methods
///
/// If you are adding movement states and need somewhere to put resources, add more of the above things to this script
///
/// Unrelated note: scaling the player can mess some things up but I tried to make as few things break as possible. Make sure the colliders are all in the right places (feet!).
/// 
/// </summary>
public class MovementResources : MonoBehaviour
{
    [Header("References")]
    public GameObject playerModel;
    public PlayerCam playerCam;
    Rigidbody rb;
    CapsuleCollider coll;
    Gravity grav;
    AWSDMovement awsd;

    [Header("Locations")]
    public Transform orientation;       //facing of the player (when cameras are set up correctly)
    public Transform topOfPlayer;
    public Transform bottomOfPlayer;
    public Transform centerOfPlayer;
    public Transform eyes;              //where the 1st person camera is/would be on the player object

    [Header("Grounding")]
    public bool grounded = false;       //true when the spherecast hits the ground, and the slope is less than maxSlopeAngleConsideredGround 
    public bool onTooSteepSlope = false;   //true when the spherecast hits the ground, but the slope is more than maxSlopeAngleConsideredGround
    public float steepSlopeAngle = 45f;    //what steepness of slope is not considered grounded (used in AWSDMovement as the max climbing steepness)
    public RaycastHit groundHit;    //what the grounding raycast is hitting, only meaningful when grounded

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.2f;      //distance beyond the collider to check the 
    public RaycastHit leftWallHit;              //the most recent leftward raycast that hit a wall this update
    public RaycastHit rightWallHit;             //the most recent rightward raycast that hit a wall this update
    public bool wallLeft = false;               //true when one of the leftward raycasts hits a wall
    public bool wallRight = false;              //true when one of the rightward raycasts hits a wall

    [Header("Layer Masks")]
    public LayerMask terrainLayer;

    [Header("Directions")]
    [HideInInspector] public Vector3 movementInputDirection;     //used by cameras, the current direction of the player movement input, set by AWSDMovement in fixed update, can be set by other movementstate scripts (normalize!)
    [HideInInspector] public Vector3 facingDirection;       //direction the active camera is facing

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
        WallCheck();
        GetFacingDirection();
    }

    private void FixedUpdate()
    {
        //noise kill
        KillVelocityBelow(0.001f);
    }

    //Ground Check
    private void GroundCheck()
    {
        //stores the resulting RaycastHit in groundHit
        Vector3 castFrom = bottomOfPlayer.position + new Vector3(0,coll.radius,0);
        //grounded is true if Spherecast hits
        grounded = Physics.SphereCast(castFrom, 0.98f * coll.radius, Vector3.down, out groundHit, 0.1f, terrainLayer);
        //if the slope is too steep
        if (grounded && GroundAngleDeg() > steepSlopeAngle)
        {
            //turn off grounded
            grounded = false;
            //turn on onTooSteepSlope
            onTooSteepSlope = true;
        } else onTooSteepSlope = false;
    }

    //Facing Direction - uses the direction of the active camera
    private void GetFacingDirection()
    {
        facingDirection = Camera.main.gameObject.transform.forward;
    }

    //Gravity
    public Gravity getGravity()
    {
        return grav;
    }

    //Scale - more complex than you would think because of the colliders etc
    public void ScalePlayerAndModelXYZ(float scaleMultiplier)
    {
        if (scaleMultiplier <= 0) { Debug.Log("Illegal scale multiplier"); return; }
        transform.parent.transform.localScale *= scaleMultiplier;
    }
    public void ScalePlayerY(float scaleMultiplier)
    {
        if (scaleMultiplier <= 0) { Debug.Log("Illegal scale multiplier"); return; }
        if (coll.radius * 2 > coll.height * scaleMultiplier) { Debug.Log("YScale multiplier too small"); scaleMultiplier = coll.height / (2 * coll.radius); }
        float oldHeight = coll.height;
        //scale capsule collider height
        coll.height *= scaleMultiplier;
        //move capsule collider center
        float heightChange = coll.height - oldHeight;
        coll.center += heightChange/2 * Vector3.up;
        //scale orientation
        orientation.localScale = new Vector3(orientation.localScale.x, orientation.localScale.y * scaleMultiplier, orientation.localScale.z);
        playerModel.transform.localScale = new Vector3(playerModel.transform.localScale.x, playerModel.transform.localScale.y * scaleMultiplier, playerModel.transform.localScale.z);
    }
    public void ScalePlayerXZ(float scaleMultiplier)
    {
        if (scaleMultiplier <= 0) { Debug.Log("Illegal scale multiplier"); return; }
        if (coll.radius * scaleMultiplier > coll.height/2) { Debug.Log("XZScale multiplier to big"); scaleMultiplier = coll.height / (2 * coll.radius); }
        //change the collider radius
        coll.radius *= scaleMultiplier;
        //scale orientation
        orientation.localScale = new Vector3(orientation.localScale.x * scaleMultiplier, orientation.localScale.y, orientation.localScale.z * scaleMultiplier);
        //adjust feet
        //get the feet
        GameObject feet = GetAWSD().feet;
        bool feetWereActive = feet.activeInHierarchy;
        feet.SetActive(true);
        SphereCollider feetColl = feet.GetComponent<SphereCollider>();
        float oldR = feetColl.radius;
        //scale the feet
        feetColl.radius *= scaleMultiplier;
        //move the feet
        float rChange = feetColl.radius - oldR;
        feetColl.center += rChange * Vector3.up;
        //deactivate the feet if necessary
        feet.SetActive(feetWereActive);
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

    //Velocity Components
    public Vector3 XZvelocity()
    {
        return new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }
    public Vector3 Yvelocity()
    {
        return new Vector3(0, rb.velocity.y, 0);
    }

    //Kill Velocity - if the velocity is below min speed, set velocity to 0
    public void KillVelocityBelow(float minSpeed)
    {
        if (rb.velocity.magnitude < minSpeed)
            rb.velocity = Vector3.zero;
    }
    public void KillXZvelocityBelow(float minSpeed)
    {
        if (XZvelocity().magnitude < minSpeed)
            rb.velocity = Yvelocity();
    }
    public void KillYvelocityBelow(float minSpeed)
    {
        if (rb.velocity.y < minSpeed)
            rb.velocity = XZvelocity();
    }

    //Cap Velocity - if velocity is above maxSpeed, reduces velocity to maxSpeed
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

    //Drag - applies direct drag directly to velocity (either linear for ground, or square for air)
    public void ApplyXZGroundDrag(float drag)
    {
        rb.velocity -= drag * XZvelocity();
    }
    public void ApplyXYZGroundDrag(float drag)
    {
        rb.velocity -= drag * rb.velocity;
    }
    public void ApplyYGroundDrag(float drag)
    {
        rb.velocity -= drag * Yvelocity();
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

    //Extra Falling Force
    public void ApplyExtraFallingForce(float force)
    {
        if (rb.velocity.y < 0)
            rb.AddForce(force * Vector3.down, ForceMode.Force);
    }

    //Ground Results - results from grounding raycast
    public Vector3 ProjectOnGroundHit(Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, groundHit.normal);
    }
    public Vector3 ProjectOnXZ(Vector3 v)
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

    //Wall Cast - wall raycast and results
    private void WallCheck()
    {
        wallRight = false;
        wallLeft = false;
        //I took this wall check from quicksilver, I didn't really bother trying to figure out how it works but it does great and its not that expensive. I think Nate wrote it. Actually I made some edits
        int RaysToShoot = 16;
        //Shoots 4 Rays On Both Left and Right Side for More Generous Wall Detection
        float delta = 180 / (RaysToShoot * 2);
        float offset = 45;
        //This looks complicated, but essentially it makes it so if any of the rays hit than it counts as wall running
        for (int i = 0; i < RaysToShoot; i++)
        {
            var dir = Quaternion.Euler(0, offset + i * delta, 0) * orientation.forward;
            bool leftCast = Physics.Raycast(transform.position, -dir, out leftWallHit, coll.radius + wallCheckDistance, terrainLayer);
            if (leftCast)
            {
                wallLeft = true;
                break;
            }
        }
        for (int i = 0; i < RaysToShoot; i++)
        {
            var dir = Quaternion.Euler(0, offset + i * delta, 0) * orientation.forward;
            bool rightCast = Physics.Raycast(transform.position, dir, out rightWallHit, coll.radius + wallCheckDistance, terrainLayer);
            if (rightCast)
            {
                wallRight = true;
                break;
            }
        }
    }
    public Vector3 GetWallNormal()
    {
        if (wallRight)
            return rightWallHit.normal;
        if (wallLeft)
            return leftWallHit.normal;
        return Vector3.zero;
    }
}
