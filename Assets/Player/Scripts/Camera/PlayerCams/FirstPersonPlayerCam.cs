using UnityEngine;
using DG.Tweening;

public class FirstPersonPlayerCam : MonoBehaviour
{
    private GameObject camObj;
    private Camera cam;

    [Header("References")]
    public Transform orientation;
    public Transform playerModel;

    [Header("Cam Settings")]
    public float xSensitivity;
    public float ySensitivity;
    [SerializeField]
    private float fov = 80f;

    float xRotation;
    float yRotation;

    float rotation = 0;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        camObj = cam.gameObject;
        SetFOV(fov);
    }

    void Update()
    {
        //Get Mouse Input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity * 100f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity * 100f;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //Prevents looking too far up or down

        //Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        playerModel.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetFOV(float fov)
    {
        cam.fieldOfView = fov;
    }
    public void SetFOV(float fov, float transitionTime)
    {
        cam.DOFieldOfView(fov, transitionTime);
    }
    public void SetRotation(Vector3 rotation, float transitionTime)
    {
        camObj.transform.DOLocalRotate(rotation, transitionTime);
    }
}