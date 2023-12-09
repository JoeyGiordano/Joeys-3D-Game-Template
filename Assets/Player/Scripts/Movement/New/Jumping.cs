using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : MovementState
{
    public override bool EnterCondition()
    {
        //if jump key pressed return true, else return false
        return false;
    }

    public override bool ExitCondition()
    {
        //if grounded return true, else return false
        return false;
    }

    public override MoveState getMyState()
    {
        return MoveState.jumping;
    }

    public override void OnEnter(MoveState previousState)
    {
        //apply an upward force

        //if previousState == wallRun, apply additional sideways force make the character jump away from the wall
    }

    public override MoveState OnExit()
    {
        //MoveState.free is the state that will be entered after landing
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        //reset gravity to normal, etc
    }

    public override void OnReset()
    {
        //reset any accumulated values etc
    }

    public override void OnStartup()
    {
        //instantiate anything needed
    }

    public override void WhileActive()
    {
        //if jump key released apply downward force
    }
}
