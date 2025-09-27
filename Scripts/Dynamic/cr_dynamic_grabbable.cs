using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class cr_dynamic_grabbable : cr_NetworkBehavior
{
    public Rigidbody rb;
    public bool isGrabbing => currentHand != null;
    public cr_player_hand currentHand;

    public float kp = 20f;          // position gain
    public float kd = .8f;         // velocity damping
    public float maxSpeed = 15f;
    public float kff = 0.6f;     // feed-forward from hand velocity
    public float maxAccel = 140f;

    Vector3 _lastHandPos;
    
    
    public bool GrabRequest(cr_player_hand hand)
    {
        if(currentHand == null)
        {
            currentHand = hand;
            return true;
        }else
        {
            currentHand.EndGrab();
            currentHand = hand;
            return false;
        }
    }
    

    // Tunables
    

    void FixedUpdate()
    {
        if (currentHand == null) return;
        
        if (currentHand.DynamicGrabPoint == null)
        {
            currentHand = null;
            return;
        }

        float dt = Time.fixedDeltaTime;

        Vector3 objPos  = rb.position;
        Vector3 handPos = currentHand.transform.position;

        // Error: object should move to the hand
        Vector3 err = handPos - objPos;

        // Hand velocity for feed-forward
        Vector3 handVel = (handPos - _lastHandPos) / Mathf.Max(dt, 1e-5f);
        _lastHandPos = handPos;

        // Target object velocity: pull toward hand, match some of hand velocity, damp own velocity
        Vector3 targetVel = kp * err + kff * handVel - kd * rb.velocity;

        // Speed clamp
        if (targetVel.magnitude > maxSpeed)
            targetVel = targetVel.normalized * maxSpeed;

        // Apply with accel clamp so it feels strong but stable
        Vector3 dv = targetVel - rb.velocity;
        dv = Vector3.ClampMagnitude(dv, maxAccel * dt);

        rb.velocity += dv;
    }

}
