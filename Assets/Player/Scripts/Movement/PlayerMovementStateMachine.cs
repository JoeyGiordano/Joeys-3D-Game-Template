using UnityEngine;
using static MovementState;

/// <summary>
///
/// ***README***
/// this class...
///  - stores the current active movement state (stored in a variable called "state" of type enum MoveState, the enum is in the MovementState script)
///  - manages transitions between movement states (using OverrideCurrentState() and TransitionTo())
///  - stores a MovementState child script for each type of movement state
///  - provides a way to get the MovementState child script that corresponds to a certain MoveState enum value (MoveStateToScript())
///  - is HEAVILY commented so that you can read it and easily figure out exactly what's going on
///
/// 
/// TO ADD A NEW MOVEMENT STATE:       (steps 1-5 can be skipped if you copy code from a preexisting MovementState child script)
/// 
///  1. Create a new class named [movement state]ing (Jumping for jump, Sliding for slide, etc)
///  2. Make the new class not a MonoBehaviour (by deleting the ': MonoBehaviour') - (note: it will be a MonoBehaviour again when it is a child of MovementState)
///  3. Delete the Start() and Update() methods
///  4. Make the class a child of the abstract class MovementState (add ': MovementState' next to class name)
///  5. There will be an error on the class name, the compiler requires the new class to inherit some methods
///      i.   hover your mouse over the error
///      ii.  click show potential fixes
///      iii. click implement abstract class
///      iv.  the inherited methods will be created automatically (don't fill them in yet)
///  6. Find all places in this script with a *TODO*TODO* and do what it says (there are 6)
///  7. Add the script to the Player gameobject
///  8. The new state has been added to the state machine, you can now implement your new state
///      i.   delete the throw statement in every auto-generated method from step 5
///      ii.  implement the methods, see the MovementState script for what they do or look at other states as examples
///
/// 
/// Note: in this script, "activate" does not mean Unity's activate, it refers to the active variable in MovementState.
///  When active is false Update() will still run. Each update the script will check the EnterCondition(), it just won't run WhileActive().
/// 
/// </summary>
public class PlayerMovementStateMachine : MonoBehaviour
{
    //the current movement state
    public MoveState state;

    //movement resources script
    MovementResources moveRes;

    //*TODO*TODO* create a variable to store your new MovementState child script
    [HideInInspector] public Free free;
    [HideInInspector] public Jump jump;
    [HideInInspector] public Crouch crouch;
    [HideInInspector] public Slide slide;
    [HideInInspector] public AirDash airDash;
    [HideInInspector] public Grapple grapple;
    [HideInInspector] public WallRun wallRun;
    [HideInInspector] public WallJump wallJump;

    void Start()
    {
        Startup();
    }

    private void Startup()
    {
        CollectReferences();
        StartupMovementStateScripts();
    }

    private void CollectReferences()
    {
        moveRes = GetComponent<MovementResources>();

        //*TODO*TODO* get and store your new movement state child script
        free = GetComponent<Free>();
        jump = GetComponent<Jump>();
        crouch = GetComponent<Crouch>();
        slide = GetComponent<Slide>();
        airDash = GetComponent<AirDash>();
        grapple = GetComponent<Grapple>();
        wallRun = GetComponent<WallRun>();
        wallJump = GetComponent<WallJump>();

    }

    private void StartupMovementStateScripts()
    {
        //*TODO*TODO* call the Startup method on your new MovementState child script, pass this and moveRes
        free.Startup(this, moveRes);
        jump.Startup(this, moveRes);
        crouch.Startup(this, moveRes);
        slide.Startup(this, moveRes);
        airDash.Startup(this, moveRes);
        grapple.Startup(this, moveRes);
        wallRun.Startup(this, moveRes);
        wallJump.Startup(this, moveRes);

    }

    /// <summary>
    /// Resets the entire state machine.
    /// </summary>
    public void Reset()
    {
        //to ensure everything gets set back to normal
        OverrideCurrentState();

        //*TODO*TODO* call the Reset method on your new MovementState child script
        free.Reset();
        jump.Reset();
        crouch.Reset();
        slide.Reset();
        airDash.Reset();
        grapple.Reset();
        wallRun.Reset();
        wallJump.Reset();

    }

    /// <summary>
    /// Used when entering a state, ie to terminate the current state before a different MovementState child script activates itself
    /// </summary>
    public void OverrideCurrentState()
    {
        //get the script associated with the current state
        MovementState currentStateScript = MoveStateToScript(state);
        //run OnOverriden(), allowing the script to reset any changed settings before deactivating (eg change gravity back to normal)
        currentStateScript.OnOverriden();
        //deactivate the script, its WhileActive() will no longer run each update
        currentStateScript.active = false;
    }

    /// <summary>
    /// Used when exiting a state, ie after a MovementState child script deactivates itself,
    /// or to transition into a state when no state was active (like at initiation).
    /// </summary>
    /// <param name="nextState"></param>
    public void TransitionTo(MoveState nextState)
    {
        //store the old state
        MoveState previousState = state;
        //change the state variable
        state = nextState;
        //get the script for the new state
        MovementState nextStateScript = MoveStateToScript(nextState);
        //activate the new state script, its WhileActive() will now run every update
        nextStateScript.active = true;
        //turn on/off awsd movement
        if (nextStateScript.UseAWSD()) moveRes.ActivateAWSD(); else moveRes.DeactivateAWSD();
        //run the on enter code (as specified in child), passing the previous state as an argument (to be used in child)
        nextStateScript.OnEnter(previousState);
    }

    public MovementState MoveStateToScript(MoveState state)
    {
        //*TODO*TODO* go to the MovementState child script and add your new state to MoveState enum (at the bottom, marked with *TODO*TODO*)
        //*TODO*TODO* in the below switch case, add a case for the the MoveState you just created and return your new MovementState child script 
        switch (state)
        {
            case MoveState.free:
                return free;
            case MoveState.jumping:
                return jump;
            case MoveState.crouching:
                return crouch;
            case MoveState.sliding:
                return slide;
            case MoveState.airDashing:
                return airDash;
            case MoveState.grappling:
                return grapple;
            case MoveState.wallRunning:
                return wallRun;
            case MoveState.wallJumping:
                return wallJump;
            default:
                Debug.LogError("PlayerMovementStateMachine: MovementState " + state + " has no associated script");
                return null;    //if the code gets here, a MovementScript-MoveState pair is not added
        }
    }

    public bool isInState(params MoveState[] states)
    {
        foreach (MoveState s in states)
            if (s == state)
                return true;
        return false;
    }
}
