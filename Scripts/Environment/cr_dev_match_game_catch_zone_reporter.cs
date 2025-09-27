using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_dev_match_game_catch_zone_reporter : cr_MonoBehavior
{
    public cr_dev_match_game_catch catchManager;
    
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collider entered");
        
        if (!catchManager.HasStateAuthority) return;
        
        //Debug.Log("We have state auth");
    
        if (other.gameObject.layer == LayerMask.NameToLayer("RemotePlayer"))
        {
            //Debug.Log("Layer is Remote");
            
            if(other.gameObject.TryGetComponent<cr_player_reference>(out var reference))
            {
                //Debug.Log("Object is player");
                
                catchManager.OnPlayerEnterCaptureZone(reference.player.playerId);
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
        {
            //Debug.Log("Layer is Local");
        
            if(other.gameObject.TryGetComponent<cr_player_physics_head>(out var reference))
            {
                //Debug.Log("Object is player");
            
                //i know this is ass, shut up
                catchManager.OnPlayerEnterCaptureZone(reference.transform.parent.parent.gameObject.GetComponent<cr_player_api>().playerId);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!catchManager.HasStateAuthority) return;
    
        if (other.gameObject.layer == LayerMask.NameToLayer("RemotePlayer"))
        {
            if(other.gameObject.TryGetComponent<cr_player_reference>(out var reference))
            {
                catchManager.OnPlayerExitCaptureZone(reference.player.playerId);
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
        {
            if(other.gameObject.TryGetComponent<cr_player_physics_head>(out var reference))
            {
                catchManager.OnPlayerExitCaptureZone(reference.transform.parent.parent.gameObject.GetComponent<cr_player_api>().playerId);
            }
        }
    }
}
