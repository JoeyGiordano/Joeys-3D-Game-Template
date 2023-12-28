using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam3rd : MonoBehaviour
{

    [Header("Player References")]
    public Transform orientation;
    public Transform player;
    public Transform playerModel;

    [Header("Other References")]
    public Transform brain;
    public Transform combatLookAt;

    [Header("Cameras")]
    public GameObject basicCam;
    public GameObject lockedBehindCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    [Header("Settings")]
    public float rotationSpeed;
    public CameraStyle currentStyle;

    public enum CameraStyle
    {
        Basic,
        LockedBehind,
        Combat,
        Topdown
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SwitchCameraStyle(currentStyle);
    }

    private void Update()
    {
        //Input
        ProcessStyleSwitchInput();

        //Rotations
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
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            //rotate the player towards its movement input direction (s is walking towards the camera, not walking backwards) 
            if (inputDir != Vector3.zero)
                SlerpPlayerModelRotationTo(inputDir.normalized);
        }

        else if (currentStyle == CameraStyle.LockedBehind)
        {
            playerModel.forward = orientation.forward;
        }

        
        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 combatViewDirection = player.position - brain.position;
            combatViewDirection.y = 0;
            orientation.forward = combatViewDirection.normalized;
            
            playerModel.forward = combatViewDirection.normalized;
        }
    }

    private void ProcessStyleSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.LockedBehind);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.Combat);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchCameraStyle(CameraStyle.Topdown);

    }

    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        basicCam.SetActive(false);
        lockedBehindCam.SetActive(false);
        combatCam.SetActive(false);
        topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) basicCam.SetActive(true);
        if (newStyle == CameraStyle.LockedBehind) lockedBehindCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);

        currentStyle = newStyle;
    }

    private void SlerpPlayerModelRotationTo(Vector3 targetRotation)
    {
        playerModel.forward = Vector3.Slerp(playerModel.forward, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
