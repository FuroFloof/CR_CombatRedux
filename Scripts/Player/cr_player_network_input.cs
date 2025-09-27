using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct cr_net_vr_input : INetworkInput {
    public Vector3 headPos;
    public Quaternion headRot;
    public Vector3 leftPos;
    public Quaternion leftRot;
    public Vector3 rightPos;
    public Quaternion rightRot;
    public float leftGrip;
    public float rightGrip;
    
}


public class cr_player_network_input : cr_NetworkBehavior, INetworkRunnerCallbacks
{
    new public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        cr_networking.localPlayer.playerInput.OnInput(runner, input);
    }
    
    [Networked] public Vector3 HeadPos { get; set; }
    [Networked] public Quaternion HeadRot { get; set; }
    [Networked] public Vector3 LeftPos { get; set; }
    [Networked] public Quaternion LeftRot { get; set; }
    [Networked] public Vector3 RightPos { get; set; }
    [Networked] public Quaternion RightRot { get; set; }
    [Networked] public float LeftGrip { get; set; }
    [Networked] public float RightGrip { get; set; }

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && GetInput<cr_net_vr_input>(out var inp)) 
        {
            HeadPos  = inp.headPos;
            HeadRot  = inp.headRot;
            LeftPos  = inp.leftPos;
            LeftRot  = inp.leftRot;
            RightPos = inp.rightPos;
            RightRot = inp.rightRot;
            LeftGrip = inp.leftGrip;
            RightGrip = inp.rightGrip;
        }

    }
    
    
    
    public void ForceVisualUpdate(cr_net_vr_input manualInp) // called by input class on at update
    {
        head.SetPositionAndRotation(manualInp.headPos, manualInp.headRot);
        leftHand.SetPositionAndRotation(manualInp.leftPos, manualInp.leftRot);
        rightHand.SetPositionAndRotation(manualInp.rightPos, manualInp.rightRot);
        
        _headVel = _leftVel = _rightVel = Vector3.zero;
    }

    

    public override void Render() {
    
        if (Object.HasInputAuthority) return;

        //InterpMovement();

        // head.SetPositionAndRotation(HeadPos, HeadRot);
        // leftHand.SetPositionAndRotation(LeftPos, LeftRot);
        // rightHand.SetPositionAndRotation(RightPos, RightRot);
    }

    void LateUpdate()
    {
        if (Object.HasInputAuthority) return;

        InterpMovement();
    }


    void InterpMovement() // apply interpolation
    {
        //if (Object.HasInputAuthority) return;
    
        float dt = Time.deltaTime;

        // HEAD
        SmoothTransform(
            head,
            HeadPos, HeadRot,
            ref _headVel,
            dt,
            true
        );

        // LEFT HAND
        SmoothTransform(
            leftHand,
            LeftPos, LeftRot,
            ref _leftVel,
            dt
        );

        // RIGHT HAND
        SmoothTransform(
            rightHand,
            RightPos, RightRot,
            ref _rightVel,
            dt
        );
    }
    
    
    public float positionSmoothTime = 0.06f;
    public float rotationLerpSpeed = 20f;
    public float maxSnapDistance = 2f;
    Vector3 _headVel, _leftVel, _rightVel;

    void SmoothTransform(Transform t, Vector3 targetPos, Quaternion targetRot, ref Vector3 vel, float dt, bool isHead = false)
    {
        if(isHead)
        {
            Vector3 TP = t.position;
            
            // Debug.Log($"Transform Position: {TP}");
            // Debug.Log($"Target Position   : {targetPos}");
        }
        
    
        // Distance check for snap on teleports or big corrections
        float dist = Vector3.Distance(t.position, targetPos);
        if (dist > maxSnapDistance || !t.gameObject.activeInHierarchy)
        {
            t.SetPositionAndRotation(targetPos, targetRot);
            vel = Vector3.zero; // clear velocity after snap
            return;
        }
        
        Vector3 newPos = Vector3.SmoothDamp(
            t.position,
            targetPos,
            ref vel,
            Mathf.Max(0.0001f, positionSmoothTime),
            Mathf.Infinity,
            dt
        );
        
        float rotT = 1f - Mathf.Exp(-rotationLerpSpeed * dt);
        Quaternion newRot = Quaternion.Slerp(t.rotation, targetRot, rotT);

        t.SetPositionAndRotation(newPos, newRot);
    }
}
