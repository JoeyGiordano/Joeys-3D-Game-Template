using UnityEngine;

public class WallRun : MovementState
{
    [Header("Wallrunning")]
    public float wallRunForce;
    public float wallRunGravity;

    [Header("Walljump")]    //this could really be a separate state but if fits just fine here so...   actually maybe it should be a separate state
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float exitWallTime;
    private bool exitingWall;
    private float exitWallTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minHeightAboveGround;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    //note: have not set "desired move speed"

    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.wallRunning;
    }

    public override bool EnterCondition()
    {
        if (WallRunCondition() && stateMachine.isInState(MoveState.free, MoveState.jumping))
            return true;
        return false;
    }

    public override bool UseAWSD()
    {
        return true;
    }

    public override void OnEnter(MoveState previousState)
    {
        exitingWall = false;
        MoveRes.getGravity().gravity = wallRunGravity;
        MoveRes.CapYVelocity(0); //prevent extra y vel from remaining
        if (wallLeft) MoveRes.playerCam.SetFirstPersonTilt(-5, 0.25f);
        if (wallRight) MoveRes.playerCam.SetFirstPersonTilt(5, 0.25f);
    }

    public override void WhileActive()
    {
        if (Input.GetKeyDown(stateMachine.jump.jumpKey)) WallJump();

        if (exitingWall)
        {
            exitWallTimer -= Time.fixedDeltaTime;
            MoveRes.ApplyAirDrag(MoveRes.GetAWSD().airDrag);
            return;
        }

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((MoveRes.orientation.forward - wallForward).magnitude > (MoveRes.orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    public override bool ExitCondition()
    {
        if (exitWallTimer <= 0)
            return true;
        if (!WallRunCondition())
            return true;
        return false;
    }

    public override MoveState OnExit()
    {
        MoveRes.getGravity().ResetGravity();
        if (wallLeft) MoveRes.playerCam.SetFirstPersonTilt(0, 0.25f);
        if (wallRight) MoveRes.playerCam.SetFirstPersonTilt(0, 0.25f);
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        MoveRes.getGravity().ResetGravity();
        if (wallLeft) MoveRes.playerCam.SetFirstPersonTilt(0, 0.25f);
        if (wallRight) MoveRes.playerCam.SetFirstPersonTilt(0, 0.25f);
    }

    public override void OnReset()
    {
        //do nothing
    }

    public override void UpdateChild()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        WallCheck();
    }

    //non override methods

    private bool WallRunCondition()
    {
        return (wallLeft || wallRight) && verticalInput > 0 && AboveGround();
    }

    private void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(MoveRes.bottomOfPlayer.position, Vector3.down, minHeightAboveGround, MoveRes.groundLayer);
    }

    private void WallCheck()
    {
        //I took this wall check from quicksilver, I didn't really bother trying to figure out how it works but it does great and its not that expensive. I think Nate wrote it
        int RaysToShoot = 16;
        int leftDetect = 0;
        int rightDetect = 0;
        //Shoots 4 Rays On Both Left and Right Side for More Generous Wall Detection
        float totalAngle = 180;
        float delta = totalAngle / (RaysToShoot * 2);
        float offset = 45;
        for (int i = 0; i < RaysToShoot; i++)
        {
            var dir = Quaternion.Euler(0, offset + i * delta, 0) * MoveRes.orientation.forward;

            //This looks complicated, but essentially it makes it so if any of the rays hit than it counts as wall running (before it was they all had to hit)
            if (!Physics.Raycast(transform.position, -dir, out rightWallHit, wallCheckDistance, MoveRes.wallLayer) && leftDetect > 0)
            {
                leftDetect -= 1;
            }
            else if (Physics.Raycast(transform.position, -dir, out rightWallHit, wallCheckDistance, MoveRes.wallLayer) && leftDetect < (RaysToShoot / 2))
            {
                leftDetect += 1;
            }

            if (!Physics.Raycast(transform.position, dir, out leftWallHit, wallCheckDistance, MoveRes.wallLayer) && rightDetect > 0)
            {
                rightDetect -= 1;
            }
            else if (Physics.Raycast(transform.position, dir, out leftWallHit, wallCheckDistance, MoveRes.wallLayer) && rightDetect < (RaysToShoot / 2))
            {
                rightDetect += 1;
            }
        }
        wallLeft = !(leftDetect == 0);
        wallRight = !(rightDetect == 0);
    }

}
