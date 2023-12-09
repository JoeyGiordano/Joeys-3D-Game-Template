using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Free : MovementState
{
    public override bool EnterCondition()
    {
        return false;   //no way to manually enter this state
    }

    public override bool ExitCondition()
    {
        return false;   //the state continues until overriden
    }

    public override MoveState getMyState()
    {
        return MoveState.free;
    }

    public override void OnEnter(MoveState previousState)
    {
        //do nothing
    }

    public override MoveState OnExit()
    {
        return MoveState.jumping;   //doesn't actually do anything because ExitCondition() is always false
    }

    public override void OnOverriden()
    {
        //do nothing
    }

    public override void OnReset()
    {
        //do nothing
    }

    public override void OnStartup()
    {
        //do nothing
    }

    public override void WhileActive()
    {
        //do nothing
    }
}
