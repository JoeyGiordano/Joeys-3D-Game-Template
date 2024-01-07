using UnityEngine;

public class WallRun : MovementState
{
    [Header("Wallrunning")]
    public float wallRunForce = 7;
    public float wallRunGravity = 3;
    public float wallRunHorDrag = 0.3f;
    public float wallRunVertDrag = 0.05f;
    public float maxTime = 1.5f;

    //input
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float minHeightAboveGround = 0.2f;

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
        return false;
    }

    public override void OnEnter(MoveState previousState)
    {
        MoveRes.CapYVelocity(0); //prevent extra y vel from remaining
        if (MoveRes.wallLeft) MoveRes.playerCam.SetFirstPersonTilt(-5, 0.25f);
        if (MoveRes.wallRight) MoveRes.playerCam.SetFirstPersonTilt(5, 0.25f);
    }

    public override void WhileActive()
    {
        MoveRes.getGravity().gravity = wallRunGravity;

        Vector3 wallNormal = MoveRes.wallRight ? MoveRes.rightWallHit.normal : MoveRes.leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((MoveRes.orientation.forward - wallForward).magnitude > (MoveRes.orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Impulse);

        // push to wall force
        if (!(MoveRes.wallLeft && horizontalInput > 0) && !(MoveRes.wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        MoveRes.ApplyXZGroundDrag(wallRunHorDrag);
        MoveRes.ApplyYGroundDrag(wallRunVertDrag);
    }

    public override bool ExitCondition()
    {
        if (secondsSinceEntered > maxTime)
            return true;
        if (!WallRunCondition())
            return true;
        return false;
    }

    public override MoveState OnExit()
    {
        MoveRes.getGravity().ResetGravity();
        MoveRes.playerCam.SetFirstPersonTilt(0, 0.5f);
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        MoveRes.getGravity().ResetGravity();
        MoveRes.playerCam.SetFirstPersonTilt(0, 0.5f);
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
    }

    //non override methods

    private bool WallRunCondition()
    {
        return (MoveRes.wallLeft || MoveRes.wallRight) && verticalInput > 0 && AboveGround();
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(MoveRes.bottomOfPlayer.position, Vector3.down, minHeightAboveGround, MoveRes.terrainLayer);
    }

}
