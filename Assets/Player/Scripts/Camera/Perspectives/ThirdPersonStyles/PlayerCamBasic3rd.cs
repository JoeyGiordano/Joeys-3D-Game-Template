using UnityEngine;

/// <summary>
/// Locks the camera to the vertical plane containing the player orientation direction (while moving but allows free rotation while no input)
/// by rotating the orientation so that the camera view and orientation are pointed in the same direction
/// but allows the player model to rotate towards the input direction without affecting the camera angle
/// ie using awsd without touching the mouse will change the orientation without changing the camera view
/// When using this more I reccomend that you make all movement forces (forward, strafe, back) the same in AWSD movement
/// </summary>
public class PlayerCamBasic3rd : MonoBehaviour
{
    MovementResources moveRes;

    [Header("References")]
    public Transform brain;
    public Transform player;
    public Transform orientation;
    public Transform playerModel;

    public float rotationSpeed = 7;

    private void Start()
    {
        moveRes = player.gameObject.GetComponent<MovementResources>();
    }

    void Update()
    {
        RotatePlayerOrientation();
        RotatePlayerModel();
    }

    private void RotatePlayerOrientation()
    {
        Vector3 viewDirection = player.position - brain.position;
        viewDirection.y = 0;
        orientation.forward = viewDirection.normalized;
    }

    private void RotatePlayerModel()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        inputDir = moveRes.movementInputDirection;

        //rotate the player towards its movement input direction (s is facing and walking towards the camera, not walking towards the cam and facing away) 
        if (inputDir != Vector3.zero)
            playerModel.forward = Vector3.Slerp(playerModel.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

    }
}
