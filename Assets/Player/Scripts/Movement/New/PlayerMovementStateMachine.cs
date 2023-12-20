using UnityEngine;

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
/// How it works if you want to use it:
///  Really easy!
///  1. Attach this script, MovementResources, and all desired MovementState child scripts to the player object (Free, Jumping, etc)
///  2. If you want to change a certain movement type, you only need to edit the associated script
///  3. Done!
///  4. If you want to add a new movement state, follow the directions below
///
/// 
/// TO ADD A NEW MOVEMENT STATE:       (steps 1-5 can be skipped if you copy a preexisting MovementState child script)
/// 
///  1. Create a new class named [movement state]ing (Jumping for jump, Sliding for slide, etc)
///  2. Make the new class not a MonoBehaviour (by deleting the ': MonoBehaviour') - (note: it will be a MonoBehaviour again when it is a child of MovementState)
///  3. Delete the Start() and Update() methods
///  4. Make the class a child of the abstract class MovementState (add ': MovementState' next to class name)
///  5. There will be an error on the class name, the compiler requires the new class to inherit some methods
///      i.   hover over the error
///      ii.  click show potential fixes
///      iii. click implement abstract class
///      iv.  the inherited methods will be automatically created (don't fill them in yet)
///  6. Find all places in this script with a *TODO*TODO* and do what it says (there are 6)
///  7. The new state has been added to the state machine, you can now implement your new state
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
    public MovementState.MoveState state;

    //*TODO*TODO* create a variable to store your new MovementState child script
    MovementState free;
    MovementState jump;

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
        //*TODO*TODO* check if your new MovementState child script is attached to the gameobject, if it is get and store it
        if (GetComponent<Free>())
            free = GetComponent<Free>();
        if (GetComponent<Jump>())
            jump = GetComponent<Jump>();

    }

    private void StartupMovementStateScripts()
    {
        //get the MovementResources that will be passed to the MovementScripts
        MovementResources moveRes = GetComponent<MovementResources>();

        //*TODO*TODO* call the Startup method on your new MovementState child script, pass this and moveRes
        free.Startup(this, moveRes);
        jump.Startup(this, moveRes);

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
    public void TransitionTo(MovementState.MoveState nextState)
    {
        //store the old state
        MovementState.MoveState previousState = state;
        //change the state variable
        state = nextState;
        //get the script for the new state
        MovementState nextStateScript = MoveStateToScript(nextState);
        //activate the new state script, its WhileActive() will now run every update
        nextStateScript.active = true;
        //run the on enter code (as specified in child), passing the previous state as an argument (to be used in child)
        nextStateScript.OnEnter(previousState);
    }

    public MovementState MoveStateToScript(MovementState.MoveState state)
    {
        //*TODO*TODO* go to the MovementState child script and add your new state to MoveState enum (at the bottom, marked with *TODO*TODO*)
        //*TODO*TODO* in the below switch case, add a case for the the MoveState you just created and return your new MovementState child script 
        switch (state)
        {
            case MovementState.MoveState.free:
                return free;
            case MovementState.MoveState.jumping:
                return jump;
            default:
                Debug.LogError("PlayerMovementStateMachine: MovementState " + state + " has no associated script");
                return null;    //if the code gets here, a MovementScript-MoveState pair is not added
        }
    }
}
