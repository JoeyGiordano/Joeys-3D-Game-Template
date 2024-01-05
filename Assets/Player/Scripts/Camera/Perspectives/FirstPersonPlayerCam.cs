using UnityEngine;
using System.Collections;

/// <summary>
/// Positions and orients the First Person Camera. Based on the linked youtube video
/// https://www.youtube.com/watch?v=f473C43s8nE&ab_channel=Dave%2FGameDevelopment
/// Some things are different though so be careful. (also note the organization in the hirearchy)
/// </summary>
public class FirstPersonPlayerCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerModel;
    private GameObject camObj;
    private Camera cam;

    [Header("Cam Settings")]
    public float xSensitivity;
    public float ySensitivity;
    public float normalFOV = 80f;

    //camera orientation direction storage
    float xRotation;
    float yRotation;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        camObj = cam.gameObject;
        SetFOV(normalFOV);
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

    //the following methods could be done using DOTween library (like in video), but I used lerp so you don't have to download DOTween
    //if you're going to add more camera effects, use Cinemachine or DOTween (Cinemachine is probably better, but up to you)
    public void SetFOV(float fov)
    {
        cam.fieldOfView = fov;
    }
    public void SetFOV(float fov, float transitionTime)
    {
        if (transitionTime == 0) { SetFOV(fov); return; }
        StartCoroutine(LerpFOV(fov, transitionTime));
    }
    public void SetTilt(float tilt)
    {
        camObj.transform.localRotation = Quaternion.Euler(0, 0, tilt);
    }
    public void SetTilt(float tilt, float transitionTime)
    {
        if (transitionTime == 0) { SetFOV(tilt); return; }
        StartCoroutine(LerpTilt(tilt, transitionTime));
    }

    private IEnumerator LerpFOV(float endfov, float transitionTime)
    {
        float time = 0;
        float startValue = cam.fieldOfView;

        while (time < transitionTime)
        {
            cam.fieldOfView = Mathf.Lerp(startValue, endfov, Sigmoid(time/transitionTime));
            time += Time.deltaTime;
            yield return null;
        }

        cam.fieldOfView = endfov;

    }

    private IEnumerator LerpTilt(float endTilt, float transitionTime)
    {
        float time = 0;
        Quaternion startQaut = camObj.transform.localRotation;
        float startEulersZ = startQaut.eulerAngles.z;
        if (startEulersZ > 180) startEulersZ -= 360;
        if (startEulersZ < -180) startEulersZ += 360;

        while (time < transitionTime)
        {
            float currentTilt = Mathf.Lerp(startEulersZ, endTilt, Sigmoid(time / transitionTime));
            camObj.transform.localRotation = Quaternion.Euler(0, 0, currentTilt);
            time += Time.deltaTime;
            yield return null;
        }

        camObj.transform.localRotation = Quaternion.Euler(0, 0, endTilt);
    }

    private float Sigmoid(float x)
    {
        if (x == 0) return 0;
        if (x == 1) return 1;
        return 1 / (1 + Mathf.Exp(-13 * (x - 0.5f)));   //graph is desmos to see what it looks like
    }

}