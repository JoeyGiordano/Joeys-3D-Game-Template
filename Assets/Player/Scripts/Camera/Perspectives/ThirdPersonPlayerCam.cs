using UnityEngine;

/// <summary>
/// Manages all 3rd person player cameras. Provides methods to switch between 3rd person player cameras.
/// The ThirdPersonPlayerCam gameobject stores the cinemachine brain and all the virtual cameras.
/// To turn off third person camera, just deactivate the ThirdPersonPlayerCam gameobject (no additional method calls needed - DeactivateAllVCams() is NOT used to deactivate the 3rd person cam)
/// 
/// To add more 3rd person player cams,
///  - just a add a new object under ThirdPersonPlayerCam
///  - add a virtual cam (do not try to use a regular Camera, if you have not used Cinemachine before, look it up, its easy)
///  - add scripts to correctly position the virtual cam
///  - add scripts to set the player orientation
///  - edit this script
///    * add a reference to the new object
///    * add a value to the CameraStyle enum
///    * add an if statement in SetCameraStyle()
///    * add a line in DeactivateAllVCams()
/// </summary>
public class ThirdPersonPlayerCam : MonoBehaviour
{
    Camera cam; //for reference of outside scripts

    public CameraStyle currentStyle;

    [Header("VCam Objects")]
    public GameObject basicCam;
    public GameObject lockedBehindCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    public enum CameraStyle
    {
        Basic,
        LockedBehind,
        Combat,
        TopDown
    }

    private void Start()
    {
        SetCameraStyle(currentStyle);
        cam = GetComponentInChildren<Camera>();
    }

    public void SetCameraStyle(CameraStyle newStyle)
    {
        DeactivateAllVCams();

        if (newStyle == CameraStyle.Basic) basicCam.SetActive(true);
        if (newStyle == CameraStyle.LockedBehind) lockedBehindCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);
        if (newStyle == CameraStyle.TopDown) topDownCam.SetActive(true);

        currentStyle = newStyle;
    }

    /// <summary>
    /// Do not use this method to turn off the 3rd person camera, instead deactivate the 3rd person camera GameObject.
    /// This method does not turn off the 3rd person camera cinemachine brain. 
    /// </summary>
    private void DeactivateAllVCams()
    {
        basicCam.SetActive(false);
        lockedBehindCam.SetActive(false);
        combatCam.SetActive(false);
        topDownCam.SetActive(false);
    }

}
