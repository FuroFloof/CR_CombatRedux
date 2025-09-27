using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class cr_player_spawner : cr_MonoBehavior
{
    public NetworkPrefabRef PlayerPrefab;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined");
        if (!cr_networking.fusionRunner.IsServer) return;

        var spawnPos = Vector3.zero;
        var obj = cr_networking.fusionRunner.Spawn(PlayerPrefab, spawnPos, Quaternion.identity, player);

        // Register the association inside Fusion
        cr_networking.fusionRunner.SetPlayerObject(player, obj);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left");
        if (!cr_networking.fusionRunner.IsServer) return;

        if (cr_networking.fusionRunner.TryGetPlayerObject(player, out var obj))
        {
            cr_networking.fusionRunner.Despawn(obj);
            cr_networking.fusionRunner.SetPlayerObject(player, null); // tidy
        }
    }
    
    public void CleanUp()
    {
        PlayerObjects.Clear();
    }

    
    
    
    public static readonly Dictionary<PlayerRef, NetworkObject> PlayerObjects = new();
    
    
    public NetworkObject GetPlayerObjectByPlayerId(int playerId)
    {
        // Convert int to PlayerRef by scanning active players
        foreach (var pref in cr_networking.fusionRunner.ActivePlayers)
        {
            if (pref.PlayerId == playerId)
                return GetPlayerObjectByPlayerRef(pref);
        }
        return null;
    }
    
    public NetworkObject[] GetAllPlayerObjects()
    {
        List<NetworkObject> result = new List<NetworkObject>();
        foreach(var pref in cr_networking.fusionRunner.ActivePlayers)
        {
            var obj = cr_networking.fusionRunner.GetPlayerObject(pref);
            if (!obj) continue;
            result.Add(obj);
        }
        return result.ToArray();
    }
    
    public NetworkObject GetPlayerObjectByPlayerRef(PlayerRef player)
    {
        return cr_networking.fusionRunner.TryGetPlayerObject(player, out var obj) ? obj : null;
    }
    
    
    
    
    
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        CleanUp();
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        CleanUp();
    }
}
