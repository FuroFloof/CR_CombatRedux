using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_ui_local_look_at_camera : cr_MonoBehavior
{
    public bool lock_X;
    public bool lock_Y;
    public bool lock_Z;


    Transform localCam;

    public void Start()
    {
        //Debug.Log("LookAtCam enabled", gameObject);
        localCam = cr_networking.localPlayer.playerInput.Head.transform;
    }


    
    public void LateUpdate()
    {
        //Debug.Log("LookAtCam LateUpdate", gameObject);
        
        transform.LookAt(localCam.position);

        Vector3 headRot = transform.rotation.eulerAngles;
        
        if (lock_X) transform.eulerAngles = new Vector3(0, headRot.y, headRot.z);
        if (lock_Y) transform.eulerAngles = new Vector3(headRot.x, 0, headRot.z);
        if (lock_Z) transform.eulerAngles = new Vector3(headRot.x, headRot.y, 0);
    }
}
