using UnityEngine;

/// <summary>
///
/// ***README***
/// This class...
///  - contains all of the abstract methods that movement state scripts need to inherit
///  - calls those methods in Update() in a specific order so all you have to do is fill in the inherited methods in the child class
///  - has the methods Startup() and Reset() for PlayerMovementStateMachine to call when initiating/resetting the state machine
///  - has the MoveState enum which enumerates all possible movement states
///  - is HEAVILY commented so that you can read it and easily figure out exactly whats going on
///  - is HEAVILY commneted so that you can read it and easily know what to put in the inherited methods
///
///
/// Note: in this script, "activate" does not mean Unity's activate, it refers to the active variable in this script.
///  When active is false Update() will still run. Each update the script will check the EnterCondition(), it just won't run WhileActive().
///
/// </summary>
public abstract class MovementState : MonoBehaviour
{
    //the PlayerMovementStateMachine that is using this instance of MovementState
    PlayerMovementStateMachine stateMachine;

    //associates this MovementState script with a MoveState enum value
    public MoveState myState;
    //whether or not WhileActive() should run during Update()
    public bool active;

    //called by a PlayerMovementStateMachine to give this script necessary references and instantiate things (I don't like using Unity's Start() method, bc less control)
    public void Startup(PlayerMovementStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        myState = getMyState();
        active = false;
        OnStartup();
    }

    //Called when PlayerMovementStateMachine.Reset() is called
    public void Reset()
    {
        active = false;
        OnReset();
    }

    void Update()
    {
        //if exit condition is met...
        if (ExitCondition())
        {
            //run exit code (as specified in child) and obtain the next state (as specified by child, returned from OnExit())
            MoveState nextState = OnExit();
            //deactivate this script, WhileActive() will no longer run during update
            active = false;
            //tell the state machine what state to transition to next
            stateMachine.TransitionTo(nextState);
        }

        //if this state is active...
        if (active)
        {
            //run the while active code (as specified in child)
            WhileActive();
        }

        //if enter condition is met...
        if (EnterCondition())
        {
            //store the active state
            MoveState previousState = stateMachine.state;
            //tell the state machine to deactivate the active state (but first runs OnOverriden() for that state)
            stateMachine.OverrideCurrentState();
            //change the state machines state variable
            stateMachine.state = myState;
            //activate this script, WhileActive() will now run every update
            active = true;
            //run the on enter code (as specified in child), passing the previous state as an argument (to be used in child)
            OnEnter(previousState);
        }

    }

    /// <summary>
    /// A getter to associate MovementState scripts with MoveState enum values.
    /// </summary>
    /// <returns>The MoveState enum value associated with this movement state.</returns>
    public abstract MoveState getMyState();

    /// <summary>
    /// The condition for a state to activate.
    /// </summary>
    /// <returns></returns>
    public abstract bool EnterCondition();

    /// <summary>
    /// The condition for a state to deactivate.
    /// </summary>
    /// <returns></returns>
    public abstract bool ExitCondition();

    /// <summary>
    /// Called when a state is entered, either when the enter condition is met or after the exit of some other state.
    /// </summary>
    /// <param name="previousState"></param>
    public abstract void OnEnter(MoveState previousState);

    /// <summary>
    /// Called when the exit condition is met. Provides the opportunity to reset any changed settings (eg set gravity back to normal).
    /// Must return the state that should come after the exit condition is met (eg if you were in the jumping state and come into contact with ground, return walking).
    /// </summary>
    /// <returns>The state that should come after the exit condition is met.</returns>
    public abstract MoveState OnExit();

    /// <summary>
    /// Called when another state has been forcibly entered (eg by a key press) and the continuation
    /// of the current state has to be cancelled. Provides the opportunity to reset any changed settings (eg set gravity back to normal).
    /// </summary>
    public abstract void OnOverriden();

    /// <summary>
    /// Called when PlayerMovementStateMachine.Reset() is called (when the entire state machine is being reset).
    /// </summary>
    public abstract void OnReset();

    /// <summary>
    /// Called when PlayerMovementStateMachine.StartupMovementStateScripts() is called (typically on Unity's Start()).
    /// </summary>
    public abstract void OnStartup();

    /// <summary>
    /// What happens while a state is active (forces, graphics, etc).
    /// </summary>
    public abstract void WhileActive();

    //enum to store all possible movement states
    public enum MoveState
    {
        //*TODO*TODO* add a new state to this enum for the new movement script
        //other things you need to do to add a new movement state are listed in the readme in PlayerMovementStateMachine 
        free,
        jumping
    }
}
