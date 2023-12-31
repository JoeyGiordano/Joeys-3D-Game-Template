using UnityEngine;

public class StairClimb : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepPush = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //stepClimb();
    }

    void stepClimb()
    {
        RaycastHit hit;
        if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hit, 0.7f))
        {
            transform.position += new Vector3(0f, stepPush * Time.deltaTime, 0f);
            //rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
        }

        RaycastHit hitRight45;
        if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitRight45, 0.2f))
        {
            transform.position += new Vector3(0f, stepPush * Time.deltaTime, 0f);
            //rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
        }

        RaycastHit hitLeft45;
        if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLeft45, 0.2f))
        {
            transform.position += new Vector3(0f, stepPush * Time.deltaTime, 0f);
            //rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
        }
    }
}