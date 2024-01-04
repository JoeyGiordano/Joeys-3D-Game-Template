using UnityEngine;
using static ThirdPersonPlayerCam;

public class PlayerCam : MonoBehaviour
{
    GameObject firstPersonCam;
    GameObject thirdPersonCam;

    [HideInInspector]
    public FirstPersonPlayerCam firstPersonCamScript;
    [HideInInspector]
    public ThirdPersonPlayerCam thirdPersonCamScript;

    [SerializeField]
    private bool startFirstPerson;
    [HideInInspector]
    public bool firstPerson;

    private void Start()
    {
        //make the mouse invisible
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
        //input camera switch
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
            thirdPersonCamScript.SetCameraStyle(CameraStyle.Topdown);
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
