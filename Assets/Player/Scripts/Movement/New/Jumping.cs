using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : MovementState
{
    public override MoveState getMyState()
    {
        //just return the corresponding enum
        return MoveState.jumping;
    }

    public override bool EnterCondition()
    {
        //if jump key pressed return true, else return false
        return true;
    }

    public override bool ExitCondition()
    {
        //if ground hit return true, else return false
        return true;
    }

    public override void OnEnter(MoveState previousState)
    {
        //apply an upward force
        //if previous == wallRun, apply additional sideways force
    }

    public override MoveState OnExit()
    {
        //return MoveState.walking because that is the state that will be entered after landing (assumed, if wrong it will just be overriden by something else)
        return MoveState.walking;
    }

    public override void WhileActive()
    {
        //if jump key released apply downward force
        //(idk if thats the way you want it to work but)
    }

    public override void OnOverriden()
    {
        //reset gravity to normal, etc
    }
}
