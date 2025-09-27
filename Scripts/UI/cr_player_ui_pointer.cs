using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_ui_pointer : MonoBehaviour
{
    public cr_player_input_raw left;
    public cr_player_input_raw right;

    GameObject currentActivePointer;
    public GameObject LeftPointer;
    public GameObject RightPointer;

    public Transform leftPointerParent;
    public Transform rightPointerParent;

    public Vector3 pointerPositionOffset;
    public Vector3 pointerRotationOffset;

    void Start()
    {
        currentActivePointer = RightPointer;
    }

    void Update()
    {
        GameObject go = currentActivePointer;
        
        if (left.GetTriggerValue() > .5f && right.GetTriggerValue() < .5f) go = LeftPointer;
        
        if (right.GetTriggerValue() > .5f && left.GetTriggerValue() < .5f) go = RightPointer;

        if(go != currentActivePointer)
        {
            var oldPointer = go == LeftPointer ? RightPointer : LeftPointer;
            oldPointer.SetActive(false);
        }
        
        currentActivePointer = go;
        
        go.SetActive(true);
        
        go.transform.localPosition = pointerPositionOffset;
        go.transform.localEulerAngles = pointerRotationOffset;
    }
}
