using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("---------Camera Settings---------")]
    public float FollowSpeed;
    public float YOffset;
    public Transform Target;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(Target.position.x, Target.position.y + YOffset, -10f);
        transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
    }
}
