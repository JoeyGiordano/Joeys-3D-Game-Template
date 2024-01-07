using UnityEngine;

/// <summary>
/// Locks the camera to the vertical plane containing the player's view direction
/// by rotating the playerModel and orientation so that the orientation and camera view are pointed in the same XZ direction
/// ie must stop and turn around to look behind yourself
/// </summary>
public class PlayerCamLockedBehind : MonoBehaviour
{
    [Header("References")]
    public Transform brain;
    public Transform player;
    public Transform orientation;
    public Transform playerModel;

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
        playerModel.forward = orientation.forward;
    }
}
