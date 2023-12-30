using UnityEngine;

public class ThirdPersonPlayerCam : MonoBehaviour
{
    public CameraStyle currentStyle;

    [Header("Cameras")]
    public GameObject basicCam;
    public GameObject lockedBehindCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    public enum CameraStyle
    {
        Basic,
        LockedBehind,
        Combat,
        Topdown
    }

    private void Start()
    {
        SetCameraStyle(currentStyle);
    }

    public void SetCameraStyle(CameraStyle newStyle)
    {
        DeactivateAllVCams();

        if (newStyle == CameraStyle.Basic) basicCam.SetActive(true);
        if (newStyle == CameraStyle.LockedBehind) lockedBehindCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);

        currentStyle = newStyle;
    }

    private void DeactivateAllVCams()
    {
        basicCam.SetActive(false);
        lockedBehindCam.SetActive(false);
        combatCam.SetActive(false);
        topDownCam.SetActive(false);
    }
}
