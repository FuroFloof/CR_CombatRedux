using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class cr_player_input : cr_MonoBehavior
{
    public cr_player_input_raw[] _hands;
    public cr_player_input_raw leftHand => _hands[(int)cr_input_hand_type.left];
    public cr_player_input_raw rightHand => _hands[(int)cr_input_hand_type.right];
    public GameObject Head;
    public cr_player_network_input netInput;
    public cr_player_physics_head headPhys;

    public Vector3 GetLocalHeadPosition()
    {
        return Head.transform.position;
    }

    public Transform VisHead;
    public Vector3 GetRemoteHeadPosition()
    {
        return VisHead.position;
    }
    
    public cr_net_vr_input GetVRInput()
    {
        return new cr_net_vr_input 
        {
            headPos  = Head.transform.position,
            headRot  = Head.transform.rotation,
            leftPos  = leftHand.hand.GetCurrentGrabPointObject().position, //leftHand.gameObject.transform.position,
            leftRot  = leftHand.gameObject.transform.rotation,
            rightPos = rightHand.hand.GetCurrentGrabPointObject().position, //rightHand.gameObject.transform.position,
            rightRot = rightHand.gameObject.transform.rotation,
            leftGrip = leftHand.GetGripValue(),
            rightGrip = rightHand.GetGripValue()
        };
    }
    
    public cr_player_input_raw GetOppositeHandByType(cr_input_hand_type type)
    {
        switch (type)
        {
            case cr_input_hand_type.left:
                return rightHand;
            
            case cr_input_hand_type.right:
                return leftHand;
        }
        return leftHand;
    }
    
    public cr_player_input_raw GetCurrentGrabHandOrNull()
    {
        if (leftHand.hand.StaticGrabPoint) return leftHand;
        if (rightHand.hand.StaticGrabPoint) return rightHand;
        return null;
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var my = GetVRInput();
        
        input.Set(my);
    }

    void Update()
    {
        if (!netInput.HasInputAuthority) return;
        
        netInput.ForceVisualUpdate(GetVRInput());   
    }

}