using UnityEngine;

public class AWSDMovement : MonoBehaviour
{
    //References
    public Transform orientation;
    MovementResources moveRes;
    Rigidbody rb;

    //Active
    private bool active = true;

    //Movement
    public float moveForce = 15;
    public float backForce = 8;
    public float strafeForce = 10;
    public float groundDrag = 0.15f;
    public float airDrag = 0.001f;
    public DragType dragType = DragType.GROUND;
    public float maxSpeed = 10;

    //Input
    float horizontalInput;
    float verticalInput;

    private void Start()
    {
        moveRes = GetComponent<MovementResources>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // calculate movement direction (you can't just add then normalize bc different directions apply different forces)
        //find the combined movement direction
        Vector2 combined = Vector2.up * verticalInput + Vector2.right * horizontalInput;
        //normalize it
        combined.Normalize();
        //reseparate the forces and apply multipliers
        Vector3 forward = moveForce * orientation.forward * Mathf.Max(combined.y, 0);
        Vector3 backward = backForce * orientation.forward * Mathf.Min(combined.y, 0);
        Vector3 strafe = strafeForce * orientation.right * combined.x;
        //recombine the forces to get the move force vector
        Vector3 moveForceVector = forward + backward + strafe;

        //you can only move if xz speed is less than max speed, works as long as drag > 0
        if (moveRes.XZvelocity().magnitude < maxSpeed)
            rb.AddForce(moveForceVector * 10f, ForceMode.Force);

        //apply drag
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

    public void activate()
    {
        active = true;
        rb.freezeRotation = true;
    }
    public void deactivate()
    {
        active = false;
    }

    public enum DragType
    {
        NONE, GROUND, AIR, GROUND_AND_AIR
    }
}