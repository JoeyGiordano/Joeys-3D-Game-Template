using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MovementState
{
    public KeyCode crouchKey = KeyCode.CapsLock;
    public float crouchScale = 0.6f;
    public float moveMultiplier = 0.6f;
    public float cutoffSpeed = 7f;

    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.crouching;
    }

    public override bool EnterCondition()
    {
        if (stateMachine.state == MoveState.free && Input.GetKey(crouchKey))
            return true;
        else
            return false;
    }

    public override bool UseAWSD()
    {
        return true;
    }

    public override void OnEnter(MoveState previousState)
    {
        MoveRes.GetAWSD().moveMultiplier = moveMultiplier;
        MoveRes.GetAWSD().cutoffSpeed = cutoffSpeed;
        MoveRes.ScalePlayerY(crouchScale);
    }

    public override void WhileActive()
    {
        //do nothing
    }

    public override bool ExitCondition()
    {
        if (Input.GetKeyUp(crouchKey))
            return true;
        else
            return false;
    }

    public override MoveState OnExit()
    {
        MoveRes.GetAWSD().ResetValues();
        MoveRes.ScalePlayerY(1/crouchScale);
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        MoveRes.ScalePlayerY(1 / crouchScale);
    }

    public override void OnReset()
    {
        //do nothing
    }

}
