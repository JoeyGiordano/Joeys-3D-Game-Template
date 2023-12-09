using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovementStateMachine : MonoBehaviour
{
    /// <summary>
    ///
    /// ***README***
    /// this class...
    /// stores the current active movement state (in a variable called state of type enum MoveState, the enum is in MovementState)
    /// manages transitions between movement states (OverrideCurrentState() and TransitionTo())
    /// stores a MovementState script for each type of movement state
    /// provides a way to get a MovementState script given an MoveState enum value (MoveStateToScript())
    /// Is HEAVILY commented so that you can read it and easily figure out exactly whats going on
    /// 
    /// 
    /// TO ADD A NEW MOVEMENT STATE:
    /// 
    /// 1. Create a new class named [movement state]ing (Jumping for jump, Sliding for slide, etc)
    /// 2. Make that class a child class of the abstract class MovementState (delete the : MonoBehaviour, add : MovementState)
    /// 3. The compiler will then require you to inherit seven methods, see the MovementState script for what each one should do (you don't have to fill them out right away)
    /// 4. Find all places in this script (5) and the MovementState script (1) with a *TODO*TODO* and do what it says
    /// 5. If 1-4 is done it should work, you may need to modify other scripts to get all the transitions up and running
    ///
    /// 
    /// note: in this script activate does not mean Unity's activate, it refers to the active variable in MovementState
    /// 
    /// </summary>

    public MovementState.MoveState state;   //the current movement state

    //*TODO*TODO* create a variable to store your new MovementState script
    MovementState jumping;


    void Start()
    {
        CollectReferences();
        StartupMovementStateScripts();
    }

    private void CollectReferences()
    {
        //*TODO*TODO* get and store your new MovementState script
        jumping = GetComponent<Jumping>();

    }

    private void StartupMovementStateScripts() {
        //*TODO*TODO* run the Startup method on your new MovementState script, pass this
        jumping.Startup(this);

    }

    /// <summary>
    /// Used when entering a state, ie to terminate the current state before a different MovementState script activates itself
    /// </summary>
    public void OverrideCurrentState()
    {
        //get the script associated with the current state
        MovementState currentStateScript = MoveStateToScript(state);
        //run OnOverriden(), allowing the script to make any necessary changes before deactivating
        currentStateScript.OnOverriden();
        //deactivate the script, its WhileActive() will no longer run each update
        currentStateScript.active = false;
    }

    /// <summary>
    /// Used when exiting a state, ie after a MovementState script deactivates itself,
    /// or to transition into a state when no state is active (like at initiation).
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
        //activate the new script, its WhileActive() will now run every update
        nextStateScript.active = true;
        //run OnEnter for the new state script, passing the previous state as an argument (to be used in child)
        nextStateScript.OnEnter(previousState);
    }

    public MovementState MoveStateToScript(MovementState.MoveState state)
    {
        //*TODO*TODO* don't forget to go to the MovementState script and add the new state to enum MoveState
        //*TODO*TODO* add a case for the the MoveState you just created and return the new MovementState script
        switch (state)
        {
            case MovementState.MoveState.jumping:
                return jumping;
            //case MovementState.MoveState.walking:
                //return walking;
            default:
                return null;    //if the code gets here, a MovementScript-MoveState pair is not added
        }
    }
}
