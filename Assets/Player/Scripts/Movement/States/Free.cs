using UnityEngine;

/// <summary>
/// The simplest movement state child. Does nothing excepts allows AWSD movement.
/// Has no enter condition because this state never override another state
/// Has no exit condition because this state can only end by being overriden
///
/// //Note to people that are copying this to make a new movement state:
/// Several useful references are stored in the parent
///  - player gameObject (Player)
///  - player RigidBody (rb)
///  - MovementResources (MoveRes) has many useful flags and resource methods
///  - PlayerMovementStateMachine (stateMachine) has current state (state) and isInState()
///  - secondsSinceEntered, the time since the state was entered
/// 
/// </summary>
public class Free : MovementState
{
    public override void OnStartup()
    {
        //do nothing
    }

    public override MoveState getMyState()
    {
        return MoveState.free;
    }

    public override bool EnterCondition()
    {
        return false;   //no way to manually enter this state
    }

    public override bool UseAWSD()
    {
        return true;    //allow awsd movement
    }

    public override void OnEnter(MoveState previousState)
    {
        //do nothing
    }

    public override void WhileActive()
    {
        //do nothing
    }

    public override bool ExitCondition()
    {
        return false;   //the state continues until overriden
    }

    public override MoveState OnExit()
    {
        //this method will never actually be called because ExitCondition always returns false
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
