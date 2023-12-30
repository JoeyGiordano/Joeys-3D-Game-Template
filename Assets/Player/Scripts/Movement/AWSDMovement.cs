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

    private Vector3 ApplySlopeHandling(Vector3 moveForceVector)
    {
        //if grounded align the moveVector with the slope plane 
        if (moveRes.grounded)
            moveForceVector = moveRes.ProjectOnGroundHit(moveForceVector);
        //if slope to steep only allow movement away from slope
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