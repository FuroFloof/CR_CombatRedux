using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class cr_MatchBehavior : cr_NetworkBehavior
{


    void Update()
    {
        if (!Object) return;
        if (!Object.IsValid) return;
        if (Object.HasStateAuthority) StateAuthorityTick();

        CommonPlayerTick();
    }

    public virtual void StateAuthorityTick() { }
    public virtual void CommonPlayerTick() { }


    public override void Spawned()
    {
        UpdatePlayerList();
    }


    public cr_match_behavior_spawn_manager SpawnManager;
    public cr_MatchBehavior_spawn_behaviors SpawnType;
    public cr_MatchBehavior_match_type MatchType;
    
    public Transform GetGlobalSpawnPoint(cr_player_api player)
    {
        if (!Object || !Object.HasStateAuthority) return null;
        
        return SpawnManager.GetMatchSpawnForPlayer(player).SpawnPoint;
    }
    
    
    public void DistrobutePlayer(int playerID)
    {
        if (!Object.HasStateAuthority) return;
        
        Transform T = GetGlobalSpawnPoint(cr_networking.Instance.GetPlayerByID(playerID));

        RPC_MovePlayerToTarget(T.position, T.forward, playerID);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_MovePlayerToTarget(Vector3 position, Vector3 forward, int playerTarget)
    {
        var localPlayer = cr_networking.localPlayer;
        
        if (playerTarget != localPlayer.playerId) return; //if it's not us, we do not care

        localPlayer.playerInput.headPhys.TeleportHead(position, forward, true);
        
    }
    

    public cr_player_api[] playersInMatch;
    public void UpdatePlayerList()
    {
        Debug.Log("Updating Player List");
        playersInMatch = cr_networking.Instance.GetAllPlayers();
    }
    public override void _OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        base._OnPlayerJoined(runner, player);

        UpdatePlayerList();

        DistrobutePlayer(player.PlayerId);
    }
    public override void _OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        base._OnPlayerLeft(runner, player);

        UpdatePlayerList();
    }
}

public enum cr_MatchBehavior_spawn_behaviors
{
    Random,
    RoundRobin
}

public enum cr_MatchBehavior_match_type
{
    FreeForAll,
    Teams
}