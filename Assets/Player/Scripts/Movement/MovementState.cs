using UnityEngine;

/// <summary>
///
/// ***README***
/// This class makes creating a new movement state a fill-in-the-blank process
/// When you make a new movement state you extend this class which requires you to implement (fill in) the abstract methods (the blanks).
/// The abstract methods are then called here in update and fixed update making you implementation work without you having to do anything. 
///
/// This class...
///  - contains all of the abstract methods that movement state scripts need to inherit
///  - calls those methods in Update() and FixedUpdate() in a specific order so all you have to do is fill in the inherited methods in the child class
///  - has the methods Startup() and Reset() for PlayerMovementStateMachine to call when initiating/resetting the state machine
///  - has the MoveState enum which enumerates all possible movement states
///  - records the duration that the state has been active for
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
    //player stuffs
    protected GameObject player;
    protected Rigidbody rb;

    //the PlayerMovementStateMachine that is using this instance of MovementState
    protected PlayerMovementStateMachine stateMachine;
    //the MovementResources associated with stateMachine
    protected MovementResources MoveRes;

    //associates this MovementState script with a MoveState enum value (different for each child)
    [HideInInspector]
    public MoveState myState;
    //whether or not WhileActive() should run during Update()
    [HideInInspector]
    public bool active;

    //timer
    private float timeOfEnter = 0;
    protected float secondsSinceEntered = 0;

    //called by a PlayerMovementStateMachine to give this script necessary references and instantiate things (I don't like using Unity's Start() method, bc less control)
    public void Startup(PlayerMovementStateMachine stateMachine, MovementResources moveRes)
    {
        //set references
        this.stateMachine = stateMachine;
        this.MoveRes = moveRes;
        //get the player object stuff (MovementState child scripts should be attached to a player)
        player = gameObject;
        rb = GetComponent<Rigidbody>();
        //set myState, the enum associated with this script (as specified in child)
        myState = getMyState();
        //deactivate
        active = false;
        //call startup method for child
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
        //update child
        UpdateChild();

        //if exit condition is met...
        if (active && ExitCondition())
        {
            //run exit code (as specified in child) and obtain the next state (as specified by child, returned from OnExit())
            MoveState nextState = OnExit();
            //deactivate this script, WhileActive() will no longer run during update
            active = false;
            //tell the state machine what state to transition to next
            stateMachine.TransitionTo(nextState);
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
            //turn on/off awsd movement
            if (UseAWSD()) MoveRes.ActivateAWSD(); else MoveRes.DeactivateAWSD();
            //run the on enter code (as specified in child), passing the previous state as an argument (to be used in child)
            OnEnter(previousState);
            //start timer
            timeOfEnter = Time.time;
        }

    }

    private void FixedUpdate()
    {
        //if this state is active...
        if (active)
        {
            //update timer
            secondsSinceEntered = Time.time - timeOfEnter;
            //run the while active code (as specified in child)
            WhileActive();
        }
    }

    /// <summary>
    /// A getter to associate MovementState child scripts with MoveState enum values.
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
    /// Whether or not to use AWSD movement during this state.
    /// </summary>
    /// <returns></returns>
    public abstract bool UseAWSD();

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
    /// What happens while a state is active (forces, graphics, etc). Called during FixedUpdate()
    /// </summary>
    public abstract void WhileActive();

    /// <summary>
    /// Called during Update (before anything else), most children will not use this
    /// </summary>
    public abstract void UpdateChild();

    //enum to store all possible movement states
    public enum MoveState
    {
        //*TODO*TODO* add a new state to this enum for the new movement script
        //other things you need to do to add a new movement state are listed in the readme in PlayerMovementStateMachine 
        free,
        jumping,
        crouching,
        sliding,
        airDashing,
        grappling,
        wallRunning,
        wallJumping
    }
}
