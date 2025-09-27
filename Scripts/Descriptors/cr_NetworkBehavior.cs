using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class cr_NetworkBehavior : NetworkBehaviour, INetworkRunnerCallbacks
{
    void Awake()
    {
        AddToRunnerCallback();
    }

    void Start()
    {
        AddToRunnerCallback();
    }

    void OnEnable() 
    {
        AddToRunnerCallback();
    }
    
    public void AddToRunnerCallback()
    {
        var runner = cr_networking.fusionRunner;

        if (!runner)
        {
            Debug.LogError("FusionRunner not found from cr_networking", gameObject);
            return;
        }
    
        runner.AddCallbacks(this);
    }
    
    void OnDisable() 
    {
        // var runner = cr_networking.fusionRunner;

        // if (!runner) return;
    
        // runner.RemoveCallbacks(this);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public virtual void _OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        _OnPlayerJoined(runner, player);
    }
    
    public virtual void _OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        _OnPlayerLeft(runner, player);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    
    }

    public cr_Behavior behavior;
}
