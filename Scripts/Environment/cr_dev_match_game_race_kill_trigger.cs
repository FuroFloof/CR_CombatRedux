using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_dev_match_game_race_kill_trigger : cr_MonoBehavior
{
    public cr_dev_match_game_race raceManager;

    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collider entered");
        if (!raceManager.Object) return;
        if (!raceManager.Object.HasStateAuthority) return;
        
        //Debug.Log("We have state auth");
    
        if (collision.gameObject.layer == LayerMask.NameToLayer("RemotePlayer"))
        {
            //Debug.Log("Layer is Remote");
            
            if(collision.gameObject.TryGetComponent<cr_player_reference>(out var reference))
            {
                //Debug.Log("Object is player");
                
                raceManager.PlayerCollidedWithDeath(reference.player.playerId);
            }
        }
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
        {
            //Debug.Log("Layer is Local");
        
            if(collision.gameObject.TryGetComponent<cr_player_physics_head>(out var reference))
            {
                Debug.Log("Object is player");
            
                //i know this is ass, shut up
                raceManager.PlayerCollidedWithDeath(reference.transform.parent.parent.gameObject.GetComponent<cr_player_api>().playerId);
            }
        }
    }
}
