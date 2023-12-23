using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public Transform objToTrack;

    // Update is called once per frame
    void Update()
    {
        transform.position = objToTrack.transform.position;
    }
}
