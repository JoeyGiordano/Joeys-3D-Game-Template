using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CutToFirstPersonCam();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CutToThirdPersonCam();
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
}
