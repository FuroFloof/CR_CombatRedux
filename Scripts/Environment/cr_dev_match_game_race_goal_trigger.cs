using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cr_dev_match_game_race_goal_trigger : cr_MonoBehavior
{
    public cr_dev_match_game_race raceManager;
    [SerializeField] Transform _spawnPoint;
    public Transform spawnPoint
    {
        get
        {
            if (!_spawnPoint) return transform;
            return _spawnPoint;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collider entered");
        if (!raceManager.Object) return;
        if (!raceManager.Object.HasStateAuthority) return;
        
        Debug.Log("We have state auth");
    
        if (other.gameObject.layer == LayerMask.NameToLayer("RemotePlayer"))
        {
            Debug.Log("Layer is Remote");
            
            if(other.gameObject.TryGetComponent<cr_player_reference>(out var reference))
            {
                Debug.Log("Object is remote player");
                
                raceManager.PlayerEnteredGoal(reference.player.playerId);
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
        {
            Debug.Log("Layer is Local");
        
            if(other.gameObject.TryGetComponent<cr_player_physics_head>(out var reference))
            {
                Debug.Log("Object is local player");
            
                //i know this is ass, shut up
                raceManager.PlayerEnteredGoal(reference.transform.parent.parent.gameObject.GetComponent<cr_player_api>().playerId);
            }
        }
    }
}
