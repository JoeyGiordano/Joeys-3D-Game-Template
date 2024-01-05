using UnityEngine;

public class WallJump : MovementState
{
    public float maxTime = 0.3f;
    public float wallJumpUpForce = 15;
    public float wallJumpSideForce = 8;


    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.free; //TODO
    }

    public override bool EnterCondition()
    {
        if (stateMachine.state == MoveState.wallRunning && Input.GetKeyDown(stateMachine.jump.jumpKey))
            return true;
        return false;
    }

    public override bool UseAWSD()
    {
        return true;
    }

    public override void OnEnter(MoveState previousState)
    {
        //do nothing
    }

    public override void WhileActive()
    {
        Vector3 forceToApply = transform.up * wallJumpUpForce + MoveRes.GetWallNormal() * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    public override bool ExitCondition()
    {
        if (secondsSinceEntered > maxTime || MoveRes.grounded)
            return true;
        return false;
    }

    public override MoveState OnExit()
    {
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        //do nothing
    }

    public override void OnReset()
    {
        //do nothing
    }

    public override void UpdateChild()
    {
        //do nothing
    }
}
