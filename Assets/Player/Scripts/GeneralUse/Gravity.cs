using UnityEngine;

/// <summary>
/// Applies gravity using rigidbody forces.
/// </summary>
public class Gravity : MonoBehaviour
{
    //References
    Rigidbody rb;

    [Header("World Gravity")]
    public static float worldGravity = 30; //the same number for all objects
    public bool useWorldGravity = false;

    [Header("Settings")]
    public float normalGravity = 37;
    public float gravity = 37;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (useWorldGravity) rb.AddForce(worldGravity * Vector3.down, ForceMode.Force);
        else rb.AddForce(gravity * Vector3.down, ForceMode.Force);
    }

    public void SetGravity(float gravity)
    {
        this.gravity = gravity;
    }

    public void ResetGravity()
    {
        gravity = normalGravity;
    }


}
