using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_player_grab_motor : cr_MonoBehavior
{
    public cr_player_hand currentStaticGrabHand;
    public bool isGrabbing => currentStaticGrabHand != null;
    public cr_player_physics_head headPhys;

    public LayerMask RemotePlayerLayer;
    
    /// <summary>
    /// Request for current grab priority.
    /// </summary>
    /// <param name="hand"></param>
    /// <returns>Boolean: Were we grabbing before this request?</returns>
    public bool GrabRequest(cr_player_hand hand)
    {
        if(currentStaticGrabHand == null)
        {
            currentStaticGrabHand = hand;
            return true;
        }else
        {
            currentStaticGrabHand.EndGrab();
            currentStaticGrabHand = hand;
            return false;
        }
    }
    
    public void UnGrab(cr_player_hand hand)
    {
        float maxPushoffSpeed = cr_game_defaults.p_max_pushoff_speed;
        float grabPointSpeed = hand.StaticGrabPoint.speed;

        headPhys.rb.velocity = Vector3.ClampMagnitude(headPhys.rb.velocity, grabPointSpeed + maxPushoffSpeed);
    }

    

    public float kp = 40f;          // position gain
    public float kd = .8f;         // velocity damping
    public float maxSpeed = 15f;

    Vector3 _lastHandPos;

    void Update()
    {
        if (!isGrabbing || currentStaticGrabHand.StaticGrabPoint == null) return;

        float dt = Time.deltaTime;
        
        Vector3 anchor = currentStaticGrabHand.StaticGrabPoint.transform.position;
        Vector3 handPos = currentStaticGrabHand.transform.position;

        Vector3 err = anchor - handPos;                                   // how far from anchor
        Vector3 handVel = (handPos - _lastHandPos) / dt; // hand world velocity
        _lastHandPos = handPos;

        Vector3 targetVel = kp * err - kd * handVel; // snap toward anchor, resist fast hand swings

        if (targetVel.magnitude > maxSpeed) targetVel = targetVel.normalized * maxSpeed;

        // apply smoothly so you do not jerk it
        Vector3 dv = targetVel - headPhys.rb.velocity;
        float maxAccel = 120f; // m/s^2
        dv = Vector3.ClampMagnitude(dv, maxAccel * dt);

        headPhys.rb.velocity += dv;
    }

}
