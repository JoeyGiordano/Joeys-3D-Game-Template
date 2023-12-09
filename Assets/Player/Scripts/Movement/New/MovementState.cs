using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementState : MonoBehaviour
{
    /// <summary>
    ///
    /// ***README***
    /// This class...
    /// Contains all of the abstract methods that movement states need to inherit
    /// Calls those methods in its Update function so you just have to fill in the inherited methods
    /// Has a Startup method for PlayerMovementStateMachine to call to initiate/reset the state machine
    /// Has the MoveState enum which holds all possible movement states
    /// Is HEAVILY commented so that you can read it and easily figure out exactly whats going on
    /// Is HEAVILY commneted so that you can read it and easily know what to put in the inherited methods
    /// 
    /// </summary>


    PlayerMovementStateMachine stateMachine;     //the PlayerMovementStateMachine that is using this instance of MovementState

    public MoveState myState;       //associates this MovementState script with a MoveState enum value
    public bool active;     //whether or not WhileActive() is running every update


    //called by a PlayerMovementStateMachine to give this script necessary references and instantiate values (I don't like using Unity's Start() method, bc less control)
    public void Startup(PlayerMovementStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        myState = getMyState();
        active = false;
    }

    void Update()
    {
        //if exit condition is met...
        if (ExitCondition())
        {
            //run exit code (as specified in child) and obtain the next state (as specified in child, returned from OnExit())
            MoveState nextState = OnExit();
            //deactivate this script, the WhileActive() will no longer run during update
            active = false;
            //tell the state machine what state to transition to
            stateMachine.TransitionTo(nextState);
        }

        //if this state is active...
        if (active)
        {
            //run the while active code
            WhileActive();
        }

        //if enter condition is met...
        if (EnterCondition())
        {
            //store the old state
            MoveState previousState = stateMachine.state;
            //tell the state machine to deactivate the old state (also runs OnOverriden() for the old state)
            stateMachine.OverrideCurrentState();
            //change the state machines state variable
            stateMachine.state = myState;
            //activate the new script, WhileActive() will now run every update
            active = true;
            //run OnEnter, passing the previous state as an argument (to be used in child)
            OnEnter(previousState);
        }

    }

    /// <summary>
    /// Just a getter to associate MovementState scripts with MoveState enum values.
    /// </summary>
    /// <returns>The MoveState enum value associated with this movement state.</returns>
    public abstract MoveState getMyState();

    /// <summary>
    /// The condition for a state to activate.
    /// </summary>
    /// <returns></returns>
    public abstract bool EnterCondition();

    /// <summary>
    /// Called when a state is entered, either when the enter condition is met or from the exit of some other state.
    /// </summary>
    /// <param name="previousState"></param>
    public abstract void OnEnter(MoveState previousState);

    /// <summary>
    /// What happens while a state is active (eg nothing, forces, graphics, etc).
    /// </summary>
    public abstract void WhileActive();

    /// <summary>
    /// The condition for a state to deactivate.
    /// </summary>
    /// <returns></returns>
    public abstract bool ExitCondition();

    /// <summary>
    /// Called when the exit condition is met. Provides an opportunity to set gravity back to normal etc.
    /// Must return the state that should come next, ie if you were jumping and just came into contact with ground (exit condition), return walking.
    /// </summary>
    /// <returns>The state that should come next.</returns>
    public abstract MoveState OnExit();

    /// <summary>
    /// Called when another state has been forcibly entered (eg by a key press) and the continuation
    /// of the current state has to be cancelled, provides an opportunity to set gravity back to normal etc.
    /// </summary>
    public abstract void OnOverriden();

    //enum to store all possible movement states
    public enum MoveState
    {
        //*TODO*TODO* add a new state to this enum for the new movement script
        //*TODO*TODO* there are other things you need to do in PlayerMovementStateMachine 
        jumping,
        walking
    }
}
