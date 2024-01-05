using UnityEngine;

/// <summary>
/// Makes this object follow objToTrack exactly. (childing without the consequences) 
/// </summary>
public class Track : MonoBehaviour
{
    public Transform objToTrack;
    public Vector3 offset = Vector3.zero;

    void Update()
    {
        transform.position = objToTrack.transform.position + offset;
    }
}
