using UnityEngine;

public class Attacher : MonoBehaviour
{
    public GameObject attachTo;

    void Update()
    {
        transform.position = attachTo.transform.position;
    }
}
