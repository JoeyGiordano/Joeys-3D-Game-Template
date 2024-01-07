using UnityEngine;

public class Grapple : MovementState
{
    int grappleMouseButton = 0;

    [Header("Settings")]
    public float grappleRange = 25f;
    [Range(0, 1)]
    public float pullDuringSwing = 0.1f;
    public float extraFallingForce = 10;
    public float releaseForce = 6;
    
    [Header("References")]
    public GameObject grappleGun;
    LineRenderer lr;
    PlayerCam cam;

    //for calculations
    [HideInInspector] public Vector3 swingPoint;
    private SpringJoint joint;

    public override void OnStartup()
    {
        lr = grappleGun.GetComponent<LineRenderer>();
        cam = MoveRes.playerCam;
    }

    public override MoveState getMyState()
    {
        return MoveState.grappling;
    }

    public override bool EnterCondition()
    {
        //if the correct button is pressed
        if (Input.GetMouseButtonDown(grappleMouseButton))
        {
            RaycastHit hit;
            //if a raycast in the camera's facing direction hits a close enough grabbleable surface
            if (Physics.Raycast(MoveRes.eyes.position, MoveRes.facingDirection, out hit, grappleRange, MoveRes.terrainLayer))
            {
                swingPoint = hit.point;
                return true;
            }
        }
        return false;
    }

    public override bool UseAWSD()
    {
        return false;
    }

    public override void OnEnter(MoveState previousState)
    {
        //create joint
        joint = player.AddComponent<SpringJoint>();

        //attach joint to swing point (it is already attached to player)
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        //set range that the joint will try to maintain
        float distanceFromSwingPoint = Vector3.Distance(player.transform.position, swingPoint);
        joint.maxDistance = distanceFromSwingPoint * (1 - pullDuringSwing);
        joint.minDistance = 0;

        //set joint constants
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        //prepare line renderer (for showing the rope)
        lr.positionCount = 2;

        //change the FOV
        cam.SetFirstPersonFOV(90f, 0.25f);

    }

    public override void WhileActive()
    {
        MoveRes.ApplyExtraFallingForce(extraFallingForce);
    }

    public override bool ExitCondition()
    {
        if (Input.GetMouseButtonUp(grappleMouseButton))
            return true;
        return false;
    }

    public override MoveState OnExit()
    {
        DissembleGrapple();
        return MoveState.free;
    }

    public override void OnOverriden()
    {
        DissembleGrapple();
    }

    public override void OnReset()
    {
        //do nothing
    }

    public override void UpdateChild()
    {
        //do nothing
    }

    //non override methods
    private void DissembleGrapple()
    {
        //reset FOV
        cam.SetFirstPersonFOV(cam.firstPersonCamScript.normalFOV, 0.25f);

        //stop showing the rope
        lr.positionCount = 0;

        //remove the joint component
        Destroy(joint);

        rb.AddForce(releaseForce * rb.velocity.normalized, ForceMode.Impulse);

    }

    void DrawRope()
    {
        //move the line renderer
        lr.SetPosition(0, grappleGun.transform.position);
        lr.SetPosition(1, swingPoint);
    }

    private void LateUpdate()
    {
        if (active) DrawRope();
    }
}
