using UnityEngine;

/// <summary>
/// Locks the camera to the vertical plane containing the player orientation direction
/// by rotating the orientation so that the camera view and orientation are pointed in the same direction
/// but allows the player model to rotate towards the input direction without affecting the camera angle
/// ie can look behind yourself while running
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
        //TODO^

        //rotate the player towards its movement input direction (s is walking towards the camera, not walking backwards) 
        if (inputDir != Vector3.zero)
            playerModel.forward = Vector3.Slerp(playerModel.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

    }
}
