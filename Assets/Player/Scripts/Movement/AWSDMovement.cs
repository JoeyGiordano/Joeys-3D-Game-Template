using UnityEngine;

public class AWSDMovement : MonoBehaviour
{
    //References
    MovementResources moveRes;
    Rigidbody rb;

    //settings
    private bool active = true;
    private bool allowDeground = false;

    [Header("Move Forces")]
    public float moveForce = 40;
    public float backForce = 24;
    public float strafeForce = 15;
    public float moveMultiplier = 1;     //multiplies the movement force vector always
    public float airMultiplier = 1;  //multiplies the movement force vector when in the air
    public float cutoffSpeed = 10;   //speed at which awsd force no longer applied, ~120% of desired max speed

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintMultiplier = 1.3f;   //multiplies the movement force vector when sprint key is held
    public float sprintCutoffSpeed = 14;    //the cutoff speed while sprint key is held

    [Header("Drag")]
    public float groundDrag = 0.3f;     //player feet will always cause some drag when on surfaces
    public float airDrag = 0.001f;
    public DragType dragType = DragType.GROUND;
    public GameObject feet;

    [Header("Slope Handling")]
    public float groundingForce = 200;  //max force used for grounding the player
    public float tooSteepSlopeForce = 100;  //max force used for pushing the player down a too steep slope
    public float steepSlopeUpwardYDragMultiplier = 0.45f;

    [Header("Stairs")]
    public float maxStepHeight = 0.3f;
    public float minStepDepth = 0.55f;
    public float stairPush = 10;             //upwards transform movement applied when on stairs
    private float stairCheckAngle = 45;      //anlge at which stair check are performed (in addition to forward)

    //Input
    Vector2 input;
    bool sprinting;

    //Save starting values, (for easy reset)
    float[] savedValues;
    DragType savedDragType;

    private void Start()
    {
        moveRes = GetComponent<MovementResources>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        SaveStartingValues();
    }

    private void Update()
    {
        if (!active) return;

        GetInput();
    }

    private void FixedUpdate()
    {
        if (!active) return;

        MovePlayer();
        ApplyGroundingForce();
        ApplyDrag();

    }

    private void GetInput()
    {
        sprinting = Input.GetKey(sprintKey);

        input = Vector2.zero;
        input += Vector2.right * Input.GetAxisRaw("Horizontal");
        input += Vector2.up * Input.GetAxisRaw("Vertical");
    }

    //MovePlayer Methods

    private void MovePlayer()
    {
        //Calculate move force from input
        Vector3 moveForceVector = CalculateMoveForce();
        //Update movementInputDirection in MovementResources
        moveRes.movementInputDirection = moveForceVector.normalized;
        //Handle Stairs
        HandleStairs(moveForceVector);
        //Apply slope adjustments
        moveForceVector = ApplySlopeHandling(moveForceVector);
        //Add force to the player rb
        ApplyMovementForce(moveForceVector);
    }

    private Vector3 CalculateMoveForce()
    {
        //(you can't just add then normalize bc different directions apply different forces)

        //normalize input (make it not faster to go diagonal)
        Vector2 combined = input.normalized;
        //reseparate the forces and apply multipliers
        Vector3 forward = moveForce * moveRes.orientation.forward * Mathf.Max(combined.y, 0);
        Vector3 backward = backForce * moveRes.orientation.forward * Mathf.Min(combined.y, 0);
        Vector3 strafe = strafeForce * moveRes.orientation.right * combined.x;
        //recombine the forces to get the move force vector
        Vector3 moveForceVector = forward + backward + strafe;

        //Multipliers
        moveForceVector *= moveMultiplier;
        if (!moveRes.grounded)
            moveForceVector *= airMultiplier;
        if (sprinting)
            moveForceVector *= sprintMultiplier;

        return moveForceVector;
    }

    private bool HandleStairs(Vector3 moveForceVector)
    {
        //calculate stairCast inputs
        //a skinny box with height (maxStepHeight - a little big) and length (collider radius)
        Vector3 lowCenter = moveRes.bottomOfPlayer.transform.position + (0.01f + maxStepHeight / 2) * Vector3.up + moveForceVector.normalized * moveRes.coll.radius / 2;
        Vector3 lowHalfExtents = new Vector3(0.2f, maxStepHeight / 2, moveRes.coll.radius);
        //a skinny box bow with height (player height - maxStepHeight - a little bit) and length (collider radius)
        Vector3 highCenter = moveRes.bottomOfPlayer.transform.position + (0.01f + moveRes.coll.height/2 + maxStepHeight) * Vector3.up + moveForceVector.normalized * moveRes.coll.radius / 2;
        Vector3 highHalfExtents = new Vector3(0.2f, (moveRes.coll.height - maxStepHeight - 0.01f) / 2, moveRes.coll.radius);
        //direction is angled up so it doesn't hit any walkable slopes
        Vector3 direction = RotateTowards(moveForceVector.normalized, Vector3.up, moveRes.steepSlopeAngle);
        //length of boxcasts
        float lowDist = moveRes.coll.radius * 1.2f;
        float highDist = minStepDepth;
        //if small step impeding movement
        Debug.DrawRay(lowCenter + moveRes.orientation.forward, highHalfExtents.y * Vector3.up, Color.red, 0.2f);
        if (StepCast(lowCenter, lowHalfExtents, highCenter, highHalfExtents, direction, lowDist, highDist, moveRes.groundLayer))
        {
            Debug.Log("here");
            //move up and return true
            transform.position += stairPush * Vector3.up * Time.deltaTime;
            return true;
        }
        direction = Quaternion.Euler(0, -stairCheckAngle, 0) * direction;   //rotate the cast direction about the y axis
        if (StepCast(lowCenter, lowHalfExtents, highCenter, highHalfExtents, direction, lowDist, highDist, moveRes.groundLayer))
        {
            Debug.Log("here");
            transform.position += stairPush * Vector3.up * Time.deltaTime;
            return true;
        }
        direction = Quaternion.Euler(0, stairCheckAngle * 2, 0) * direction;  //rotate the cast direction to the other side
        if (StepCast(lowCenter, lowHalfExtents, highCenter, highHalfExtents, direction, lowDist, highDist, moveRes.groundLayer))
        {
            Debug.Log("here");
            transform.position += stairPush * Vector3.up * Time.deltaTime;
            return true;
        }
        return false;
    }

    private Vector3 ApplySlopeHandling(Vector3 moveForceVector)
    {
        //if grounded align the moveVector with the slope plane 
        if (moveRes.grounded)
            moveForceVector = moveRes.ProjectOnGroundHit(moveForceVector);
        //if slope to steep and not on stairs only allow movement away from slope
        if (moveRes.onTooSteepSlope)
        {
            Vector3 slopeFacing = moveRes.ProjectOnFlat(moveRes.GroundNormal()).normalized;
            Vector3 slopeDown = moveRes.ProjectOnGroundHit(slopeFacing).normalized;
            Vector3 moveXZ = moveForceVector.normalized;

            //as the rb slides down a slope
            //the slope applies a kinda upwards force when the rb gets pushed by it
            //this cancels that out
            //the cos makes the force right for different slope angles
            //the
            if (moveForceVector == Vector3.zero)
                rb.AddForce(slopeDown * tooSteepSlopeForce * 2f * Mathf.Cos(moveRes.GroundAngleRad()), ForceMode.Force);

            //if the movement vector points towards the slope return before movement force is applied
            //prevents wall climbing
            if (Vector3.Dot(slopeFacing, moveXZ) < 0f)
            {
                return Vector3.zero;
            }
        }

        return moveForceVector;
    }

    private void ApplyMovementForce(Vector3 moveForceVector)
    {
        //you can only move if xz speed is less than cutoff speed, works as long as drag > 0
        float xzSpeed = moveRes.XZvelocity().magnitude;

        float adjustedCutoffSpeed = cutoffSpeed;
        if (sprinting) adjustedCutoffSpeed = sprintCutoffSpeed;

        if (xzSpeed <= adjustedCutoffSpeed)
            rb.AddForce(moveForceVector * 10f, ForceMode.Force);
    }

    //Other Fixed Update Methods

    private void ApplyGroundingForce()
    {
        if (moveRes.grounded && !allowDeground)
        {
            float angle = moveRes.GroundAngleRad();
            float sm = Mathf.Pow(Mathf.Sin(angle), 0.25f); //steepness multiplier, 0 when angle = 0, 1 when angle = pi/2, concave down
            rb.AddForce(groundingForce * sm * -moveRes.GroundNormal(), ForceMode.Force);
        }
    }

    private void ApplyDrag()
    {
        if (dragType == DragType.GROUND)
            ApplyGroundDrag();
        else if (dragType == DragType.AIR)
            moveRes.ApplyAirDrag(airDrag);
        else if (dragType == DragType.GROUND_AND_AIR)
        {
            if (moveRes.grounded) ApplyGroundDrag();
            moveRes.ApplyAirDrag(airDrag); //apply air drag either way, it's realistic
        }
    }

    private void ApplyGroundDrag()
    {
        if (moveRes.grounded)
            moveRes.ApplyXYZGroundDrag(groundDrag);
        else if (moveRes.onTooSteepSlope)
        {
            moveRes.ApplyXZGroundDrag(groundDrag);
            if (rb.velocity.y >= 0)
                moveRes.ApplyYGroundDrag(groundDrag * steepSlopeUpwardYDragMultiplier);
        }
        else
            moveRes.ApplyXZGroundDrag(groundDrag);
    }

    private bool StepCast(Vector3 lowCenter, Vector3 lowHalfExtents, Vector3 highCenter, Vector3 highHalfExtents, Vector3 direction, float lowDist, float highDist, LayerMask mask)
    {
        //if there is a hit on the low box cast...
        if (Physics.BoxCast(lowCenter, lowHalfExtents, direction, moveRes.orientation.rotation, lowDist, mask)) {
            Debug.Log("low");
            //but there isn't a hit on the high boxcast...
            if (!Physics.BoxCast(highCenter, highHalfExtents, direction, moveRes.orientation.rotation, highDist, mask)) {
                //a step has been hit
                Debug.Log("high");
                return true;
            }
        }
        return false;
    }

    private Vector3 RotateTowards(Vector3 start, Vector3 target, float maxDegrees)
    {
        return Vector3.RotateTowards(start, target, maxDegrees * Mathf.Deg2Rad, 0);
    }

    //Resource Methods

    public bool isActive()
    {
        return active;
    }
    public void Activate()
    {
        active = true;
        rb.freezeRotation = true;
        DisallowDeground();
        feet.SetActive(true);
    }
    public void Deactivate()
    {
        active = false;
        feet.SetActive(false);
    }
    public void AllowDeground()
    {
        allowDeground = true;
    }
    public void DisallowDeground()
    {
        allowDeground = false;
    }

    public void SaveStartingValues()
    {
        savedValues = new float[13];

        //Movement
        savedValues[0] = moveForce;
        savedValues[1] = backForce;
        savedValues[2] = strafeForce;
        savedValues[3] = moveMultiplier;
        savedValues[4] = airMultiplier;
        savedValues[5] = cutoffSpeed;

        //Sprint
        savedValues[6] = sprintMultiplier;
        savedValues[7] = sprintCutoffSpeed;

        //Drag
        savedValues[8] = groundDrag;
        savedValues[9] = airDrag;
        savedDragType = dragType;

        //Slope
        savedValues[10] = groundingForce;
        savedValues[11] = tooSteepSlopeForce;
        savedValues[12] = steepSlopeUpwardYDragMultiplier;
    }
    public void ResetValues()
    {
        active = true;
        allowDeground = false;

        //Movement
        moveForce = savedValues[0];
        backForce = savedValues[1];
        strafeForce = savedValues[2];
        moveMultiplier = savedValues[3];
        airMultiplier = savedValues[4];
        cutoffSpeed = savedValues[5];

        //Sprint
        sprintMultiplier = savedValues[6];
        sprintCutoffSpeed = savedValues[7];

        //Drag
        groundDrag = savedValues[8];
        airDrag = savedValues[9];
        dragType = savedDragType;

        //Slope
        groundingForce = savedValues[10];
        tooSteepSlopeForce = savedValues[11];
        steepSlopeUpwardYDragMultiplier = savedValues[12];
    }

    public enum DragType
    {
        GROUND, AIR, GROUND_AND_AIR
        //player feet will always cause some drag when on surfaces
    }
}