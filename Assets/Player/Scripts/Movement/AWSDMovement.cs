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
    public float airMultiplier = 1;  //multiplies the movement force vector when in the air
    public float cutoffSpeed = 10;   //speed at which awsd force no longer applied, ~120% of desired max speed

    [Header("Drag")]
    public float groundDrag = 0.3f;
    public float airDrag = 0.001f;
    public DragType dragType = DragType.GROUND;

    [Header("Slope Handling")]
    public float groundingForce = 200;  //max force used for grounding the player
    public float tooSteepSlopeForce = 100;  //max force used for pushing the player down a too steep slope
    public float steepSlopeUpwardYDragMultiplier = 0.45f;

    //Input
    Vector2 input;

    //Save starting values
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
        input = Vector2.zero;
        input += Vector2.right * Input.GetAxisRaw("Horizontal");
        input += Vector2.up * Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {

        //Calculate movement direction (you can't just add then normalize bc different directions apply different forces)

        //normalize input (make it not faster to go diagonal)
        Vector2 combined = input.normalized;
        //reseparate the forces and apply multipliers
        Vector3 forward = moveForce * moveRes.orientation.forward * Mathf.Max(combined.y, 0);
        Vector3 backward = backForce * moveRes.orientation.forward * Mathf.Min(combined.y, 0);
        Vector3 strafe = strafeForce * moveRes.orientation.right * combined.x;
        //recombine the forces to get the move force vector
        Vector3 moveForceVector = forward + backward + strafe;

        //Update movementInputDirection in MovementResources
        moveRes.movementInputDirection = moveForceVector.normalized;

        //Air Multiplier
        if (!moveRes.grounded)
            moveForceVector *= airMultiplier;

        //Slope handling

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
            //prevents normal climbing
            if (Vector3.Dot(slopeFacing, moveXZ) < 0f)
            {
                return;
            }
        }

        //Apply movement force

        //you can only move if xz speed is less than max speed, works as long as drag > 0
        float xzSpeed = moveRes.XZvelocity().magnitude;
        if (xzSpeed <= cutoffSpeed)
            rb.AddForce(moveForceVector * 10f, ForceMode.Force);
    }

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
            ApplyGroundDrag();
            moveRes.ApplyAirDrag(airDrag);
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

    public void Activate()
    {
        active = true;
        rb.freezeRotation = true;
    }
    public void Deactivate()
    {
        active = false;
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
        savedValues = new float[10];

        //Movement
        savedValues[0] = moveForce;
        savedValues[1] = backForce;
        savedValues[2] = strafeForce;
        savedValues[3] = airMultiplier;
        savedValues[4] = cutoffSpeed;

        //Drag
        savedValues[5] = groundDrag;
        savedValues[6] = airDrag;
        savedDragType = dragType;

        //Slope
        savedValues[7] = groundingForce;
        savedValues[8] = tooSteepSlopeForce;
        savedValues[9] = steepSlopeUpwardYDragMultiplier;
    }
    public void Reset()
    {
        active = true;
        allowDeground = false;

        //Movement
        moveForce = savedValues[0];
        backForce = savedValues[1];
        strafeForce = savedValues[2];
        airMultiplier = savedValues[3];
        cutoffSpeed = savedValues[4];

        //Drag
        groundDrag = savedValues[5];
        airDrag = savedValues[6];
        dragType = savedDragType;

        //Slope
        groundingForce = savedValues[7];
        tooSteepSlopeForce = savedValues[8];
        steepSlopeUpwardYDragMultiplier = savedValues[9];
    }

    public enum DragType
    {
        NONE, GROUND, AIR, GROUND_AND_AIR
    }
}