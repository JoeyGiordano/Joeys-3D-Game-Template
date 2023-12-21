using UnityEngine;

public class AWSDMovement : MonoBehaviour
{
    //References
    public Transform orientation;
    MovementResources moveRes;
    Rigidbody rb;

    //Active
    private bool active = true;

    [Header("Move Forces")]
    public float moveForce = 15;
    public float backForce = 8;
    public float strafeForce = 10;
    public float maxSpeed = 10;

    [Header("Drag")]
    public float groundDrag = 0.15f;
    public float airDrag = 0.001f;
    public DragType dragType = DragType.GROUND;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 50;   //TODO  //if a slope is steeper than this, you cannot use awsd movement to climb it
    public float groundingForce = 10;   //TODO also check the speed on slopes

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

        ApplyGroundingForce();
        //ApplySlopeAdjust();
        ApplyDrag();
        MovePlayer();
    }

    private void GetInput()
    {
        input = Vector2.zero;
        input += Vector2.right * Input.GetAxisRaw("Horizontal");
        input += Vector2.up * Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        //calculate movement direction (you can't just add then normalize bc different directions apply different forces)

        //normalize input (make it not faster to go diagonal)
        Vector2 combined = input.normalized;
        //reseparate the forces and apply multipliers
        Vector3 forward = moveForce * orientation.forward * Mathf.Max(combined.y, 0);
        Vector3 backward = backForce * orientation.forward * Mathf.Min(combined.y, 0);
        Vector3 strafe = strafeForce * orientation.right * combined.x;
        //recombine the forces to get the move force vector
        Vector3 moveForceVector = forward + backward + strafe;

        //apply movement force
        //you can only move if xz speed is less than max speed, works as long as drag > 0
        float xzSpeed = moveRes.XZvelocity().magnitude;
        if (xzSpeed < 0.95 * maxSpeed)
            rb.AddForce(moveForceVector * 10f, ForceMode.Force);
        else
        {
            rb.AddForce(moveForceVector * 10f * Mathf.Exp(-(xzSpeed - 0.95f * maxSpeed)), ForceMode.Force);
        }
    }

    private void ApplySlopeAdjust()
    {

    }

    private void ApplyGroundingForce()
    {
        if (moveRes.grounded)
        {
            float angle = moveRes.GroundSlopeRad();
            float sm = Mathf.Sqrt(Mathf.Sin(angle)); //steepness multiplier, 0 when angle = 0, 1 when angle = pi/2, concave down
            rb.AddForce(groundingForce * sm * -moveRes.GroundNormal(), ForceMode.Force);
        }

    }

    private void ApplyDrag()
    {
        if (dragType == DragType.GROUND)
            moveRes.ApplyGroundDrag(groundDrag);
        else if (dragType == DragType.AIR)
            moveRes.ApplyAirDrag(airDrag);
        else if (dragType == DragType.GROUND_AND_AIR)
        {
            moveRes.ApplyGroundDrag(groundDrag);
            moveRes.ApplyAirDrag(airDrag);
        }
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

    public void SaveStartingValues()
    {
        savedValues = new float[8];

        //Movement
        savedValues[0] = moveForce;
        savedValues[1] = backForce;
        savedValues[2] = strafeForce;
        savedValues[3] = maxSpeed;

        //Drag
        savedValues[4] = groundDrag;
        savedValues[5] = airDrag;
        savedDragType = dragType;

        //Slope
        savedValues[6] = maxSlopeAngle;
        //savedValues[7] = minSlopeSlipAnlge;
    }
    public void Reset()
    {
        //Movement
        moveForce = savedValues[0];
        backForce = savedValues[1];
        strafeForce = savedValues[2];
        maxSpeed = savedValues[3];

        //Drag
        groundDrag = savedValues[4];
        airDrag = savedValues[5];
        dragType = savedDragType;

        //Slope
        maxSlopeAngle = savedValues[6];
        //minSlopeSlipAnlge = savedValues[7];
    }

    public enum DragType
    {
        NONE, GROUND, AIR, GROUND_AND_AIR
    }
}