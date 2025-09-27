using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class cr_player_hand : cr_MonoBehavior
{
    public cr_player_input input;
    public cr_player_input_raw handInput;
    public cr_player_grab_motor motor;
    public Rigidbody xrRB;

    public cr_player_static_grab_point StaticGrabPoint;
    public GameObject DynamicGrabPoint;

    public float grabRange = .125f;
    public float grabMaxDistance = 0.25f;
    public LayerMask grabLayers;

    public bool wantsToGrab;
    public bool canGrabEarly = true;


    void Update()
    {
        if (!cr_networking.localPlayer) return;

        bool IsDead = cr_networking.localPlayer.gameplayData.GetIsDead();
    
        if(handInput.GetGripDown() && !IsDead) //start grab
        {
            StartGrab();
            Debug.Log("Beginning Grab");
        }
        
        if((StaticGrabPoint || DynamicGrabPoint) && !handInput.GetGripPressed()) //no longer grabbing
        {
            EndGrab();
            Debug.Log("Ending Grab");
        }
        
        if(wantsToGrab && !(StaticGrabPoint || DynamicGrabPoint) && canGrabEarly && !IsDead) // early grab logic
        {
            StartGrab();
            Debug.Log("Attempting Early Grab");
            
        }else if(wantsToGrab && !canGrabEarly && !IsDead)
        {
            if (!handInput.GetGripPressed()) canGrabEarly = true;
        }

        if (IsDead && IsGrabbing()) EndGrab();
        
        wantsToGrab = handInput.GetGripPressed();
    }
    
    
    public void StartGrab()
    {
        Vector3 origin = transform.position;

        Collider[] collidersInHand = Physics.OverlapSphere(origin, grabRange, grabLayers);
        
        
        if(collidersInHand.Length > 0)
        {
            var col = collidersInHand[0];
        
            bool isDynamic = false;
            cr_dynamic_grabbable _dynamic = null;
            
            if(col.gameObject.TryGetComponent<cr_dynamic_grabbable>(out var dynamic))
            {
                isDynamic = true;
                _dynamic = dynamic;
            }else
            {
                isDynamic = false;
            }
            
            if(isDynamic) //dynamic moveable prop
            {
                GameObject grabPoint = new GameObject($"GRAB_POINT_{handInput.handType}");
                grabPoint.transform.position = transform.position;
                grabPoint.transform.SetParent(transform);
                DynamicGrabPoint = grabPoint;
                
                _dynamic.GrabRequest(this);
                
            }else //static "prop"
            {
                //Before anything, check if we're trying to grab another player's object.
                cr_player_api player = null;
                if(col.gameObject.TryGetComponent<cr_player_reference>(out var playerReference))
                {//we are trying to grab another player, ensure they are not already grabbing us, otherwise throw a fucking feud you piece of shit!
                    player = playerReference.player;
                }

                // Check if other player is grabbing us, if they are call a feud and end grab!
                var localPlayer = cr_networking.localPlayer;
                if(player)
                {// we are grabbing a player
                    if(player.GetCurrentGrabbingPlayer() == localPlayer.playerId)
                    {//player is grabbing us! call feud!!!
                        localPlayer.PlayerGrabFeud(player.playerId);
                        EndGrab();
                        Debug.Log("Feud Found! Calling end and reporting.");
                        return;
                    }
                    
                    //player is not grabbing us, so we update to current target player!

                    localPlayer.GrabbingPlayer(player.playerId, (int)handInput.handType);
                }
                
                GameObject grabPoint = new GameObject($"GRAB_POINT_{handInput.handType}");

                var gp = grabPoint.AddComponent<cr_player_static_grab_point>();
                
                grabPoint.transform.position = transform.position;
                grabPoint.transform.SetParent(col.transform);
                StaticGrabPoint = gp;
                
                motor.GrabRequest(this);
            }
            
            canGrabEarly = false;
        }
    }

    public void EndGrab()
    {
        motor.UnGrab(this);
    
        if (StaticGrabPoint)
        {
            Destroy(StaticGrabPoint);
            StaticGrabPoint = null;
            if(motor.currentStaticGrabHand == this) motor.currentStaticGrabHand = null;
        } 
        if (DynamicGrabPoint)
        {
            Destroy(DynamicGrabPoint);  
            DynamicGrabPoint = null;
        } 
        
        cr_networking.localPlayer.GrabbingPlayer(-1, (int)handInput.handType);
    }
    
    public void HandleGrab()
    {
        
    }
    
    public bool IsGrabbing()
    {
        if (DynamicGrabPoint) return true;
        if (StaticGrabPoint) return true;
        
        return false;
    }
    
    public Transform GetCurrentGrabPointObject()
    {
        if (StaticGrabPoint) return StaticGrabPoint.transform;
        if(DynamicGrabPoint) return DynamicGrabPoint.transform;
        
        return transform;
    }
    
    
    public Vector3 GetHandGrabOffset()
    {
        if(StaticGrabPoint)
        {
            return transform.position - StaticGrabPoint.transform.position;
            
        }else if(DynamicGrabPoint)
        {
            return DynamicGrabPoint.transform.position - transform.position;
        }
        
        return Vector3.zero;
    }
}