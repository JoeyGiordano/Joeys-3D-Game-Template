using UnityEngine;

/// <summary>
/// Handles WASD movement with Rigidbody Physics.
/// Other forces can be added without affecting functionality. This will not affect other forces (by speed limiting etc) except for freeze rotation.
/// Provides many public variables to change the nature of the movement. Provides resource methods to turn on and off different features. Saves the default values to be reset to during runtime.
/// Handles slopes, grounding, and drag. Does not handle stairs. (Believe me I tried. Just use invisible ramps. If you figure out a way to get stairs to work, please email me)
/// Is pretty heavily commented to try to help you figure whats going on.
/// </summary>
public class AWSDMovement : MonoBehaviour
{
    [Header("References")]
    MovementResources moveRes;
    Rigidbody rb;

    [Header("Flags")]
    private bool active = true;
    private bool allowDeground = false;
    private bool sprinting = false;

    [Header("Move Forces")]
    public float moveForce = 40;       //force for forward movement
    public float backForce = 24;        //force for backward movement, if both w and s are pressed 0 force is applied
    public float strafeForce = 15;      //force for sideways movement
    public float moveMultiplier = 1;     //multiplies the movement force vector always
    public float airMultiplier = 1;  //multiplies the movement force vector when in the air
    public float cutoffSpeed = 13;   //speed at which awsd force no longer applied, ~120% of desired max speed

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintMultiplier = 1.3f;   //multiplies the movement force vector when sprint key is held
    public float sprintCutoffSpeed = 17;    //the cutoff speed while sprint key is held

    [Header("Drag")]
    public float groundDrag = 0.3f;     //player feet only cause static friction
    public float airDrag = 0.001f;
    public DragType dragType = DragType.GROUND;
    public GameObject feet;     //used for static friction with ground (but not for walls)

    [Header("Slope Handling")]
    public float flatGroundingForce = 10;   //grounding force on flat ground
    public float maxGroundingForce = 200;  //max force used for grounding the player
    public float tooSteepSlopeForce = 100;  //max force used for pushing the player down a too steep slope
    public float steepSlopeUpwardYDragMultiplier = 0.45f;

    [Header("Input")]
    Vector2 input;

    [Header("Default Values")]
    [HideInInspector] public float[] savedValues;
    [HideInInspector] public DragType savedDragType;

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

    // *** MovePlayer Methods ***

    private void MovePlayer()
    {
        //Calculate move force and direction from input
        Vector3 moveForceVector = CalculateMoveForce();
        //Update movementInputDirection in MovementResources
        moveRes.movementInputDirection = moveForceVector.normalized;
        //Apply slope adjustments (I tried really hard to add stairs adjustments and it didn't work well enough, lowkey its not worth it just use invisible ramps, please email me if you figure it out)
        moveForceVector = ApplySlopeHandling(moveForceVector);
        //Add force to the player rb
        ApplyMovementForce(moveForceVector);
    }

    private Vector3 CalculateMoveForce()
    {
        //(you can't just add then normalize bc different directions apply different amounts of force)

        //normalize input (make it not faster to go diagonal)
        Vector2 combined = input.normalized;
        //separate the forces, align with orientation, apply multipliers
        Vector3 forward = moveForce * moveRes.orientation.forward * Mathf.Max(combined.y, 0);
        Vector3 backward = backForce * moveRes.orientation.forward * Mathf.Min(combined.y, 0);
        Vector3 strafe = strafeForce * moveRes.orientation.right * combined.x;
        //recombine the forces to get the move force vector
        Vector3 moveForceVector = forward + backward + strafe;

        //Apply Multipliers
        moveForceVector *= moveMultiplier;
        if (!moveRes.grounded)
            moveForceVector *= airMultiplier;
        if (sprinting)  //to make sprinting only work on ground, put this if inside the previous one
            moveForceVector *= sprintMultiplier;

        return moveForceVector;
    }

    private Vector3 ApplySlopeHandling(Vector3 moveForceVector)
    {
        //if grounded make the moveVector parallel to the slope plane 
        if (moveRes.grounded)
            moveForceVector = moveRes.ProjectOnGroundHit(moveForceVector);
        //if slope to steep only allow movement away from slope
        if (moveRes.onTooSteepSlope)
        {
            //find the XZ direction the slope is facing by casting the normal on the XZ place
            Vector3 slopeFacing = moveRes.ProjectOnXZ(moveRes.GroundNormal()).normalized;
            //find the vector pointing downhill
            Vector3 slopeDown = moveRes.ProjectOnGroundHit(slopeFacing).normalized;
            //the moveforce vector is still in th XZ plane 
            Vector3 moveXZDir = moveForceVector.normalized;

            //as the rb slides down a slope
            //the slope applies a kinda upwards force when the rb gets pushed by it
            //this cancels that out to make gravity feel right
            //the cos makes the force right for different slope angles
            if (moveForceVector == Vector3.zero)
                rb.AddForce(slopeDown * tooSteepSlopeForce * 2f * Mathf.Cos(moveRes.GroundAngleRad()), ForceMode.Force);

            //if the movement vector points towards the slope make the move force vector 0
            //prevents wall climbing
            if (Vector3.Dot(slopeFacing, moveXZDir) < 0f)
            {
                return Vector3.zero;
            }
        }

        return moveForceVector;
    }

    private void ApplyMovementForce(Vector3 moveForceVector)
    {
        //you can only move if xz speed is less than cutoff speed, works as long as drag > 0
        //could directly cap velocity but then you wouldn't be able to apply additional forces
        //unfortunately this causes the rb.velocity to jitter around below the cutoff speed, but there is no visual effect so it's ok

        float xzSpeed = moveRes.XZvelocity().magnitude;

        float CutoffSpeed = cutoffSpeed;
        if (sprinting) CutoffSpeed = sprintCutoffSpeed;

        if (xzSpeed <= CutoffSpeed)
            rb.AddForce(moveForceVector * 10f, ForceMode.Force);
    }

    // *** Other Fixed Update Methods ***

    private void ApplyGroundingForce()
    {
        //applied a force perpendicular to the ground to keep the player grounded
        //the force is bigger the steeper the slope is and flatGroundingForce for flat ground
        if (moveRes.grounded && !allowDeground)
        {
            float angle = moveRes.GroundAngleRad();
            float sm = Mathf.Pow(Mathf.Sin(angle), 0.25f); //steepness multiplier, 0 when angle = 0, 1 when angle = pi/2, concave down
            float groundingForce = (maxGroundingForce - flatGroundingForce) * sm + flatGroundingForce; //at sm = 1, gf = maxGF , at sm = 0 gf = flatGF
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
        savedValues = new float[14];

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
        savedValues[10] = flatGroundingForce;
        savedValues[11] = maxGroundingForce;
        savedValues[12] = tooSteepSlopeForce;
        savedValues[13] = steepSlopeUpwardYDragMultiplier;
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
        flatGroundingForce = savedValues[10];
        maxGroundingForce = savedValues[11];
        tooSteepSlopeForce = savedValues[12];
        steepSlopeUpwardYDragMultiplier = savedValues[13];
    }

    public enum DragType
    {
        GROUND, AIR, GROUND_AND_AIR
        //player feet will always cause some drag when on surfaces
    }
}