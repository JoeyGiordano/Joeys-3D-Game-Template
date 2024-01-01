using UnityEngine;

public class AirDash : MovementState
{
    public int mouseButtonCode = 1;
    public float airDashImpulse = 30f;
    public float airDashForce = 5f;
    public float airDashDrag = 0.03f;
    public float minAirDashSpeed = 5;

    Vector3 airDashDirection;

    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.airDash;
    }

    public override bool EnterCondition()
    {
        if (Input.GetMouseButtonDown(1) && !active && !MoveRes.grounded)
            return true;
        return false;
    }

    public override bool UseAWSD()
    {
        return false;
    }

    public override void OnEnter(MoveState previousState)
    {
        airDashDirection = MoveRes.facingDirection;
        rb.AddForce(airDashDirection  * airDashImpulse, ForceMode.Impulse);
    }

    public override void WhileActive()
    {
        rb.AddForce(airDashDirection * airDashForce, ForceMode.Force);
        MoveRes.ApplyAirDrag(airDashDrag);
    }

    public override bool ExitCondition()
    {
        if (MoveRes.XZvelocity().magnitude < minAirDashSpeed || MoveRes.grounded)
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
}
