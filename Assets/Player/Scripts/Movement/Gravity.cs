using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    //References
    MovementResources moveRes;
    Rigidbody rb;

    //static worldGravity variable?

    //Values
    private float normalGravity;
    public float gravity;

    private void Start()
    {
        moveRes = GetComponent<MovementResources>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        rb.AddForce(gravity * Vector3.down, ForceMode.Force);
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
