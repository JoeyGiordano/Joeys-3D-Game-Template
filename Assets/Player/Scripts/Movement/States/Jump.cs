using UnityEngine;

public class Jump : MovementState
{
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 5f;

    private bool leftGround = false;

    public override void OnStartup()
    {
        //instantiate anything needed
    }

    public override MoveState getMyState()
    {
        return MoveState.jumping;
    }

    public override bool EnterCondition()
    {
        if (Input.GetKeyDown(jumpKey) /*&& !active*/)     //&& !active prevents double jumping
            return true;
        else
            return false;
    }

    public override bool UseAWSD()
    {
        return true;    //allow awsd movement
    }

    public override void OnEnter(MoveState previousState)
    {
        MoveRes.ActivateAWSD();
        MoveRes.GetAWSD().AllowDeground();
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(new Vector3(0,jumpForce,0), ForceMode.Impulse);
        leftGround = false;
        //example of using previousState: if previousState == wallRun, apply additional sideways force make the character jump away from the wall
    }

    public override void WhileActive()
    {
        if (!MoveRes.grounded) leftGround = true;
        //if jump key released apply downward force?
    }

    public override bool ExitCondition()
    {
        //if grounded, exit state
        if (leftGround && MoveRes.grounded)
            return true;
        else
            return false;
    }

    public override MoveState OnExit()
    {
        //if you land on the ground transition to free
        MoveRes.GetAWSD().DisallowDeground();
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        //do anything needed to set things back to normal
        MoveRes.GetAWSD().DisallowDeground();
    }

    public override void OnReset()
    {
        //reset any accumulated values etc
    }

    public override void UpdateChild()
    {
        //do nothing
    }
}
