using UnityEngine;
using static ThirdPersonPlayerCam;

/// <summary>
/// Manages all player cameras through the first and third person camera scripts.
/// To add more third person camera styles see the ThirdPersonPlayerCam script.
/// You can turn player camera off by deactivating the PlayerCam object (no additional method calls needed)
/// If adding non-player cameras
///  - do not use the third person cam cinemachine brain to manage external virtual cameras (either make one outside and delete the one inside or make two and have one activated at a time)
///  - move the Cursor settings lines to your camera manager script
///  - you may have to add scripts to make sure the player orientation responds to input
/// The input based camera switching in Update() is just for testing, feel free to delete it.
/// </summary>
public class PlayerCam : MonoBehaviour
{
    [Header("Camera Objects")]
    GameObject firstPersonCam;
    GameObject thirdPersonCam;

    [Header("Camera Scripts")]
    [HideInInspector]
    public FirstPersonPlayerCam firstPersonCamScript;
    [HideInInspector]
    public ThirdPersonPlayerCam thirdPersonCamScript;

    [Header("Active Camera")]
    [SerializeField]
    private bool startFirstPerson;
    [HideInInspector]
    public bool firstPerson;

    private void Start()
    {
        //make the mouse invisible, if adding more camera stuff move these two lines to your camera manager script 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //get references
        firstPersonCamScript = GetComponentInChildren<FirstPersonPlayerCam>();
        firstPersonCam = firstPersonCamScript.gameObject;
        thirdPersonCamScript = GetComponentInChildren<ThirdPersonPlayerCam>();
        thirdPersonCam = thirdPersonCamScript.gameObject;

        //activate the correct cam
        if (startFirstPerson) CutToFirstPersonCam();
        else CutToThirdPersonCam();
    }

    void Update()
    {
        //input camera switch, just for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CutToFirstPersonCam();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CutToThirdPersonCam();
            thirdPersonCamScript.SetCameraStyle(CameraStyle.Basic);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CutToThirdPersonCam();
            thirdPersonCamScript.SetCameraStyle(CameraStyle.LockedBehind);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CutToThirdPersonCam();
            thirdPersonCamScript.SetCameraStyle(CameraStyle.Combat);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CutToThirdPersonCam();
            thirdPersonCamScript.SetCameraStyle(CameraStyle.TopDown);
        }
    }

    public void CutToFirstPersonCam()
    {
        firstPersonCam.SetActive(true);
        thirdPersonCam.SetActive(false);
        firstPerson = true;
    }
    public void CutToThirdPersonCam()
    {
        thirdPersonCam.SetActive(true);
        firstPersonCam.SetActive(false);
        firstPerson = false;
    }

    public void SetFirstPersonFOV(float fov, float transitionTime)
    {
        bool camActive = firstPersonCam.activeInHierarchy;
        firstPersonCam.SetActive(true);
        firstPersonCamScript.SetFOV(fov, transitionTime);
        firstPersonCam.SetActive(camActive);
    }

    public void SetFirstPersonTilt(float tilt, float transitionTime)
    {
        bool camActive = firstPersonCam.activeInHierarchy;
        firstPersonCam.SetActive(true);
        firstPersonCamScript.SetTilt(tilt, transitionTime);
        firstPersonCam.SetActive(camActive);
    }
}
