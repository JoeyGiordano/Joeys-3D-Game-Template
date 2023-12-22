using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MovementState
{
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 5f;

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
        if (Input.GetKeyDown(jumpKey))
            return true;
        else
            return false;
    }

    public override void OnEnter(MoveState previousState)
    {
        MoveRes.GetAWSD().AllowDeground();
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(new Vector3(0,jumpForce,0), ForceMode.Impulse);
        //example of using previousState: if previousState == wallRun, apply additional sideways force make the character jump away from the wall
    }

    public override void WhileActive()
    {
        //if jump key released apply downward force?
    }

    public override bool ExitCondition()
    {
        //if grounded, exit state
        if (MoveRes.grounded)
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

}
