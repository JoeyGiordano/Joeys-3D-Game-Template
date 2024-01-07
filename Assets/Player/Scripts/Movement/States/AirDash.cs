using UnityEngine;

public class AirDash : MovementState
{
    int mouseButtonCode = 1;
    public float airDashImpulse = 50f;
    public float airDashForce = 0f;
    public float airDashDrag = 0.001f;
    public float maxTime = 0.6f;

    Vector3 airDashDirection;

    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.airDashing;
    }

    public override bool EnterCondition()
    {
        if (Input.GetMouseButtonDown(mouseButtonCode) && !active && !MoveRes.grounded)
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
        if (MoveRes.grounded || secondsSinceEntered > maxTime)
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
