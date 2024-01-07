using UnityEngine;

public class Slide : MovementState
{
    public KeyCode slideKey = KeyCode.LeftControl;
    public float slideScale = 0.6f;
    public float slideImpulse = 25;
    public float slideForce = 2;
    public float slideDrag = 0.02f;
    public float minSlideSpeed = 4;

    private Vector3 slideDirection;

    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.sliding;
    }

    public override bool EnterCondition()
    {
        if (stateMachine.state == MoveState.free
            && Input.GetKeyDown(slideKey)
            && rb.velocity.magnitude >= minSlideSpeed)
            return true;
        return false;
    }

    public override bool UseAWSD()
    {
        return false;
    }

    public override void OnEnter(MoveState previousState)
    {
        MoveRes.ScalePlayerY(slideScale);
        slideDirection = MoveRes.orientation.forward;
        rb.AddForce(slideDirection * slideImpulse, ForceMode.Impulse);
    }

    public override void WhileActive()
    {
        rb.AddForce(slideDirection * slideForce, ForceMode.Force);
        MoveRes.ApplyXYZGroundDrag(slideDrag);
    }

    public override bool ExitCondition()
    {
        if (Input.GetKeyUp(slideKey) || rb.velocity.magnitude < minSlideSpeed)
            return true;
        return false;
    }

    public override MoveState OnExit()
    {
        MoveRes.ScalePlayerY(1/slideScale);
        if (rb.velocity.magnitude < minSlideSpeed)
            return MoveState.crouching;
        else
            return MoveState.free;
    }

    public override void OnOverriden()
    {
        MoveRes.ScalePlayerY(1 / slideScale);
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
